using Azure.Messaging.EventHubs;
using IoTHub.Models;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace IoTHub
{
    public class UpdateDeviceSensor
    {
        private readonly ILogger<UpdateDeviceSensor> _logger;

        public UpdateDeviceSensor(ILogger<UpdateDeviceSensor> logger)
        {
            _logger = logger;
        }

        [Function("UpdateDeviceSensor")]
        [CosmosDBOutput(
            databaseName: "%CosmosDb%",
            containerName: "%CosmosDbContainer%",
            Connection = "CosmosDBConnection")]
        public object? Run(
            [EventHubTrigger("%EventHubName%", Connection = "IoTHubConnection")] EventData[] events,
            [CosmosDBInput(
                databaseName: "%CosmosDb%",
                containerName: "%CosmosDbContainer%",
                Connection = "CosmosDBConnection")] Device[] devices)
        {
            if (devices == null || devices.Length == 0)
            {
                _logger.LogWarning("No devices found in the database.");
              
                return null;
            }

            if (events == null || events.Length == 0)
            {
                _logger.LogWarning("No events found in the Event Hub.");

                return null;
            }

            _logger.LogInformation($"Received {events.Length} events from Event Hub.");

            List<Device> updatedDevices = [];

            foreach (EventData @event in events)
            {
                // Decode the incoming telemetry message
                var messagePayload = Encoding.UTF8.GetString(@event.Body.ToArray());
                _logger.LogInformation($"Telemetry received: {messagePayload}");

                // Log custom properties.
                if (@event.Properties != null && @event.Properties.Count > 0)
                {
                    foreach (var prop in @event.Properties)
                    {
                        _logger.LogInformation($"Custom property: {prop.Key} = {prop.Value}");
                    }
                }
                else
                {
                    _logger.LogWarning("No custom application properties found on the message.");
                }

                // Optionally, log system properties as well.
                if (@event.SystemProperties != null && @event.SystemProperties.Count > 0)
                {
                    foreach (var prop in @event.SystemProperties)
                    {
                        _logger.LogInformation($"System property: {prop.Key} = {prop.Value}");
                    }
                }

                if (string.IsNullOrEmpty(messagePayload))
                {
                    _logger.LogWarning("Received empty telemetry message.");
                    return null;
                }

                // Parse JSON payload to extract required variables
                dynamic telemetry = JsonConvert.DeserializeObject(messagePayload)!;
                string serialNumber = telemetry.serialNumber;
                string name = telemetry.name;

                List<SensorReading> readings = [];

                foreach (var reading in telemetry.readings)
                {
                    string measurementType = reading.measurementType;
                    string unit = reading.unit;
                    double latestReading = Convert.ToDouble(reading.latestReading);

                    readings.Add(new() { measurementType = measurementType, unit = unit, reading = latestReading });
                }

                Device device = devices.FirstOrDefault(d => d.serialNumber == serialNumber)!;
                if (device == null)
                {
                    _logger.LogWarning($"Device with serial number {serialNumber} not found.");
                    return null;
                }

                _logger.LogInformation($"Device {device.serialNumber} found. Updating...");
                _logger.LogInformation($"Device {device.id} found. Updating...");

                // Update device and sensor data as in your previous logic
                device.isOnline = true;
                device.lastConnectionTime = DateTimeOffset.Now;

                Sensor? existingSensor = device.sensors.FirstOrDefault(s => s.name == name);
                if (existingSensor == null)
                {
                    existingSensor = new()
                    {
                        isOnline = true,
                        lastConnectionTime = DateTimeOffset.Now,
                        name = name,
                        latestReadings = readings
                    };
                    device.sensors.Add(existingSensor);
                }
                else
                {
                    existingSensor.isOnline = true;
                    existingSensor.lastConnectionTime = DateTimeOffset.Now;

                    foreach (var sensorReading in readings)
                    {
                        SensorReading? existingReading = existingSensor.latestReadings.FirstOrDefault(r => r.measurementType == sensorReading.measurementType);
                        if (existingReading == null)
                        {
                            existingSensor.latestReadings.Add(new SensorReading
                            {
                                measurementType = sensorReading.measurementType,
                                unit = sensorReading.unit,
                                reading = sensorReading.reading
                            });
                        }
                        else
                        {
                            existingReading.reading = sensorReading.reading;
                            existingReading.unit = sensorReading.unit;
                        }
                    }
                }

                _logger.LogInformation($"Device {serialNumber} updated successfully.");
                updatedDevices.Add(device);
            }

            return updatedDevices;
        }
    }
}

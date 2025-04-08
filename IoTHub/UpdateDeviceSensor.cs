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
            databaseName: "iot-device-manager",
            containerName: "devices",
            Connection = "CosmosDBConnection")]
        public object? Run(
            [EventHubTrigger("iothub-ehub-pk-idm-63834165-76a0d6dee5", Connection = "IoTHubConnection")] EventData[] events,
            [CosmosDBInput(
                databaseName: "iot-device-manager",
                containerName: "devices",
                Connection = "CosmosDBConnection")] Device[] devices)
        {
            List<Device> updatedDevices = [];
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

            EventData @event = events.FirstOrDefault()!;

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
            string measurementType = telemetry.measurementType;
            string unit = telemetry.unit;
            double latestReading = Convert.ToDouble(telemetry.latestReading);

            _logger.LogInformation($"Parsed telemetry: SerialNumber = {serialNumber}, Name = {name}," +
                $" MeasurementType = {measurementType}, Unit = {unit}, LatestReading = {latestReading}");

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
                    latestReadings =
                    [
                        new SensorReading
                        {
                            measurementType = measurementType,
                            unit = unit,
                            reading = latestReading
                        }
                    ]
                };
                device.sensors.Add(existingSensor);
            }
            else
            {
                existingSensor.isOnline = true;
                existingSensor.lastConnectionTime = DateTimeOffset.Now;

                SensorReading? existingReading = existingSensor.latestReadings.FirstOrDefault(r => r.measurementType == measurementType);
                if (existingReading == null)
                {
                    existingSensor.latestReadings.Add(new SensorReading
                    {
                        measurementType = measurementType,
                        unit = unit,
                        reading = latestReading
                    });
                }
                else
                {
                    existingReading.reading = latestReading;
                    existingReading.unit = unit;
                }
            }

            _logger.LogInformation($"Device {serialNumber} updated successfully.");
            return device;
        }
    }
}

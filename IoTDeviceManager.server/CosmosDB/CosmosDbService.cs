using Microsoft.Azure.Cosmos;

namespace IoTDeviceManager.server.CosmosDB
{
    public class CosmosDbService(CosmosDbFactory cosmosDbFactory) : ICosmosDbService
    {
        private Container DevicesContainer = cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "Devices");
    }
}

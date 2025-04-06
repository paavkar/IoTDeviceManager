using Microsoft.Azure.Cosmos;

namespace IoTDeviceManager.server.CosmosDB
{
    public class CosmosDbFactory
    {
        public CosmosClient CosmosClient { get; set; }
        public string DatabaseName { get; set; }

        public CosmosDbFactory()
        {
            DatabaseName = Environment.GetEnvironmentVariable("COSMOSDB_DATABASE") ?? "IoTDeviceManager";
            var account = Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT")
                             ?? "https://localhost:8081"; 
            var key = Environment.GetEnvironmentVariable("COSMOSDB_KEY")
                         ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            var serializationOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            };

            CosmosClient = new CosmosClient(account, key, new CosmosClientOptions()
            {
                SerializerOptions = serializationOptions
            });

            InitializeDatabase().GetAwaiter().GetResult();
        }

        public async Task InitializeDatabase()
        {
            await CosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName);

            var database = CosmosClient.GetDatabase(DatabaseName);

            await database.CreateContainerIfNotExistsAsync(id: "Devices", partitionKeyPath: "/partitionKey");
        }
    }
}

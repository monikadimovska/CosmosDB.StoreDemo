using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;
using System.Configuration;
using Microsoft.Azure.WebJobs;

namespace UpdateCategoryName.Function2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task Run([Microsoft.Azure.WebJobs.CosmosDBTrigger(
            databaseName: "Store",
            containerName: "productsMeta",
            Connection = "COSMOS_CONNECTION_STRING",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)]IReadOnlyList<Category> input,
            [CosmosDB(
                databaseName: "Store",
                containerName: "products",            
                Connection = "COSMOS_CONNECTION_STRING")]
        IAsyncCollector<Product> productsOut,
            ILogger log)
        {
            log.LogInformation("Documents modified: " + input.Count);
            log.LogInformation("First document Id: " + input[0].Id);
            log.LogInformation("First document: " + JsonConvert.SerializeObject(input));


            foreach (var document in input)
            {
                using (CosmosClient client = new(connectionString: Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING"),
                                                    new CosmosClientOptions() { AllowBulkExecution = true }))
                {
                    var database = client.GetDatabase("Store");
                    var container = database.GetContainer("products");

                    foreach (var doc in input)
                    {
                        log.LogInformation("Get documents from Products container");

                        log.LogInformation("Updated document id: " + document.Id);

                        log.LogInformation("Got Feed Iterator");

                        string sql = "SELECT * FROM c where c.categoryId = @id";
                        QueryDefinition query = new(sql);
                        query.WithParameter("@id", document.Id);

                        using FeedIterator<Product> feed = database.GetContainer("products").GetItemQueryIterator<Product>(
                                                                                            queryDefinition: query
                                                                                        );

                        string categoryName = document.CategoryName;
                        List<Task> tasks = new List<Task>();

                        while (feed.HasMoreResults)
                        {
                            var response = await feed.ReadNextAsync();

                            log.LogInformation("Records found: " + response.Count);

                            foreach (var item in response)
                            {
                                item.CategoryName = categoryName;

                                log.LogInformation("New category name: " + categoryName);
                                log.LogInformation("Update item category name: " + item.CategoryName);

                                await productsOut.AddAsync(item);
                            }
                        }
                    }
                }
            }
        }
    }
}
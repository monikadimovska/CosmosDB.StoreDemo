using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace UpdateCategoryName.Function
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "Store",
            collectionName: "productsMeta",
            ConnectionStringSetting = "COSMOS_CONNECTION_STRING",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input,
            ILogger log)
        {
            log.LogInformation("Documents modified: " + input.Count);
            log.LogInformation("First document Id: " + input[0].Id);
            log.LogInformation("First document: " + JsonConvert.SerializeObject(input));
            foreach (var document in input)
            {
                //using (DocumentClient client = new DocumentClient(new Uri("yourCosmosDbUri"), "yourCosmosDbKey"))
                using (CosmosClient client = new(connectionString: Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING"), 
                                                    new CosmosClientOptions() { AllowBulkExecution = true }))
                {
                    var database = client.GetDatabase("Store");
                    var container = database.GetContainer("products");

                    foreach (var doc in input)
                    {
                        // Perform your updates here  
                        // For instance, here's a simple example of updating a property in the document  

                        log.LogInformation("Get documents from Products container");



                        log.LogInformation("Updated document id: " + document.Id);

                        log.LogInformation("Got Feed Iterator");

                        string sql = "SELECT * FROM c where c.categoryId = @id";
                        QueryDefinition query = new(sql);
                        query.WithParameter("@id", document.Id);

                        using FeedIterator<Product> feed = database.GetContainer("products").GetItemQueryIterator<Product>(
                                                                                            queryDefinition: query
                                                                                        );

                        string categoryName = document.GetPropertyValue<string>("name");
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

                                tasks.Add(container.UpsertItemAsync(item, new PartitionKey(item.CategoryId))
                                   .ContinueWith(itemResponse =>
                                   {
                                       if (!itemResponse.IsCompletedSuccessfully)
                                       {
                                           AggregateException innerExceptions = itemResponse.Exception.Flatten();
                                           if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                                           {
                                               log.LogInformation($"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                                           }
                                           else
                                           {
                                               log.LogInformation($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                                           }
                                       }
                                   }));
                            }
                        }

                        await Task.WhenAll(tasks);
                    }
                }
            }
        }
    }
}
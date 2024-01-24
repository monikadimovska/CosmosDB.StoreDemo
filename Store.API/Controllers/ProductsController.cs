using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Store.API.Models;

namespace Store.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public ProductsController()
        {

        }

        /// <summary>
        /// NoSQL modeling, embedded 
        /// </summary>
        /// <param name="categoryId">DEMO categoryId: 26C74104-40BC-4541-8EF5-9892F7F03D72</param>
        /// <returns></returns>
        [HttpGet("averagePrice")]
        public async Task<double> GetAveragePrice(string categoryId)
        {
            string endpoint = "https://cosmosdb-demo-store.documents.azure.com:443/";
            string key = "MlbrKrLlbHEo9JVJvjfWVz6e4B3KF56O4f58UDSArssm8yGeGVSO2Ie2Q7vNt59QAhcukQu0SAjNACDbp5gFjw==";

            CosmosClient client = new CosmosClient(endpoint, key);

            Database database = client.GetDatabase("Store");

            // Get Customer Details
            Container container = database.GetContainer("products");

            string storedProcedureId = "CalculateAveragePriceByCategory";

            // Execute the stored procedure
            StoredProcedureExecuteResponse<double> response = await container.Scripts.ExecuteStoredProcedureAsync<double>(
                storedProcedureId,
                new PartitionKey(categoryId),
                null
                );

            return response.Resource;
        }

        /// <summary>
        /// NoSQL modeling, embedded 
        /// </summary>
        /// <param name="categoryId">DEMO categoryId: 26C74104-40BC-4541-8EF5-9892F7F03D72</param>
        /// <param name="productId">DEMO productId: 027D0B9A-F9D9-4C96-8213-C8546C4AAE71</param>
        /// <returns></returns>
        [HttpGet("appliedTax")]
        public async Task<Product> GetProductWithTax(string categoryId, string productId)
        {
            string endpoint = "https://cosmosdb-demo-store.documents.azure.com:443/";
            string key = "MlbrKrLlbHEo9JVJvjfWVz6e4B3KF56O4f58UDSArssm8yGeGVSO2Ie2Q7vNt59QAhcukQu0SAjNACDbp5gFjw==";

            CosmosClient client = new CosmosClient(endpoint, key);

            Database database = client.GetDatabase("Store");

            // Get Customer Details
            Container container = database.GetContainer("products");

            string storedProcedureId = "GetProductWithAppliedTax";

            // Execute the stored procedure
            StoredProcedureExecuteResponse<Product> response = await container.Scripts.ExecuteStoredProcedureAsync<Product>(
                storedProcedureId,
                new PartitionKey(categoryId),
                new dynamic[] { productId }
                );

            return response.Resource;
        }
    }
}

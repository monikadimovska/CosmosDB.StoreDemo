using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Options;
using Store.API.Models;

namespace Store.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {

        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ILogger<CustomersController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Relational approach to modeling customer 
        /// </summary>
        /// <param name="customerId">DEMO customerId: 0012D555-C7DE-4C4B-B4A4-2E8A6B8E1161</param>
        /// <returns></returns>
        [HttpGet("relationalApproach/customerProfile")]
        public async Task<ApiResponse> GetCustomerDataRelationaAppoach(string customerId)
        {
            string endpoint = "https://cosmosdb-demo-store.documents.azure.com:443/";
            string key = "MlbrKrLlbHEo9JVJvjfWVz6e4B3KF56O4f58UDSArssm8yGeGVSO2Ie2Q7vNt59QAhcukQu0SAjNACDbp5gFjw==";
            double totalRUCost = 0;

            CosmosClient client = new CosmosClient(endpoint, key);

            Database database = client.GetDatabase("Store_Relational_Approach");
            
            // Get Customer Details
            Container container = database.GetContainer("customers");
            PartitionKey partitionKey = new PartitionKey(customerId);
            ItemResponse<Customer> responseCustomer = await container.ReadItemAsync<Customer>(customerId, partitionKey);
            Customer customer = responseCustomer.Resource;
            totalRUCost += responseCustomer.RequestCharge;

            // Get Customer Address
            string sql = "SELECT * FROM customersAddress c where c.customerId = @id";
            QueryDefinition query = new(sql);
            query.WithParameter("@id", customerId);

            using FeedIterator<Address> feed = database.GetContainer("customersAddress").GetItemQueryIterator<Address>(
                queryDefinition: query
            );

            while (feed.HasMoreResults)
            {
                FeedResponse<Address> response = await feed.ReadNextAsync();
                foreach (Address address in response)
                {
                    if(customer.Addresses == null) customer.Addresses = new List<Address>();
                        customer.Addresses.Add(address);
                }

                totalRUCost += response.RequestCharge;
            }

            
            ApiResponse result = new ApiResponse();
            result.Customer = customer;
            result.TotalRUCharge= totalRUCost;

            return result;
        }

        /// <summary>
        /// NoSQL modeling, embedded 
        /// </summary>
        /// <param name="customerId">DEMO customerId: 0012D555-C7DE-4C4B-B4A4-2E8A6B8E1161</param>
        /// <returns></returns>
        [HttpGet("customerProfile")]
        public async Task<ApiResponse> GetCustomerData(string customerId)
        {
            string endpoint = "https://cosmosdb-demo-store.documents.azure.com:443/";
            string key = "MlbrKrLlbHEo9JVJvjfWVz6e4B3KF56O4f58UDSArssm8yGeGVSO2Ie2Q7vNt59QAhcukQu0SAjNACDbp5gFjw==";
            double totalRUCost = 0;

            CosmosClient client = new CosmosClient(endpoint, key);

            Database database = client.GetDatabase("Store");

            // Get Customer Details
            Container container = database.GetContainer("customers");
            PartitionKey partitionKey = new PartitionKey(customerId);
            ItemResponse<Customer> responseCustomer = await container.ReadItemAsync<Customer>(customerId, partitionKey);
            Customer customer = responseCustomer.Resource;
            totalRUCost += responseCustomer.RequestCharge;

            ApiResponse result = new ApiResponse();
            result.Customer = customer;
            result.TotalRUCharge = totalRUCost;

            return result;
        }

        /// <summary>
        /// Update the customer's loyalty points for the purchase amount
        /// </summary>
        /// <param name="customerId">DEMO customerId: 0012D555-C7DE-4C4B-B4A4-2E8A6B8E1161</param>
        /// <param name="purchaseAmount"></param>
        /// <returns></returns>
        [HttpPost("loyaltyPoints")]
        public async Task<int> UpdateLoyaltyPoints(string customerId, decimal purchaseAmount)
        {
            if(string.IsNullOrWhiteSpace(customerId)) customerId = "0012D555-C7DE-4C4B-B4A4-2E8A6B8E1161";
            string endpoint = "https://cosmosdb-demo-store.documents.azure.com:443/";
            string key = "MlbrKrLlbHEo9JVJvjfWVz6e4B3KF56O4f58UDSArssm8yGeGVSO2Ie2Q7vNt59QAhcukQu0SAjNACDbp5gFjw==";

            CosmosClient client = new CosmosClient(endpoint, key);

            Database database = client.GetDatabase("Store");

            // Get Customer Details
            Container container = database.GetContainer("customers");

            string storedProcedureId = "UpdateCustomerLoyaltyPoints";

            // Execute the stored procedure
            var response = await container.Scripts.ExecuteStoredProcedureAsync<int>(
                storedProcedureId,
                new PartitionKey(customerId),
                new dynamic[] { purchaseAmount },
                //new dynamic[] { new { test = "test"} },
                new StoredProcedureRequestOptions { EnableScriptLogging = true } // Enable to get the log form the script execution (logged via console.log)
                ) ;

            // Return total loyalty points
            return response.Resource;
        }

        /// <summary>
        /// Create new customer with creationDate set by trigger
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<Customer> CreateCustomer(CustomerRequest customer)
        {
            string endpoint = "https://cosmosdb-demo-store.documents.azure.com:443/";
            string key = "MlbrKrLlbHEo9JVJvjfWVz6e4B3KF56O4f58UDSArssm8yGeGVSO2Ie2Q7vNt59QAhcukQu0SAjNACDbp5gFjw==";

            string id = Guid.NewGuid().ToString();
            Customer customerData = new Customer
            {
                CustomerId = id,
                Id = id,
                FirstName= customer.FirstName,
                LastName = customer.LastName
            };

            CosmosClient client = new CosmosClient(endpoint, key);

            Database database = client.GetDatabase("Store");

            // Get Customer Details
            Container container = database.GetContainer("customers");

            ItemRequestOptions options = new()
            {
                PreTriggers = new List<string> { "setCreationDate" }
            };

            var response = await container.CreateItemAsync(customerData, requestOptions: options);

            // Return created customer
            return response.Resource;
        }
    }
}
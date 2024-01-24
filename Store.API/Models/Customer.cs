using Newtonsoft.Json;

namespace Store.API.Models
{
    public class Customer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("customerId")]
        public string CustomerId { get; set; }
        public string Title { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public List<Address> Addresses { get; set; }
        public Password Password { get; set; }
    }
}
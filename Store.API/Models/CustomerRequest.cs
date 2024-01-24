namespace Store.API.Models
{
    public class CustomerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        internal string Id;
        internal string CustomerId;
    }
}

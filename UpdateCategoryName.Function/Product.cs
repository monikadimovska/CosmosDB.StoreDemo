using Azure;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UpdateCategoryName
{
    public class Product
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "categoryId")]
        public string CategoryId { get; set; }

        [JsonProperty(PropertyName = "categoryName")]
        public string CategoryName { get; set; }

        [JsonProperty(PropertyName = "sku")]
        public string SKU { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public List<Tag> Tags { get; set; }
    }

    public class Tag
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}

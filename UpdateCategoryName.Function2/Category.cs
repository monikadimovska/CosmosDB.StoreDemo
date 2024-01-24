using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateCategoryName.Function2
{
    public class Category
    {
        [JsonProperty("name")]
        public string CategoryName { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}

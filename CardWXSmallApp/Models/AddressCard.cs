using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Models
{
    public class AddressCard
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }
        public String Province { get; set; }
        public String City { get; set; }
        public String District { get; set; }
    }
}

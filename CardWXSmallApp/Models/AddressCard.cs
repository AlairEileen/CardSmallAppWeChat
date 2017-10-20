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
        public Int32[] ProvinceCityIndexArray { get; set; }
        public String ProvinceName { get; set; }
        public String CityName { get; set; }
    }
}

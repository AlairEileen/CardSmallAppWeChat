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
        public ObjectId MyProperty { get; set; }
        public Int32 ProvinceIndex { get; set; }
        public String ProvinceName { get; set; }
        public Int32 CityIndex { get; set; }
        public String CityName { get; set; }
        public Int32 AreaIndex { get; set; }
        public String AreaName { get; set; }
    }
}

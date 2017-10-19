using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Models
{
    public class ProvinceCity
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("name")]
        public String Name { get; set; }
        [JsonProperty("initial")]
        public String Initial { get; set; }
        [JsonProperty("children")]
        public List<City> CityList { get; set; }
    }

    public class City
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("name")]
        public String Name { get; set; }
        [JsonProperty("initial")]
        public String Initial { get; set; }
        [JsonProperty("children")]
        public List<Area> AreaList { get; set; }
    }

    public class Area
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("name")]
        public String Name { get; set; }
        [JsonProperty("initial")]
        public String Initial { get; set; }
    }
}

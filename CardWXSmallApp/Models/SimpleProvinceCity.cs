using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Models
{
    public class SimpleProvinceCity
    {
        [JsonProperty("provinces")]
        public List<SimpleProvince> ProvinceList { get; set; }
    }

    public class SimpleProvince
    {
        [JsonProperty("provinceName")]
        public String ProvinceName { get; set; }
        [JsonProperty("citys")]
        public List<SimpleCity> SimpleCityList { get; set; }
    }
    public class SimpleCity
    {
        [JsonProperty("citysName")]
        public String CityName { get; set; }
    }

    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CardWXSmallApp.Tools.DB;
using CardWXSmallApp.Models;
using MongoDB.Driver;
using CardWXSmallApp.Tools;
using System.IO;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CardWXSmallApp.Controllers
{
    public class CommonController : Controller
    {
      public string getProvince()
        {
            ResetProvinceCity();
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(new string[] { "code", "name", "initial" });
            string jsonString = JsonConvert.SerializeObject(ConstantProperty.CityProvinceList,jsonSerializerSettings);
            return jsonString;
        }
        public string getAllProvince()
        {
            ResetProvinceCity();
            string jsonString = JsonConvert.SerializeObject(ConstantProperty.CityProvinceList);
            return jsonString;
        }
        public string getProvinceCity()
        {
            string json = null;
            using (StreamReader fs = new StreamReader("ProvinceCity.json"))
            {
                json = fs.ReadToEnd();
            }
            SimpleProvinceCity list = null;
            if (json != null)
            {
                list = JsonConvert.DeserializeObject<SimpleProvinceCity>(json);
            }
            return JsonConvert.SerializeObject(list);
        }
        private void ResetProvinceCity()
        {
            if (ConstantProperty.CityProvinceList == null || ConstantProperty.CityProvinceList.Count == 0)
            {
                string json = null;
                using (StreamReader fs = new StreamReader("city.json"))
                {
                    json = fs.ReadToEnd();
                }
                List<ProvinceCity> list = null;
                if (json != null)
                {
                    list = JsonConvert.DeserializeObject<List<ProvinceCity>>(json);
                }
                ConstantProperty.CityProvinceList = list;
            }
        }
    }
}

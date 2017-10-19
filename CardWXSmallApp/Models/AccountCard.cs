using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Models
{
    public class AccountCard 
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public String OpenId { get; set; }
        public String AvatarUrl { get; set; }
        public String AccountName { get; set; }
        public Int32 Gender { get; set; }
        [JsonConverter(typeof(Tools.DateConverter))]
        public DateTime Birthday { get; set; }
        public String PhoneNumber { get; set; }
        public String Province { get; set; }
        public String City { get; set; }
        public Int32 SafeMode { get; set; }
        [JsonIgnore]
        public DateTime CreateTime { get; set; }
        [JsonIgnore]
        public DateTime LastLoginTime { get; set; }
        public List<AddressCard> AddressList { get; set; }


        
    }
}

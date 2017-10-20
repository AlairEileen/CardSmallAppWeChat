using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Models
{
    public class NameCard
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public String Company { get; set; }
        public String Post { get; set; }
        public String Email { get; set; }
        public String More { get; set; }

        public LocationCard Address { get; set; }

        public AccountCard AccountCard { get; set; }
        public AlbumCard Album { get; set; }

    }
}

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
        [JsonIgnore]
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public String OpenId { get; set; }
        public String AvatarUrl { get; set; }
        public String AccountName { get; set; }

        private int gender = 0;
        public Int32 Gender { get => gender; set => gender = value; }
        [JsonConverter(typeof(Tools.DateConverter))]
        public DateTime Birthday { get; set; }
        public String PhoneNumber { get; set; }
        public AddressCard Address { get; set; }
        public Int32 SafeMode { get; set; }
        [JsonIgnore]
        public DateTime CreateTime { get; set; }
        [JsonIgnore]
        public DateTime LastLoginTime { get; set; }
        /// <summary>
        /// 位置列表
        /// </summary>
        private List<LocationCard> locationList = new List<LocationCard>();
        public List<LocationCard> LocationList { get => locationList; set => locationList = value; }

        /// <summary>
        /// 名片
        /// </summary>
        public NameCard NameCard { get; set; }
        /// <summary>
        /// 相册
        /// </summary>
        public List<AlbumCard> AlbumCardList { get; set; }
        /// <summary>
        /// 关系
        /// </summary>
        public RelationCard Relation { get; set; }
    }
}

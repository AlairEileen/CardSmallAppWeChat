using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Models
{
    public class InteractionCard
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        /// <summary>
        /// 0:人气，1:赞，2:留言，3:人脉
        /// </summary>
        public Int32 Mode { get; set; }
        public String content { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastChangeTime { get; set; }
        /// <summary>
        /// 主动方
        /// </summary>
        public AccountCard Sender { get; set; }
        /// <summary>
        /// 被动方
        /// </summary>
        public AccountCard Receiver { get; set; }
    }
}

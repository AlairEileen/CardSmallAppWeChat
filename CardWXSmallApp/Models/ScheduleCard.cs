using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Models
{
    public class ScheduleCard
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public String SubJect { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonConverter(typeof(Tools.DateConverterEndMinute))]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        [JsonConverter(typeof(Tools.DateConverterEndMinute))]
        public DateTime LastChangeTime { get; set; }
        /// <summary>
        /// 执行时间
        /// </summary>
        [JsonConverter(typeof(Tools.DateConverterEndMinute))]
        public DateTime Time { get; set; }
        /// <summary>
        /// 提醒
        /// </summary>
        public Int32 Remind { get; set; }
        /// <summary>
        /// 执行状态 1：未执行，2：正在执行，3：已结束
        /// </summary>
        public Int32 Status { get; set; }
        /// <summary>
        /// 更多
        /// </summary>
        public String More { get; set; }
        /// <summary>
        /// 相关图片
        /// </summary>
        public AlbumCard Pics { get; set; }
        /// <summary>
        /// test
        /// </summary>
        public MongoDBRef album { get; set; }
        /// <summary>
        /// 相关录音
        /// </summary>
        public FileCard<string> Record { get; set; }
        /// <summary>
        /// 相关链接
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// 参与人
        /// </summary>
        public List<AccountSchedule> ReceiverList { get; set; }
    }

    public class AlbumCardRef : MongoDBRef
    {
        public AlbumCardRef(string collectionName, BsonValue id) : base(collectionName, id)
        {
            
        }
        
    }

    public class AccountSchedule
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public String AvatarUrl { get; set; }
        public String AccountName { get; set; }
        public String PhoneNumber { get; set; }
        public String Post { get; set; }
        /// <summary>
        /// 是否是发起人
        /// </summary>
        public bool IsSender { get; set; }
        /// <summary>
        /// 加入时间
        /// </summary>
        [JsonConverter(typeof(Tools.DateConverterEndMinute))]
        public DateTime JoinTime { get; set; }
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastChangeTime { get; set; }
        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 提醒
        /// </summary>
        public Int32 Remind { get; set; }
        /// <summary>
        /// 更多
        /// </summary>
        public String More { get; set; }
        /// <summary>
        /// 发起人
        /// </summary>
        public AccountCard Sender { get; set; }
        /// <summary>
        /// 参与人
        /// </summary>
        public List<AccountCard> ReceiverList { get; set; }
    }
}

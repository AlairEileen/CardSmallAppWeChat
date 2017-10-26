using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Models
{

    public class RelationCard
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public RelationContent RelationActive { get; set; }
        public RelationContent RelationUnActive { get; set; }

    }

    public class RelationContent
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public Int32 LeaveWordCount{get; set;}
        public Int32 PopularCount{get; set;}
        public Int32 FabulousCount { get; set;}

        /// <summary>
        /// 留言
        /// </summary>
        public List<LeaveWord> LeaveWordList { get; set; }
        /// <summary>
        /// 人气
        /// </summary>
        public List<Popular> PopularList { get; set; }
        /// <summary>
        /// 赞
        /// </summary>
        public List<Fabulous> FabulousList { get; set; }
    }

    public class LeaveWord
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public String Content { get; set; }
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 关联人
        /// </summary>
        public AccountRelation RelatedPerson { get; set; }
    }

    public class AccountRelation
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public String OpenId { get; set; }
        public String AvatarUrl { get; set; }
        public String AccountName { get; set; }
        public String Post { get; set; }
    }

    public class Popular
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 关联人
        /// </summary>
        public AccountRelation RelatedPerson { get; set; }
    }

    public class Fabulous
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 关联人
        /// </summary>
        public AccountRelation RelatedPerson { get; set; }
    }

}

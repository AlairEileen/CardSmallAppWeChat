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
        public Int32 LeaveWordCount
        {
            get
            {
                if (LeaveWordList != null)
                {
                    LeaveWordCount = LeaveWordList.Count;
                }
                return LeaveWordCount;
            }
            set { LeaveWordCount = value; }
        }
        public Int32 PopularCount
        {
            get
            {
                if (PopularList != null)
                {
                    PopularCount = PopularList.Count;
                }
                return PopularCount;
            }
            set { PopularCount = value; }
        }
        public Int32 FabulousCount
        {
            get
            {
                if (FabulousList != null)
                {
                    FabulousCount = FabulousList.Count;
                }
                return FabulousCount;
            }
            set { FabulousCount = value; }
        }
        public Int32 PeopleCount
        {
            get
            {
                if (PeopleList != null)
                {
                    PeopleCount = PeopleList.Count;
                }
                return PeopleCount;
            }
            set { PeopleCount = value; }
        }
        public List<LeaveWord> LeaveWordList { get; set; }
        public List<Popular> PopularList { get; set; }
        public List<Fabulous> FabulousList { get; set; }
        public List<People> PeopleList { get; set; }
    }

    public class LeaveWord
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public String content { get; set; }
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
        public String Company { get; set; }
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

    public class People
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

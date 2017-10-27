using CardWXSmallApp.Models;
using CardWXSmallApp.Tools.Strings;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CardWXSmallApp.AppData.DB
{
    public class RelationData
    {
        #region 重建个人关联信息
        /// <summary>
        /// 重建个人关联信息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="saveName"></param>
        /// <param name="dbTool"></param>
        internal void ResetRelationInfo(string openId)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessResetCardHolder), openId);
        }

        /// <summary>
        /// 执行线程
        /// </summary>
        /// <param name="state"></param>
        private void ProcessResetCardHolder(object state)
        {
            string openId = (string)state;
            var collection = new MongoDBTool().GetMongoCollection<AccountCard>();
            var thisAccount = collection.Find(x => x.OpenId.Equals(openId)).FirstOrDefault();
            if (thisAccount == null)
            {
                return;
            }
            AccountRelation accountRelation = new AccountRelation()
            {
                Id = thisAccount.Id,
                AccountName = thisAccount.AccountName,
                PhoneNumber = thisAccount.PhoneNumber,
                OpenId = thisAccount.OpenId,
                Post = thisAccount.NameCard == null ? "暂无" : thisAccount.NameCard.Post,
                AvatarUrl = thisAccount.AvatarUrl,
                PostLetterFirst = thisAccount.NameCard.Post.GetLetterFirst(),
                AccountNameLetterFirst = thisAccount.AccountName.GetLetterFirst()
            };
            ///获取到所有关联用户Id
            ///
            List<ObjectId> relationObjectIds = GetAllRelationObjectId(thisAccount);
            if (relationObjectIds == null)
            {
                return;
            }
            ProcessResetInfo(thisAccount.Id, collection, relationObjectIds, accountRelation);
        }

        /// <summary>
        /// 重置信息
        /// </summary>
        /// <param name="relationObjectIds"></param>
        /// <param name="accountRelation"></param>
        private void ProcessResetInfo(ObjectId thisId, IMongoCollection<AccountCard> collection, List<ObjectId> relationObjectIds, AccountRelation accountRelation)
        {
            var list = collection.Find(Builders<AccountCard>.Filter.In(x => x.Id, relationObjectIds)).ToList();
            foreach (var item in list)
            {
                if (item.Relation == null)
                {
                    continue;
                }
                if (item.Relation.RelationActive != null)
                {
                    if (item.Relation.RelationActive.FabulousList != null)
                    {
                        var fabulous = item.Relation.RelationActive.FabulousList.Find(x => x.RelatedPerson.Id.Equals(thisId));
                        if (fabulous != null) fabulous.RelatedPerson = accountRelation;
                    }
                    if (item.Relation.RelationActive.LeaveWordList != null)
                    {
                        var leaveWord = item.Relation.RelationActive.LeaveWordList.Find(x => x.RelatedPerson.Id.Equals(thisId));
                        if (leaveWord != null) leaveWord.RelatedPerson = accountRelation;
                    }
                    if (item.Relation.RelationActive.PeopleList != null)
                    {
                        var people = item.Relation.RelationActive.PeopleList.Find(x => x.RelatedPerson.Id.Equals(thisId));
                        if (people != null) people.RelatedPerson = accountRelation;
                    }
                    if (item.Relation.RelationActive.PopularList != null)
                    {
                        var popular = item.Relation.RelationActive.PopularList.Find(x => x.RelatedPerson.Id.Equals(thisId));
                        if (popular != null) popular.RelatedPerson = accountRelation;
                    }
                }
                if (item.Relation.RelationUnActive != null)
                {
                    if (item.Relation.RelationUnActive.FabulousList != null)
                    {
                        var fabulousU = item.Relation.RelationUnActive.FabulousList.Find(x => x.RelatedPerson.Id.Equals(thisId));
                        if (fabulousU != null) fabulousU.RelatedPerson = accountRelation;
                    }
                    if (item.Relation.RelationUnActive.LeaveWordList != null)
                    {
                        var leaveWordU = item.Relation.RelationUnActive.LeaveWordList.Find(x => x.RelatedPerson.Id.Equals(thisId));
                        if (leaveWordU != null) leaveWordU.RelatedPerson = accountRelation;
                    }
                    if (item.Relation.RelationUnActive.PeopleList != null)
                    {
                        var peopelU = item.Relation.RelationUnActive.PeopleList.Find(x => x.RelatedPerson.Id.Equals(thisId));
                        if (peopelU != null) peopelU.RelatedPerson = accountRelation;
                    }
                    if (item.Relation.RelationUnActive.PopularList != null)
                    {
                        var popularU = item.Relation.RelationUnActive.PopularList.Find(x => x.RelatedPerson.Id.Equals(thisId));
                        if (popularU != null) popularU.RelatedPerson = accountRelation;
                    }
                }
                var update = Builders<AccountCard>.Update.Set(x => x.Relation, item.Relation);
                var filter = Builders<AccountCard>.Filter.Eq(x=>x.Id,item.Id);
                collection.UpdateOneAsync(filter, update);
            }
        }

        /// <summary>
        /// 获取所有关联用户id
        /// </summary>
        /// <param name="thisAccount"></param>
        /// <returns></returns>
        private List<ObjectId> GetAllRelationObjectId(AccountCard thisAccount)
        {
            if (thisAccount.Relation == null)
            {
                return null;
            }
            List<ObjectId> list = new List<ObjectId>();
            if (thisAccount.Relation.RelationActive != null)
            {
                if (thisAccount.Relation.RelationActive.FabulousList != null)
                {
                    foreach (var item in thisAccount.Relation.RelationActive.FabulousList)
                    {
                        list.Add(item.RelatedPerson.Id);
                    }
                }
                if (thisAccount.Relation.RelationActive.LeaveWordList != null)
                {
                    foreach (var item in thisAccount.Relation.RelationActive.LeaveWordList)
                    {
                        list.Add(item.RelatedPerson.Id);
                    }
                }
                if (thisAccount.Relation.RelationActive.PeopleList != null)
                {
                    foreach (var item in thisAccount.Relation.RelationActive.PeopleList)
                    {
                        list.Add(item.RelatedPerson.Id);
                    }
                }
                if (thisAccount.Relation.RelationActive.PopularList != null)
                {
                    foreach (var item in thisAccount.Relation.RelationActive.PopularList)
                    {
                        list.Add(item.RelatedPerson.Id);
                    }
                }
            }
            if (thisAccount.Relation.RelationUnActive != null)
            {

                if (thisAccount.Relation.RelationUnActive.FabulousList != null)
                {
                    foreach (var item in thisAccount.Relation.RelationUnActive.FabulousList)
                    {
                        list.Add(item.RelatedPerson.Id);
                    }
                }
                if (thisAccount.Relation.RelationUnActive.LeaveWordList != null)
                {
                    foreach (var item in thisAccount.Relation.RelationUnActive.LeaveWordList)
                    {
                        list.Add(item.RelatedPerson.Id);
                    }
                }
                if (thisAccount.Relation.RelationUnActive.PeopleList != null)
                {
                    foreach (var item in thisAccount.Relation.RelationUnActive.PeopleList)
                    {
                        list.Add(item.RelatedPerson.Id);
                    }
                }
                if (thisAccount.Relation.RelationUnActive.PopularList != null)
                {
                    foreach (var item in thisAccount.Relation.RelationUnActive.PopularList)
                    {
                        list.Add(item.RelatedPerson.Id);
                    }
                }
            }
            return list.Distinct().ToList();
        }
        #endregion
    }
}

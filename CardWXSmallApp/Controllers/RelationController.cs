using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CardWXSmallApp.Models;
using CardWXSmallApp.AppData.DB;
using MongoDB.Driver;
using CardWXSmallApp.ResponseModels;
using Newtonsoft.Json;
using CardWXSmallApp.Tools;
using MongoDB.Bson;
using CardWXSmallApp.Tools.Strings;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CardWXSmallApp.Controllers
{
    public class RelationController : Controller
    {

        #region 添加或者移出关系
        /// <summary>
        /// 添加或者移出关系
        /// </summary>
        /// <param name="thisOpenId">当前用户openid</param>
        /// <param name="thatOpenId">关联用户openid</param>
        /// <param name="relationType">0:人气，1：赞，2：留言，3：人脉</param>
        /// <param name="content">留言内容</param>
        /// <returns></returns>
        public string PushOrPullRelation(string thisOpenId, string thatOpenId, int relationType, string content)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            if (thisOpenId == null || thatOpenId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                responseModel.JsonData = $@"参数：thisOpenId:{thisOpenId},thatOpenId{thatOpenId}";
                return JsonConvert.SerializeObject(responseModel);
            }
            else if (thisOpenId.Equals(thatOpenId))
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                responseModel.JsonData = $@"不能这样执行";
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;
            try
            {
                var collection = new MongoDBTool().GetMongoCollection<AccountCard>();
                var thisAccount = collection.Find(x => x.OpenId.Equals(thisOpenId)).FirstOrDefault();
                var thatAccount = collection.Find(x => x.OpenId.Equals(thatOpenId)).FirstOrDefault();

                if (thisAccount == null || thatAccount == null)
                {
                    responseModel.StatusCode = (int)ActionParams.code_error_null;
                    responseModel.JsonData = "账户不存在";
                }
                else
                {
                    AccountRelation thisAccountRelation, thatAccountRelation;
                    InitPushOrPullRelationParams(thisAccount, thatAccount, out thisAccountRelation, out thatAccountRelation);
                    ObjectId popularId = ObjectId.GenerateNewId(), fabulousId = ObjectId.GenerateNewId(), leaveWordId = ObjectId.GenerateNewId(), peopleId = ObjectId.GenerateNewId();
                    DoRelation(thisAccount.Relation.RelationActive, relationType, content, thatAccountRelation, popularId, fabulousId, leaveWordId, peopleId);
                    DoRelation(thatAccount.Relation.RelationUnActive, relationType, content, thisAccountRelation, popularId, fabulousId, leaveWordId, peopleId);
                    var thisUpdate = Builders<AccountCard>.Update.Set(x => x.Relation, thisAccount.Relation);
                    var thatUpdate = Builders<AccountCard>.Update.Set(x => x.Relation, thatAccount.Relation);
                    collection.UpdateOne(x => x.Id.Equals(thisAccount.Id), thisUpdate);
                    collection.UpdateOne(x => x.Id.Equals(thatAccount.Id), thatUpdate);
                }
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
            }

            return JsonConvert.SerializeObject(responseModel);
        }

        private void DoRelation(RelationContent relationContent, int relationType, string content, AccountRelation accountRelation, ObjectId popularId, ObjectId fabulousId, ObjectId leaveWordId, ObjectId peopleId)
        {
            switch (relationType)
            {
                case 0:
                    if (relationContent.PopularList == null)
                    {
                        relationContent.PopularList = new List<Popular>();
                    }
                    if (!relationContent.PopularList.Exists(x => x.RelatedPerson.Id.Equals(accountRelation.Id)))
                    {
                        relationContent.PopularList.Add(new Popular() { Id = popularId, CreateTime = DateTime.Now, RelatedPerson = accountRelation });
                        relationContent.PopularCount = relationContent.PopularList.Count();
                    }
                    break;
                case 1:
                    if (relationContent.FabulousList == null)
                    {
                        relationContent.FabulousList = new List<Fabulous>();
                    }
                    Fabulous fabulous = relationContent.FabulousList.Find(x => x.RelatedPerson.Id.Equals(accountRelation.Id));
                    if (fabulous == null)
                    {
                        relationContent.FabulousList.Add(new Fabulous() { Id = fabulousId, CreateTime = DateTime.Now, RelatedPerson = accountRelation });
                    }
                    else
                    {
                        relationContent.FabulousList.Remove(fabulous);
                    }
                    relationContent.FabulousCount = relationContent.FabulousList.Count();

                    break;
                case 2:
                    if (relationContent.LeaveWordList == null)
                    {
                        relationContent.LeaveWordList = new List<LeaveWord>();
                    }
                    relationContent.LeaveWordList.Add(new LeaveWord() { Id = leaveWordId, CreateTime = DateTime.Now, RelatedPerson = accountRelation, Content = content });
                    relationContent.LeaveWordCount = relationContent.LeaveWordList.Count();
                    break;
                case 3:
                    if (relationContent.PeopleList == null)
                    {
                        relationContent.PeopleList = new List<People>();
                    }
                    People people = relationContent.PeopleList.Find(x => x.RelatedPerson.Id.Equals(accountRelation.Id));
                    if (people == null)
                    {
                        relationContent.PeopleList.Add(new People() { Id = peopleId, CreateTime = DateTime.Now, RelatedPerson = accountRelation });
                    }
                    else
                    {
                        relationContent.PeopleList.Remove(people);
                    }
                    relationContent.FabulousCount = relationContent.PeopleList.Count();

                    break;
            }
        }

        private void InitPushOrPullRelationParams(AccountCard thisAccount, AccountCard thatAccount, out AccountRelation thisAccountRelation, out AccountRelation thatAccountRelation)
        {
            thisAccountRelation = new AccountRelation()
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
            thatAccountRelation = new AccountRelation()
            {
                Id = thatAccount.Id,
                AccountName = thatAccount.AccountName,
                PhoneNumber = thatAccount.PhoneNumber,
                OpenId = thatAccount.OpenId,
                Post = thatAccount.NameCard == null ? "暂无" : thatAccount.NameCard.Post,
                AvatarUrl = thatAccount.AvatarUrl,
                PostLetterFirst = thatAccount.NameCard.Post.GetLetterFirst(),
                AccountNameLetterFirst = thatAccount.AccountName.GetLetterFirst()
            };
            //thatAccountRelation = new AccountRelation() { AccountName = thatAccount.AccountName, AvatarUrl = thatAccount.AvatarUrl, Id = thatAccount.Id, OpenId = thatAccount.OpenId, Post = thatAccount.NameCard == null ? "暂无" : thatAccount.NameCard.Post };
            if (thisAccount.Relation == null)
            {
                thisAccount.Relation = new RelationCard() { Id = ObjectId.GenerateNewId() };
            }
            if (thatAccount.Relation == null)
            {
                thatAccount.Relation = new RelationCard();
            }
            if (thatAccount.Relation.RelationUnActive == null)
            {
                thatAccount.Relation.RelationUnActive = new RelationContent() { Id = ObjectId.GenerateNewId() };
            }
            if (thisAccount.Relation.RelationActive == null)
            {
                thisAccount.Relation.RelationActive = new RelationContent() { Id = ObjectId.GenerateNewId() };
            }
        }

        #endregion
        /// <summary>
        /// 删除留言
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="id">留言id</param>
        /// <returns></returns>
        public string DelLeaveWord(string openId, string id)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            responseModel.StatusCode = (int)ActionParams.code_ok;

            if (openId == null || id == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                responseModel.JsonData = $@"参数：openId:{openId} or id:{id}";
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;

            var collection = new MongoDBTool().GetMongoCollection<AccountCard>();
            var account = collection.Find(x => x.OpenId.Equals(openId)).FirstOrDefault();
            ObjectId leaveWordRelateId = ObjectId.Empty;
            if (account != null)
            {
                var leaveWord = account.Relation.RelationActive.LeaveWordList.Find(x => x.Id.Equals(new ObjectId(id)));
                leaveWordRelateId = leaveWord.RelatedPerson.Id;
                account.Relation.RelationActive.LeaveWordList.Remove(leaveWord);
                account.Relation.RelationActive.LeaveWordCount = account.Relation.RelationActive.LeaveWordList.Count;
                var update = Builders<AccountCard>.Update.Set(x => x.Relation.RelationActive.LeaveWordList, account.Relation.RelationActive.LeaveWordList);
                collection.UpdateOne(x => x.Id.Equals(account.Id), update);
            }
            else { responseModel.StatusCode = (int)ActionParams.code_error_null; }
            AccountCard thatAccount = null;
            if (leaveWordRelateId != ObjectId.Empty)
            {
                thatAccount = collection.Find(x => x.Id.Equals(leaveWordRelateId)).FirstOrDefault();

            }
            else { responseModel.StatusCode = (int)ActionParams.code_error_null; }

            if (thatAccount != null)
            {
                var leaveWord = thatAccount.Relation.RelationUnActive.LeaveWordList.Find(x => x.Id.Equals(new ObjectId(id)));
                leaveWordRelateId = leaveWord.RelatedPerson.Id;
                thatAccount.Relation.RelationUnActive.LeaveWordList.Remove(leaveWord);
                thatAccount.Relation.RelationUnActive.LeaveWordCount = thatAccount.Relation.RelationUnActive.LeaveWordList.Count;
                var update = Builders<AccountCard>.Update.Set(x => x.Relation.RelationUnActive.LeaveWordList, thatAccount.Relation.RelationUnActive.LeaveWordList);
                collection.UpdateOne(x => x.Id.Equals(thatAccount.Id), update);
            }
            else { responseModel.StatusCode = (int)ActionParams.code_error_null; }

            return JsonConvert.SerializeObject(responseModel);
        }
    }
}

﻿using System;
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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CardWXSmallApp.Controllers
{
    public class RelationController : Controller
    {
        /// <summary>
        /// 添加或者移出关系
        /// </summary>
        /// <param name="thisOpenId">当前用户openid</param>
        /// <param name="thatOpenId">关联用户openid</param>
        /// <param name="relationType">0:人气，1：赞，2：留言</param>
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
                    DoRelation(thisAccount.Relation.RelationActive, relationType, content, thatAccountRelation);
                    DoRelation(thatAccount.Relation.RelationUnActive, relationType, content, thisAccountRelation);
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

        private void DoRelation(RelationContent relationContent, int relationType, string content, AccountRelation accountRelation)
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
                        relationContent.PopularList.Add(new Popular() { CreateTime = DateTime.Now, RelatedPerson = accountRelation });
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
                        relationContent.FabulousList.Add(new Fabulous() { CreateTime = DateTime.Now, RelatedPerson = accountRelation });
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
                    relationContent.LeaveWordList.Add(new LeaveWord() { CreateTime = DateTime.Now, RelatedPerson = accountRelation, Content = content });
                    relationContent.LeaveWordCount = relationContent.LeaveWordList.Count();
                    break;
            }
        }

        private void InitPushOrPullRelationParams(AccountCard thisAccount, AccountCard thatAccount, out AccountRelation thisAccountRelation, out AccountRelation thatAccountRelation)
        {
            thisAccountRelation = new AccountRelation() { AccountName = thisAccount.AccountName, AvatarUrl = thisAccount.AvatarUrl, Id = thisAccount.Id, OpenId = thisAccount.OpenId, Post = thisAccount.NameCard == null ? "暂无" : thisAccount.NameCard.Post };
            thatAccountRelation = new AccountRelation() { AccountName = thatAccount.AccountName, AvatarUrl = thatAccount.AvatarUrl, Id = thatAccount.Id, OpenId = thatAccount.OpenId, Post = thatAccount.NameCard == null ? "暂无" : thatAccount.NameCard.Post };
            if (thisAccount.Relation == null)
            {
                thisAccount.Relation = new RelationCard() { Id=ObjectId.GenerateNewId()};
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
    }
}
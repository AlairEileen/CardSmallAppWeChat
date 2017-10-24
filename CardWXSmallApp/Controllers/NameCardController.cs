using CardWXSmallApp.Models;
using CardWXSmallApp.ResponseModels;
using CardWXSmallApp.Tools;
using CardWXSmallApp.Tools.DB;
using CardWXSmallApp.Tools.Strings;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Controllers
{
    public class NameCardController : Controller
    {
        /// <summary>
        /// 修改名片信息
        /// </summary>
        /// <param name="accountCard">基本信息</param>
        /// <param name="nameCard">名片信息</param>
        /// <param name="albumCard">照片</param>
        /// <param name="location">位置</param>
        /// <param name="fileIdList">照片数组</param>
        /// <returns></returns>
        [HttpGet]
        public string ChangeNameCard(AccountCard accountCard, NameCard nameCard, AlbumCard albumCard, LocationCard location, string[] fileIdList)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            if (accountCard.OpenId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                responseModel.JsonData = $@"参数：openId:{accountCard.OpenId}";
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;

            var mongo = new MongoDBTool();
            List<FileCard<string[]>> fileCardList = null;
            try
            {
                if (fileIdList != null && fileIdList.Count() > 0)
                {

                    ObjectId[] objectIds = new ObjectId[fileIdList.Length];
                    for (int i = 0; i < fileIdList.Length; i++)
                    {
                        objectIds[i] = new ObjectId(fileIdList[i]);
                    }
                    var filterSelect = Builders<FileCard<string[]>>.Filter.In(x => x.Id, objectIds);
                    var fileList = mongo.GetMongoCollection<FileCard<string[]>>("FileCard").Find(filterSelect).ToList();
                    fileCardList = fileList;

                }

                var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, accountCard.OpenId);
                var list = mongo.GetMongoCollection<AccountCard>().Find(filter);
                var account = list.FirstOrDefault();
                if (location.Latitude != 0 && location.Longitude != 0 && !string.IsNullOrEmpty(location.GPSAddress))
                {
                    location.Id = ObjectId.GenerateNewId();
                    nameCard.Location = location;
                }
                if (fileCardList != null)
                {
                    if (account.NameCard.Album != null)
                    {
                        albumCard.Id = albumCard.Id = account.NameCard.Album.Id;
                    }
                    else
                    {
                        albumCard.Id = ObjectId.GenerateNewId();
                        albumCard.Name = "名片相册";
                        albumCard.CreateTime = DateTime.Now;
                    }
                    if (account.NameCard != null)
                    {
                        nameCard.Id = account.NameCard.Id;
                    }
                    else
                    {
                        nameCard.Id = ObjectId.GenerateNewId();
                    }
                    albumCard.ImageList = fileCardList;
                }
                nameCard.Album = albumCard;
                var update = Builders<AccountCard>.Update.Set(x => x.NameCard, nameCard).Set(x => x.PhoneNumber, accountCard.PhoneNumber).Set(x => x.AccountName, accountCard.AccountName).Set(x => x.NameCard, nameCard);
                mongo.GetMongoCollection<AccountCard>().UpdateOne(filter, update);
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;

                //throw;
            }
            return JsonConvert.SerializeObject(responseModel);

        }

        /// <summary>
        /// 获取名片列表
        /// </summary>
        /// <param name="accountCard"></param>
        /// <returns></returns>
        public string GetAllNameCard(AccountCard accountCard)
        {
            BaseResponseModel<List<NameCardSave>> responseModel = new BaseResponseModel<List<NameCardSave>>();
            if (accountCard.OpenId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;
            try
            {
                accountCard = new MongoDBTool().GetMongoCollection<AccountCard>().Find(x => x.OpenId.Equals(accountCard.OpenId)).FirstOrDefault();
                var list = accountCard.CardHolder;
                responseModel.JsonData = list;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                //throw;
            }
            return JsonConvert.SerializeObject(responseModel);
        }

        public string PutToCardHolder(string myOpenId, string hisOpenId)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            if (string.IsNullOrEmpty(myOpenId) || string.IsNullOrEmpty(hisOpenId))
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;
            try
            {
                var collection = new MongoDBTool().GetMongoCollection<AccountCard>();
                var myFilter = Builders<AccountCard>.Filter.Eq(x=>x.OpenId,myOpenId);
                var hisFilter = Builders<AccountCard>.Filter.Eq(x=>x.OpenId,hisOpenId);
                var myAccountCard = collection.Find(myFilter).FirstOrDefault();
                var hisAccountCard = collection.Find(hisFilter).FirstOrDefault();
                NameCardSave mySaveCard = GetSaveCard(myAccountCard, hisOpenId);
                NameCardSave hisSaveCard = GetSaveCard(hisAccountCard, myOpenId);
                List<NameCardSave> myList = ConvertToNameCardSaveList(myAccountCard, mySaveCard, hisAccountCard);
                List<NameCardSave> hisList = ConvertToNameCardSaveList(hisAccountCard,hisSaveCard,myAccountCard);
                var myUpdate = Builders<AccountCard>.Update.Set(x=>x.CardHolder,myList);
                var hisUpdate = Builders<AccountCard>.Update.Set(x=>x.CardHolderReceive,hisList);
                collection.UpdateOne(myFilter,myUpdate);
                collection.UpdateOne(hisFilter,hisUpdate);
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                //throw;
            }
            return JsonConvert.SerializeObject(responseModel);
        }

        private List<NameCardSave> ConvertToNameCardSaveList(AccountCard myAccountCard, NameCardSave saveCard, AccountCard hisAccountCard)
        {
            List<NameCardSave> list = new List<NameCardSave>();
            if (saveCard == null)
            {
                list.AddRange(myAccountCard.CardHolder);
                NameCardSave nameCardSave = null;
                if (hisAccountCard.NameCard == null)
                {
                    nameCardSave = new NameCardSave() { AccountName = hisAccountCard.AccountName, PhoneNumber = hisAccountCard.PhoneNumber, OpenId = hisAccountCard.OpenId, AvatarUrl = hisAccountCard.AvatarUrl, AccountNameLetterFirst = hisAccountCard.AccountName.GetLetterFirst() };
                }
                else
                {
                    nameCardSave = new NameCardSave() { AccountName = hisAccountCard.AccountName, PhoneNumber = hisAccountCard.PhoneNumber, OpenId = hisAccountCard.OpenId, Post = hisAccountCard.NameCard.Post, AvatarUrl = hisAccountCard.AvatarUrl, PostLetterFirst = hisAccountCard.NameCard.Post.GetLetterFirst(), AccountNameLetterFirst = hisAccountCard.AccountName.GetLetterFirst() };
                }

                list.Add(nameCardSave);
            }
            else
            {
                myAccountCard.CardHolder.Remove(saveCard);
                list.AddRange(myAccountCard.CardHolder);
            }
            return list;
        }

        private NameCardSave GetSaveCard(AccountCard accountCard, string openId)
        {
            if (accountCard.CardHolder == null)
            {
                accountCard.CardHolder = new List<NameCardSave>();
                return null;
            }
            else
            {
                return accountCard.CardHolder.Find(x => x.OpenId.Equals(openId));
            }
        }




    }
}

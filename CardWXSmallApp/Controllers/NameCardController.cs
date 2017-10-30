using CardWXSmallApp.AppData.DB;
using CardWXSmallApp.Models;
using CardWXSmallApp.ResponseModels;
using CardWXSmallApp.Tools;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardWXSmallApp.Controllers
{
    public class NameCardController : Controller
    {
        RelationData relationData = new RelationData();

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
        public string ChangeNameCard(AccountCard accountCard, NameCard nameCard, AlbumCard albumCard, LocationCard location, string fileIdList)
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
                if (!string.IsNullOrEmpty(fileIdList) && fileIdList.Count() > 0)
                {
                    string[] fileIds=JsonConvert.DeserializeObject<string[]>(fileIdList);
                    if (fileIds!=null&&fileIds.Count()!=0)
                    {
                        ObjectId[] objectIds = new ObjectId[fileIdList.Length];
                        for (int i = 0; i < fileIds.Count(); i++)
                        {
                            objectIds[i] = new ObjectId(fileIds[i]);
                        }
                        var filterSelect = Builders<FileCard<string[]>>.Filter.In(x => x.Id, objectIds);
                        var fileList = mongo.GetMongoCollection<FileCard<string[]>>("FileCard").Find(filterSelect).ToList();
                        fileCardList = fileList;
                    }
                   
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
                    if (account.NameCard != null && account.NameCard.Album != null)
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
                ///重置关联信息
                //nameCardData.ResetCardHolder(accountCard.OpenId);
                relationData.ResetRelationInfo(accountCard.OpenId);
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
        /// <param name="sortType">0：名称正序，1：公司名正序，2：时间倒序</param>
        /// <param name="searchParam"></param>
        /// <returns></returns>
        public string FindAllNameCard(AccountCard accountCard, int sortType, string searchParam)
        {
            BaseResponseModel<IEnumerable<IGrouping<string, People>>> responseModel = new BaseResponseModel<IEnumerable<IGrouping<string, People>>>();
            if (accountCard.OpenId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;
            try
            {
                accountCard = new MongoDBTool().GetMongoCollection<AccountCard>().Find(x => x.OpenId.Equals(accountCard.OpenId)).FirstOrDefault();
                List<People> list = new List<People>();
                list = accountCard.Relation.RelationActive.PeopleList;
                if (searchParam != null)
                {
                    list = accountCard.Relation.RelationActive.PeopleList.FindAll(x => x.RelatedPerson.AccountName.Contains(searchParam) || x.RelatedPerson.Post.Contains(searchParam));
                }

                var groupList = list.GroupBy(x => sortType == 0 ? x.RelatedPerson.AccountNameLetterFirst : sortType == 1 ? x.RelatedPerson.PostLetterFirst : x.CreateTime.ToString());
                responseModel.JsonData = groupList;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                //throw;
            }

            return JsonConvert.SerializeObject(responseModel);
        }


    }
}

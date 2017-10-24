using CardWXSmallApp.Models;
using CardWXSmallApp.ResponseModels;
using CardWXSmallApp.Tools;
using CardWXSmallApp.Tools.DB;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Controllers
{
    public class AlbumController
    {
        /// <summary>
        /// 相册修改或者添加
        /// </summary>
        /// <param name="openId">openId</param>
        /// <param name="albumCard">相册内容</param>
        /// <param name="fileIdList">相片文件id数组</param>
        /// <returns>执行状态</returns>
        public string ChangeAlbum(string openId, AlbumCard albumCard, string[] fileIdList)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            if (openId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                responseModel.JsonData = $@"参数：openId:{openId}";
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;
            try
            {
                var dbTool = new MongoDBTool();
                var collection = dbTool.GetMongoCollection<AccountCard>();
                var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, openId);
                var accountCard = collection.Find(filter).FirstOrDefault();
                List<AlbumCard> list = new List<AlbumCard>();
                if (accountCard != null)
                {
                    if (accountCard.AlbumCardList != null)
                    {
                        list.AddRange(accountCard.AlbumCardList);
                    }
                    albumCard.Id = ObjectId.GenerateNewId();
                    List<FileCard<string[]>> fileCardList = null;

                    if (fileIdList != null)
                    {
                        ObjectId[] objectIds = new ObjectId[fileIdList.Length];
                        for (int i = 0; i < fileIdList.Length; i++)
                        {
                            objectIds[i] = new ObjectId(fileIdList[i]);
                        }
                        var filterSelect = Builders<FileCard<string[]>>.Filter.In(x => x.Id, objectIds);
                        var fileList = dbTool.GetMongoCollection<FileCard<string[]>>("FileCard").Find(filterSelect).ToList();
                        fileCardList = fileList;
                    }
                    albumCard.ImageList = fileCardList;
                    list.Add(albumCard);

                }
                var update = Builders<AccountCard>.Update.Set(x => x.AlbumCardList, list);
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                responseModel.JsonData = "请求参数有误";
            }


            return JsonConvert.SerializeObject(responseModel);
        }

        /// <summary>
        /// 删除相册
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="albumId"></param>
        /// <returns></returns>
        public string DelAlbum(string openId, string albumId)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            responseModel.StatusCode = (int)ActionParams.code_ok;

            if (openId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                responseModel.JsonData = $@"参数：openId:{openId}";
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;

            try
            {
                var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, openId);
                var update = Builders<AccountCard>.Update.PullFilter(x => x.AlbumCardList, y => y.Id.Equals(new ObjectId(albumId)));
                new MongoDBTool().GetMongoCollection<AccountCard>().UpdateOne(filter, update);
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                throw;
            }
            return JsonConvert.SerializeObject(responseModel);
        }
      
        /// <summary>
        /// 查询所有相册
        /// </summary>
        /// <param name="accountCard">用户openid</param>
        /// <returns>相册列表</returns>
        public string FindAllAlbum(AccountCard accountCard)
        {
            BaseResponseModel<List<AlbumCard>> responseModel = new BaseResponseModel<List<AlbumCard>>();
            if (accountCard.OpenId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;

            try
            {
                var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, accountCard.OpenId);
                accountCard = new MongoDBTool().GetMongoCollection<AccountCard>().Find(filter).FirstOrDefault();
                List<AlbumCard> list = new List<AlbumCard>();
                if (accountCard != null)
                {
                    if (accountCard.NameCard.Album != null)
                    {
                        list.Add(accountCard.NameCard.Album);
                    }
                    if (accountCard.AlbumCardList != null)
                    {
                        list.AddRange(accountCard.AlbumCardList);
                    }
                }
                responseModel.JsonData = list;
            }
            catch (Exception)
            {

                throw;
            }

            return JsonConvert.SerializeObject(responseModel);
        }
    }
}

using CardWXSmallApp.Models;
using CardWXSmallApp.ResponseModels;
using CardWXSmallApp.Tools;
using CardWXSmallApp.Tools.DB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CardWXSmallApp.Controllers
{
    public class AccountController : Controller
    {


        /// <summary>
        /// 请求登录
        /// </summary>
        /// <param name="code"></param>
        /// <param name="iv"></param>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        [HttpGet]
        public string RequestLogin(string code, string iv, string encryptedData)
        {
            AccountCard accountCard = null;
            WXSmallAppCommon.Models.WXAccountInfo wXAccount = WXSmallAppCommon.WXInteractions.WXLoginAction.ProcessRequest(code, iv, encryptedData);
            if (wXAccount.openId != null)
            {
                var filter = Builders<AccountCard>.Filter.And(Builders<AccountCard>.Filter.Eq(x => x.OpenId, wXAccount.openId));
                var dbTool = new MongoDBTool();
                var collection = dbTool.GetMongoCollection<AccountCard>();
                var update = Builders<AccountCard>.Update.Set(x => x.LastLoginTime, DateTime.Now);
                accountCard = collection.FindOneAndUpdate<AccountCard>(filter, update);

                if (accountCard == null)
                {
                    AddressCard addressCard = new AddressCard();
                    addressCard.Province = wXAccount.province;
                    addressCard.City = wXAccount.city;
                    int gender = wXAccount.gender == 1 ? 1 : wXAccount.gender == 2 ? 2 : 3;
                    string avatarUrl = DownloadAvatar(wXAccount.avatarUrl, wXAccount.openId, dbTool);
                    accountCard = new AccountCard() { OpenId = wXAccount.openId, AccountName = wXAccount.nickName, Gender = gender, AvatarUrl = avatarUrl, Address = addressCard, CreateTime = DateTime.Now, LastLoginTime = DateTime.Now };
                    collection.InsertOne(accountCard);
                }
            }
            BaseResponseModel<AccountCard> responseModel = new BaseResponseModel<AccountCard>();
            int stautsCode = (int)(ActionParams.code_error);
            if (accountCard != null)
            {
                responseModel.JsonData = accountCard;
                stautsCode = (int)(ActionParams.code_ok);
            }
            responseModel.StatusCode = stautsCode;
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(new string[] { "StatusCode", "JsonData", "OpenId" });
            string jsonString = JsonConvert.SerializeObject(responseModel, jsonSerializerSettings);

            Console.WriteLine("json**3:" + jsonString);
            return jsonString;
        }

        private string DownloadAvatar(string avatarUrl, string openId, MongoDBTool dbTool)
        {

            //HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(avatarUrl);
            //httpWebRequest.Method = "GET";
            //httpWebRequest.ServicePoint.ConnectionLimit = int.MaxValue;
            //HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //string type = httpWebResponse.ContentType;
            //type = type.LastIndexOf("/") == -1 ? type : type.Substring(type.LastIndexOf("/")+1);
            //using (Stream stream = httpWebResponse.GetResponseStream())
            //{
            //    using (FileStream fs=new FileStream($@"{ConstantProperty.BaseDir}{ConstantProperty.AvatarDir}{openId}{type}",FileMode.Create))
            //    {
            //        stream.re

            //        fs.Write();
            //    }
            //}
            WebClient webClient = new WebClient();
            string saveDBName = $@"{ConstantProperty.AvatarDir}{openId}.jpg";
            string saveFileName = $@"{ConstantProperty.BaseDir}{saveDBName}";
            webClient.DownloadFile(avatarUrl, saveFileName);
          
            return saveDBName;
        }

        /// <summary>
        /// 根据openId 查询用户信息
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public string FindPersonInfo(string openId)
        {
            BaseResponseModel<AccountCard> responseModel = new BaseResponseModel<AccountCard>();

            responseModel.StatusCode = (int)ActionParams.code_ok;
            AccountCard accountCard = null;
            try
            {
                var collectiion = new MongoDBTool().GetMongoCollection<AccountCard>("AccountCard");
                var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, openId);
                var list = collectiion.Find<AccountCard>(filter);
                accountCard = list.FirstOrDefault();
                responseModel.JsonData = accountCard;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;

                //throw;
            }
            //驼峰
            //string jsonString = JsonConvert.SerializeObject(new BaseResponseModel<AccountCard>() { JsonData = accountCard, StatusCode = (int)ActionParams.code_ok }, new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });
            string jsonString = JsonConvert.SerializeObject(responseModel);

            return jsonString;
        }

        /// <summary>
        /// 修改个人昵称、性别、手机号、生日、隐私开关
        /// </summary>
        /// <param name="accountCard"></param>
        /// <returns></returns>
        public string ChangePersonInfo(AccountCard accountCard)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            responseModel.StatusCode = (int)ActionParams.code_error;
            try
            {
                var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, accountCard.OpenId);
                UpdateDefinition<AccountCard> update = null;
                if (!string.IsNullOrEmpty(accountCard.AccountName))
                {
                    update = Builders<AccountCard>.Update.Set(x => x.AccountName, accountCard.AccountName);

                }
                else if (accountCard.Birthday != DateTime.MinValue)
                {
                    update = Builders<AccountCard>.Update.Set(x => x.Birthday, accountCard.Birthday);
                }
                else if (accountCard.Gender != 0)
                {
                    update = Builders<AccountCard>.Update.Set(x => x.Gender, accountCard.Gender);
                }
                else if (!string.IsNullOrEmpty(accountCard.PhoneNumber))
                {
                    update = Builders<AccountCard>.Update.Set(x => x.PhoneNumber, accountCard.PhoneNumber);
                }
                else if (accountCard.SafeMode != 0)
                {
                    update = Builders<AccountCard>.Update.Set(x => x.SafeMode, accountCard.SafeMode);
                }
                else
                {
                    responseModel.StatusCode = (int)ActionParams.code_error_null;
                    responseModel.JsonData = "没有提交修改的参数！";
                    return JsonConvert.SerializeObject(responseModel);
                }
                new MongoDBTool().GetMongoCollection<AccountCard>().UpdateOne(filter, update);
                responseModel.StatusCode = (int)ActionParams.code_ok;
            }
            catch (Exception)
            {
                throw;
            }
            string jsonResult = JsonConvert.SerializeObject(responseModel);
            return jsonResult;
        }

        /// <summary>
        /// 查询个人地址列表
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public string FindAllLocation(string openId)
        {
            BaseResponseModel<List<LocationCard>> responseModel = new BaseResponseModel<List<LocationCard>>();
            if (openId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_error;
            try
            {
                var collectiion = new MongoDBTool().GetMongoCollection<AccountCard>();
                var list = collectiion.Find<AccountCard>(f => f.OpenId.Equals(openId));
                AccountCard accountCard = null;

                accountCard = list.FirstOrDefault();
                List<LocationCard> addressCardList = accountCard.LocationList;
                responseModel.JsonData = addressCardList;
                responseModel.StatusCode = (int)ActionParams.code_ok;
            }
            catch (Exception)
            {

                throw;
            }

            string jsonResult = JsonConvert.SerializeObject(responseModel);
            return jsonResult;
        }

        /// <summary>
        /// 添加地址
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="addressCard"></param>
        /// <returns></returns>
        public string AddLocation(string openId, LocationCard locationCard)
        {

            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            if (openId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                responseModel.JsonData = $@"参数：openId:{openId}";
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_error;
            try
            {
                Console.WriteLine("*************:" + locationCard.GPSAddress);
                var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, openId);
                var update = Builders<AccountCard>.Update.AddToSet(x => x.LocationList, locationCard);
                var collection = new MongoDBTool().GetMongoCollection<AccountCard>();
                var list = collection.Find(filter);
                var accountCard = list.FirstOrDefault();
                locationCard.Id = ObjectId.GenerateNewId();
                List<LocationCard> locationList = new List<LocationCard>();
                if (accountCard.LocationList != null)
                {
                    locationList.AddRange(accountCard.LocationList);
                }
                locationList.Add(locationCard);
                update = Builders<AccountCard>.Update.Set(x => x.LocationList, locationList);
                collection.UpdateOne(filter, update);
                responseModel.StatusCode = (int)ActionParams.code_ok;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;

                throw;
            }


            return JsonConvert.SerializeObject(responseModel);
        }

        /// <summary>
        /// 修改地区
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="addressCard"></param>
        /// <returns></returns>
        public string ChangeAddress(string openId, AddressCard addressCard)
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
                var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, openId);
                var update = Builders<AccountCard>.Update.Set(x => x.Address, addressCard);
                new MongoDBTool().GetMongoCollection<AccountCard>().UpdateOne(filter, update);
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;

                throw;
            }

            return JsonConvert.SerializeObject(responseModel);
        }


    }
}

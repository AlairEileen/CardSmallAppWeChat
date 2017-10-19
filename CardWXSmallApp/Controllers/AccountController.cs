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
                var collection = new MongoDBTool().GetMongoCollection<AccountCard>("AccountCard");
                var update = Builders<AccountCard>.Update.Set(x => x.LastLoginTime, DateTime.Now);
                accountCard = collection.FindOneAndUpdate<AccountCard>(filter, update);

                if (accountCard == null)
                {
                    accountCard = new AccountCard() { OpenId = wXAccount.openId, AccountName = wXAccount.nickName, Gender = wXAccount.gender, AvatarUrl = wXAccount.avatarUrl, City = wXAccount.city, Province = wXAccount.province, CreateTime = DateTime.Now, LastLoginTime = DateTime.Now };
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
            string jsonString = JsonConvert.SerializeObject(responseModel);

            Console.WriteLine("json**3:" + jsonString);
            return jsonString;
        }
        /// <summary>
        /// 根据openId 查询用户信息
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public string FindPersonInfo(string openId)
        {
            var collectiion = new MongoDBTool().GetMongoCollection<AccountCard>("AccountCard");
            var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, openId);
            var list = collectiion.Find<AccountCard>(f => f.OpenId.Equals(openId));
            AccountCard accountCard = null;

            accountCard = list.FirstOrDefault();
            //驼峰
            //string jsonString = JsonConvert.SerializeObject(new BaseResponseModel<AccountCard>() { JsonData = accountCard, StatusCode = (int)ActionParams.code_ok },new JsonSerializerSettings { ContractResolver=new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()});
            string jsonString = JsonConvert.SerializeObject(new BaseResponseModel<AccountCard>() { JsonData = accountCard, StatusCode = (int)ActionParams.code_ok });

            return jsonString;
        }


        /// <summary>
        /// 修改个人昵称、性别、手机号、生日、隐私开关
        /// </summary>
        /// <param name="accountCard"></param>
        /// <returns></returns>
        public string ChangePersonInfo(AccountCard accountCard)
        {
            BaseResponseModel<AccountCard> responseModel = new BaseResponseModel<AccountCard>();
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
                new MongoDBTool().GetMongoCollection<AccountCard>("AccountCard").UpdateOne(filter, update);
                responseModel.StatusCode = (int)ActionParams.code_ok;
            }
            catch (Exception)
            {
                throw;
            }
            string jsonResult = JsonConvert.SerializeObject(responseModel);

            return jsonResult;

        }
    }
}

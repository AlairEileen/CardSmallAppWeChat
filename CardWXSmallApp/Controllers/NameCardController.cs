using CardWXSmallApp.Models;
using CardWXSmallApp.ResponseModels;
using CardWXSmallApp.Tools;
using CardWXSmallApp.Tools.DB;
using Microsoft.AspNetCore.Mvc;
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

        public string ChangeNameCard(string openId, NameCard nameCard)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            responseModel.StatusCode = (int)ActionParams.code_ok;
            try
            {
                var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, openId);
                var update = Builders<AccountCard>.Update.Set(x => x.NameCard, nameCard);
                new MongoDBTool().GetMongoCollection<AccountCard>().UpdateOne(filter, update);
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

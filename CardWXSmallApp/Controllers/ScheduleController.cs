using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CardWXSmallApp.ResponseModels;
using CardWXSmallApp.Tools;
using Newtonsoft.Json;
using CardWXSmallApp.AppData.DB;
using CardWXSmallApp.Models;
using MongoDB.Driver;
using MongoDB.Bson;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CardWXSmallApp.Controllers
{
    public class ScheduleController : Controller
    {
        /// <summary>
        /// 查询所有日程
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public string FindAllSchedule(string openId)
        {
            BaseResponseModel<IEnumerable<IGrouping<string, ScheduleCard>>> responseModel = new BaseResponseModel<IEnumerable<IGrouping<string, ScheduleCard>>>();
            responseModel.StatusCode = (int)ActionParams.code_ok;

            if (openId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;
            var collection = new MongoDBTool().GetMongoCollection<AccountCard>();
            var account = collection.Find(x => x.OpenId.Equals(openId)).FirstOrDefault();
            if (account != null)
            {
                var scheduleList = account.ScheduleList;
                if (scheduleList != null)
                {
                    var list1 = scheduleList.FindAll(x => DateTime.Compare(DateTime.Now, x.Time) > 0);
                    if (list1 != null) { list1.ForEach(x => x.Status = 1); }
                    var list2 = scheduleList.FindAll(x => DateTime.Compare(DateTime.Now, x.Time) == 0);
                    if (list2 != null) { list2.ForEach(x => x.Status = 2); }
                    var list3 = scheduleList.FindAll(x => DateTime.Compare(DateTime.Now, x.Time) < 0);
                    if (list3 != null) { list3.ForEach(x => x.Status = 3); }
                    var update = Builders<AccountCard>.Update.Set(x => x.ScheduleList, scheduleList);
                    var thisAccount = collection.FindOneAndUpdate(x => x.Id.Equals(account.Id), update);
                    responseModel.JsonData = thisAccount.ScheduleList.GroupBy<ScheduleCard, string>(x => x.Status == 3 ? "已结束" : x.Status == 2 ? "执行中" : "待完成");
                }

            }
            return JsonConvert.SerializeObject(responseModel);

        }
        public string PushSchedule(string openId, ScheduleCard scheduleCard,string albumId)
        {
            BaseResponseModel<IEnumerable<IGrouping<string, ScheduleCard>>> responseModel = new BaseResponseModel<IEnumerable<IGrouping<string, ScheduleCard>>>();
            responseModel.StatusCode = (int)ActionParams.code_ok;
            responseModel.StatusCode = (int)ActionParams.code_ok;

            if (openId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }


            var dbTool = new MongoDBTool();
            var account = dbTool.GetMongoCollection<AccountCard>().Find(x => x.OpenId.Equals(openId)).FirstOrDefault();
            if (scheduleCard.Id == null)
            {
                scheduleCard.Id = ObjectId.GenerateNewId();
                account.ScheduleList.Add(scheduleCard);
            }
            else
            {
                var scheduleOld = account.ScheduleList.Find(x => x.Id.Equals(scheduleCard.Id));
                scheduleOld = scheduleCard;
            }
            var currentAlbum = account.AlbumCardList.Find(x => x.Id.Equals(new ObjectId(albumId)));
            dbTool.GetMongoCollection<AlbumCard>().InsertOne(currentAlbum);
            scheduleCard.album = new MongoDBRef(currentAlbum.GetType().ToString(), currentAlbum.Id);
            var update = Builders<AccountCard>.Update.Set(x => x.ScheduleList, account.ScheduleList);
            dbTool.GetMongoCollection<AccountCard>().UpdateOne(x => x.Id.Equals(account.Id), update);


            return JsonConvert.SerializeObject(responseModel);
        }
    }
}

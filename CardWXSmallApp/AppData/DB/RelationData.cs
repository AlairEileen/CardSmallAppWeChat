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
        #region 重建名片关联信息

        /// <summary>
        /// 重建名片关联信息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="saveName"></param>
        /// <param name="dbTool"></param>
        internal void ResetCardHolder(string openId)
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
            if (thisAccount==null)
            {
                return;
            }
            if (thisAccount.CardHolderReceive == null)
            {
                return;
            }
            ObjectId[] objectIds = new ObjectId[thisAccount.CardHolderReceive.Count];
            for (int i = 0; i < thisAccount.CardHolderReceive.Count; i++)
            {
                objectIds[i] = thisAccount.CardHolderReceive[i].Id;
            }
            var list = collection.Find(Builders<AccountCard>.Filter.In(x => x.Id, objectIds)).ToList();
            foreach (var item in list)
            {
                List<NameCardSave> saveList = new List<NameCardSave>();
                List<NameCardSave> saveListRe = new List<NameCardSave>();

                foreach (var item1 in item.CardHolder)
                {
                    if (item1.Id.Equals(thisAccount.Id))
                    {
                        item1.AvatarUrl = thisAccount.AvatarUrl;
                        item1.AccountName = thisAccount.AccountName;
                        item1.AccountNameLetterFirst = thisAccount.AccountName.GetLetterFirst();
                        item1.Post = thisAccount.NameCard != null ? thisAccount.NameCard.Post : "";
                        item1.PostLetterFirst = thisAccount.NameCard != null ? thisAccount.NameCard.Post.GetLetterFirst() : "";

                    }
                    saveList.Add(item1);
                }
                foreach (var item1 in item.CardHolderReceive)
                {
                    if (item1.Id.Equals(thisAccount.Id))
                    {
                        item1.AvatarUrl = thisAccount.AvatarUrl;
                        item1.AccountName = thisAccount.AccountName;
                        item1.AccountNameLetterFirst = thisAccount.AccountName.GetLetterFirst();
                        item1.Post = thisAccount.NameCard != null ? thisAccount.NameCard.Post : "";
                        item1.PostLetterFirst = thisAccount.NameCard != null ? thisAccount.NameCard.Post.GetLetterFirst() : "";

                    }
                    saveListRe.Add(item1);
                }
                var update = Builders<AccountCard>.Update.Set(x => x.CardHolder, saveList).Set(x => x.CardHolderReceive, saveListRe);
                collection.UpdateOne(x => x.Id.Equals(item.Id), update);
            }
        }
        #endregion
    }
}

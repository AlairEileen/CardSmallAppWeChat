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
    public class NameCardData
    {
        #region 移出或者移入名片到名片夹
        /// <summary>
        /// 移出或者移入名片到名片夹
        /// </summary>
        /// <param name="myOpenId"></param>
        /// <param name="hisOpenId"></param>
        internal void PullOrPushCardHolder(string myOpenId, string hisOpenId)
        {
            var collection = new MongoDBTool().GetMongoCollection<AccountCard>();
            var myFilter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, myOpenId);
            var hisFilter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, hisOpenId);
            var myAccountCard = collection.Find(myFilter).FirstOrDefault();
            var hisAccountCard = collection.Find(hisFilter).FirstOrDefault();
            NameCardSave mySaveCard = GetSaveCard(myAccountCard, hisOpenId, false);
            NameCardSave hisSaveCard = GetSaveCard(hisAccountCard, myOpenId, true);
            List<NameCardSave> myList = ConvertToNameCardSaveList(myAccountCard, mySaveCard, hisAccountCard, false);
            List<NameCardSave> hisList = ConvertToNameCardSaveList(hisAccountCard, hisSaveCard, myAccountCard, true);
            var myUpdate = Builders<AccountCard>.Update.Set(x => x.CardHolder, myList);
            var hisUpdate = Builders<AccountCard>.Update.Set(x => x.CardHolderReceive, hisList);
            collection.UpdateOne(myFilter, myUpdate);
            collection.UpdateOne(hisFilter, hisUpdate);
        }
        private List<NameCardSave> ConvertToNameCardSaveList(AccountCard myAccountCard, NameCardSave saveCard, AccountCard hisAccountCard, bool isReceiver)
        {
            List<NameCardSave> ncsList = isReceiver ? myAccountCard.CardHolderReceive : myAccountCard.CardHolder;
            List<NameCardSave> list = new List<NameCardSave>();
            if (saveCard == null)
            {
                list.AddRange(ncsList);
                NameCardSave nameCardSave = null;
                if (hisAccountCard.NameCard == null)
                {
                    nameCardSave = new NameCardSave() { Id = hisAccountCard.Id, AccountName = hisAccountCard.AccountName, PhoneNumber = hisAccountCard.PhoneNumber, OpenId = hisAccountCard.OpenId, AvatarUrl = hisAccountCard.AvatarUrl, AccountNameLetterFirst = hisAccountCard.AccountName.GetLetterFirst() };
                }
                else
                {
                    nameCardSave = new NameCardSave() { Id = hisAccountCard.Id, AccountName = hisAccountCard.AccountName, PhoneNumber = hisAccountCard.PhoneNumber, OpenId = hisAccountCard.OpenId, Post = hisAccountCard.NameCard.Post, AvatarUrl = hisAccountCard.AvatarUrl, PostLetterFirst = hisAccountCard.NameCard.Post.GetLetterFirst(), AccountNameLetterFirst = hisAccountCard.AccountName.GetLetterFirst() };
                }

                list.Add(nameCardSave);
            }
            else
            {
                ncsList.Remove(saveCard);
                list.AddRange(ncsList);
            }
            return list;
        }
        private NameCardSave GetSaveCard(AccountCard accountCard, string openId, bool isRecerver)
        {
            List<NameCardSave> ncsList = isRecerver ? accountCard.CardHolderReceive : accountCard.CardHolder;
            if (ncsList == null)
            {
                ncsList = new List<NameCardSave>();
                if (isRecerver)
                {
                    accountCard.CardHolderReceive = ncsList;
                }
                else
                {
                    accountCard.CardHolder = ncsList;
                }
                return null;
            }
            else
            {
                return ncsList.Find(x => x.OpenId.Equals(openId));
            }
        }
        #endregion
       
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

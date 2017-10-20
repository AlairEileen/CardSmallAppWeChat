using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Tools.DB
{

    public class MongoDBTool
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        private const string conn = "mongodb://127.0.0.1:27027";
        /// <summary>
        /// 指定的数据库
        /// </summary>
        private const string dbName = "name_card_wxxcx";
        /// <summary>
        /// 指定的表
        /// </summary>
        private const string tbName = "table_text";

        public IMongoDatabase GetMongoDatabase()
        {
            MongoClient mongoClient = new MongoClient(conn);
            return mongoClient.GetDatabase(dbName);
        }

        public IMongoCollection<T> GetMongoCollection<T>()
        {
            string packageName = typeof(T).ToString();
            string collectionName = packageName.Substring(packageName.LastIndexOf(".")+1);
            return GetMongoDatabase().GetCollection<T>(collectionName);
        }
    }
}

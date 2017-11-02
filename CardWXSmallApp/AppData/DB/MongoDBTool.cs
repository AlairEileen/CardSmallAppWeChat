using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.AppData.DB
{

    public class MongoDBTool
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        private const string conn = "mongodb://localhost:27027";
        /// <summary>
        /// 指定的数据库
        /// </summary>
        private const string dbName = "name_card_wxxcx";
        /// <summary>
        /// 指定的表
        /// </summary>
        private const string tbName = "table_text";

        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <returns>当前数据库</returns>
        private IMongoDatabase GetMongoDatabase()
        {
            MongoClient mongoClient = new MongoClient(conn);
            return mongoClient.GetDatabase(dbName);
        }
        /// <summary>
        /// 获取集合
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <returns>该类型集合</returns>
        public IMongoCollection<T> GetMongoCollection<T>()
        {
            string packageName = typeof(T).ToString();
            string collectionName = packageName.Substring(packageName.LastIndexOf(".")+1);
            return GetMongoDatabase().GetCollection<T>(collectionName);
        }
        /// <summary>
        /// 获取集合
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <param name="collectionName">集合名称</param>
        /// <returns>该类型集合</returns>
        public IMongoCollection<T> GetMongoCollection<T>(string collectionName)
        {
            return GetMongoDatabase().GetCollection<T>(collectionName);
        }
    }
}

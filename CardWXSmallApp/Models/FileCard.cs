using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Models
{
    public class FileCard<T>
    {
        [JsonConverter(typeof(Tools.ObjectIdConverter))]
        public ObjectId Id { get; set; }
        /// <summary>
        /// 0：源文件，1：中压缩文件，2：最小文件
        /// </summary>
        public T FileUrlData { get; set; }
    }
}

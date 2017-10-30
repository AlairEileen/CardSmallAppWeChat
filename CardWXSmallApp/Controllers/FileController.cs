using System;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using CardWXSmallApp.Tools;
using System.IO;
using MongoDB.Driver;
using CardWXSmallApp.Models;
using Microsoft.AspNetCore.Hosting;
using CardWXSmallApp.ResponseModels;
using Newtonsoft.Json;
using System.Threading;
using MongoDB.Bson;
using System.Collections.Generic;
using CardWXSmallApp.AppData.DB;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CardWXSmallApp.Controllers
{
    public class FileController : Controller
    {

        RelationData relationData = new RelationData();
        private IHostingEnvironment hostingEnvironment;
        public FileController(IHostingEnvironment environment)
        {
            this.hostingEnvironment = environment;
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="fileId"></param>
        [HttpGet]
        public IActionResult FileDownload(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return null;
            }
            fileUrl = ConstantProperty.BaseDir + fileUrl;
            var stream = System.IO.File.OpenRead(fileUrl);
            return File(stream, "application/vnd.android.package-archive", Path.GetFileName(fileUrl));
        }

        /// <summary>
        /// 头像上传
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public string UploadAvatar(string openId)
        {
            BaseResponseModel<String> responseModel = new BaseResponseModel<String>();
            if (openId == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                responseModel.JsonData = $@"参数：openId:{openId}";
                return JsonConvert.SerializeObject(responseModel);
            }
            long size = 0;
            var files = Request.Form.Files;


            try
            {
                foreach (var file in files)
                {
                    var filename = ContentDispositionHeaderValue
                                    .Parse(file.ContentDisposition)
                                    .FileName
                                    .Trim('"');
                    string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.AvatarDir}";
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    filename = filename.Substring(filename.LastIndexOf("."));
                    string saveName = ConstantProperty.AvatarDir + $@"{openId}{filename}";
                    filename = ConstantProperty.BaseDir + saveName;
                    size += file.Length;
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                        var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, openId);
                        var update = Builders<AccountCard>.Update.Set(x => x.AvatarUrl, saveName);
                        var dbTool = new MongoDBTool();
                        dbTool.GetMongoCollection<AccountCard>().UpdateOne(filter, update);
                        UpdateAvatar(openId, saveName, dbTool);
                    }

                }
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
        /// 更新头像后的联动更新
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="saveName"></param>
        /// <param name="dbTool"></param>
        private void UpdateAvatar(string openId, string saveName, MongoDBTool dbTool)
        {
            //UpdateCardHolder(openId,saveName,dbTool);
            //nameCardData.ResetCardHolder(openId);
            relationData.ResetRelationInfo(openId);
        }
       
        /// <summary>
        /// 更新名片夹头像信息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="saveName"></param>
        /// <param name="dbTool"></param>
        private void UpdateCardHolder(string openId, string saveName, MongoDBTool dbTool)
        {
            var collection = dbTool.GetMongoCollection<AccountCard>();
            var thisAccount = collection.Find(x => x.OpenId.Equals(openId)).FirstOrDefault();
            ObjectId[] objectIds = new ObjectId[thisAccount.CardHolderReceive.Count];
            for (int i = 0; i < thisAccount.CardHolderReceive.Count; i++)
            {
                objectIds[i] = thisAccount.CardHolderReceive[i].Id;
            }
            var listFilter = Builders<AccountCard>.Filter.In(x => x.Id, objectIds);
            var list = collection.Find(listFilter).ToList();
            foreach (var item in list)
            {
                List<NameCardSave> saveList = new List<NameCardSave>();
                List<NameCardSave> saveListRe = new List<NameCardSave>();

                foreach (var item1 in item.CardHolder)
                {
                    if (item1.Id.Equals(thisAccount.Id))
                    {
                        item1.AvatarUrl = saveName;

                    }
                    saveList.Add(item1);
                }
                foreach (var item1 in item.CardHolderReceive)
                {
                    if (item1.Id.Equals(thisAccount.Id))
                    {
                        item1.AvatarUrl = saveName;

                    }
                    saveListRe.Add(item1);
                }
                var update = Builders<AccountCard>.Update.Set(x => x.CardHolder, saveList).Set(x => x.CardHolderReceive, saveListRe);
                collection.UpdateOne(x => x.Id.Equals(item.Id), update);
            }
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public string UploadImage(string openId)
        {
            long size = 0;
            var files = Request.Form.Files;
            string resultFileId = null;
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();

            try
            {
                foreach (var file in files)
                {
                    var filename = ContentDispositionHeaderValue
                                    .Parse(file.ContentDisposition)
                                    .FileName
                                    .Trim('"');
                    string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.AlbumDir}";
                    string dbSaveDir = $@"{ConstantProperty.AlbumDir}";
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    string exString = filename.Substring(filename.LastIndexOf("."));
                    string saveName = Guid.NewGuid().ToString("N");
                    filename = $@"{saveDir}{saveName}{exString}";

                    size += file.Length;
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                        string[] fileUrls = new string[] { $@"{dbSaveDir}{saveName}{exString}" };
                        FileCard<string[]> fileCard = new FileCard<string[]>() { FileUrlData = fileUrls };
                        new MongoDBTool().GetMongoCollection<FileCard<string[]>>("FileCard").InsertOne(fileCard);
                        resultFileId = fileCard.Id.ToString();
                    }
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Create3Img), new string[] { filename, resultFileId });
                }
                responseModel.StatusCode = (int)ActionParams.code_ok;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
            }
            responseModel.JsonData = resultFileId;
            //return JsonConvert.SerializeObject(responseModel);
            return resultFileId;
        }

        /// <summary>
        /// 三级图片生成
        /// </summary>
        /// <param name="state"></param>
        private void Create3Img(object state)
        {
            string[] data = (string[])state;
            string fileId = data[1];
            string fileName = data[0];
            string exString = fileName.Substring(fileName.LastIndexOf("."));
            string headString = fileName.Substring(0, fileName.LastIndexOf("."));
            string nameString = headString.Substring(headString.LastIndexOf("/") + 1);
            nameString = $@"{ConstantProperty.AlbumDir }{nameString}";
            string fileName1 = $@"{headString}_1{exString}";
            string fileName1Db = $@"{nameString}_1{exString}";
            string fileName2 = $@"{headString}_2{exString}";
            string fileName2Db = $@"{nameString}_2{exString}";
            string error1 = "", error2 = "";
            string imgType = fileName.Substring(fileName.LastIndexOf(".")+1).ToLower().Equals("jpg")?"image/jpg":"image/png";
            bool hasFile1 = new ImageTool().GetCompressImage(fileName, fileName1, 800, 800, 80, out error1,imgType);
            bool hasFile2 = new ImageTool().GetCompressImage(fileName, fileName2, 200, 200, 60, out error2,imgType);
            List<string> fileUrls = new List<string>();
            fileUrls.Add($@"{nameString}{exString}");
            if (hasFile1 && hasFile2)
            {
                fileUrls.Add(fileName1Db);
                fileUrls.Add(fileName2Db);
            }
            else if (!hasFile1 && hasFile2)
            {
                fileUrls.Add($@"{nameString}{exString}");
                fileUrls.Add(fileName2Db);
            }
            else if (hasFile1 && !hasFile2)
            {
                fileUrls.Add(fileName1Db);
                fileUrls.Add($@"{nameString}{exString}");
            }
            else if (!hasFile1&&!hasFile2)
            {
                fileUrls.Add($@"{nameString}{exString}");
                  fileUrls.Add($@"{nameString}{exString}");
            }

            if (fileUrls.Count != 1)
            {
                var collection = new MongoDBTool().GetMongoCollection<FileCard<string[]>>("FileCard");
                var filter = Builders<FileCard<string[]>>.Filter.Eq(x => x.Id, new ObjectId(fileId));
                var update = Builders<FileCard<string[]>>.Update.Set(x => x.FileUrlData, fileUrls.ToArray());
                collection.UpdateOne(filter, update);
            }

        }


    }
}

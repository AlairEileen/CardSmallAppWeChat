using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using CardWXSmallApp.Tools;
using System.IO;
using MongoDB.Driver;
using CardWXSmallApp.Tools.DB;
using CardWXSmallApp.Models;
using Microsoft.AspNetCore.Hosting;
using CardWXSmallApp.ResponseModels;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CardWXSmallApp.Controllers
{
    public class FileController : Controller
    {


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
            long size = 0;
            var files = Request.Form.Files;
            BaseResponseModel<String> responseModel = new BaseResponseModel<String>();

            try
            {
                foreach (var file in files)
                {
                    var filename = ContentDispositionHeaderValue
                                    .Parse(file.ContentDisposition)
                                    .FileName
                                    .Trim('"');
                    if (!Directory.Exists(ConstantProperty.AvatarDir))
                    {
                        Directory.CreateDirectory(ConstantProperty.AvatarDir);
                    }
                    filename = filename.Substring(filename.LastIndexOf("."));
                    string saveName = ConstantProperty.AvatarDir+$@"{openId}{filename}";
                    filename = ConstantProperty.BaseDir + saveName;
                    size += file.Length;
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                        var filter = Builders<AccountCard>.Filter.Eq(x => x.OpenId, openId);
                        var update = Builders<AccountCard>.Update.Set(x => x.AvatarUrl, "http://" + Request.Host + "/File/FileDownload" + "?fileUrl=" + saveName);
                        new MongoDBTool().GetMongoCollection<AccountCard>("AccountCard").UpdateOne(filter, update);
                    }

                }
                responseModel.StatusCode = (int)ActionParams.code_ok;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
            }
            return JsonConvert.SerializeObject(responseModel);
        }

    }
}

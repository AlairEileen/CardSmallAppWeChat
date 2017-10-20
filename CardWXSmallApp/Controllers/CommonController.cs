using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CardWXSmallApp.Tools.DB;
using CardWXSmallApp.Models;
using MongoDB.Driver;
using CardWXSmallApp.Tools;
using System.IO;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CardWXSmallApp.Controllers
{
    public class CommonController : Controller
    {
      public string getProvince()
        {
            ResetProvinceCity();
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(new string[] { "code", "name", "initial" });
            string jsonString = JsonConvert.SerializeObject(ConstantProperty.CityProvinceList,jsonSerializerSettings);
            return jsonString;
        }
        public string getAllProvince()
        {
            ResetProvinceCity();
            string jsonString = JsonConvert.SerializeObject(ConstantProperty.CityProvinceList);
            return jsonString;
        }
        public string getProvinceCity()
        {
            string json = null;
            using (StreamReader fs = new StreamReader("ProvinceCity.json"))
            {
                json = fs.ReadToEnd();
            }
            SimpleProvinceCity list = null;
            if (json != null)
            {
                list = JsonConvert.DeserializeObject<SimpleProvinceCity>(json);
            }
            return JsonConvert.SerializeObject(list);
        }
        private void ResetProvinceCity()
        {
            if (ConstantProperty.CityProvinceList == null || ConstantProperty.CityProvinceList.Count == 0)
            {
                string json = null;
                using (StreamReader fs = new StreamReader("city.json"))
                {
                    json = fs.ReadToEnd();
                }
                List<ProvinceCity> list = null;
                if (json != null)
                {
                    list = JsonConvert.DeserializeObject<List<ProvinceCity>>(json);
                }
                ConstantProperty.CityProvinceList = list;
            }
        }

        public string ZipPic()
        {
            GetPicThumbnail(@"‪\Home\project_data\mpxcx\avatar\oeEn70L9nA_k16rFyQCXaycrO0mI.jpg", @"‪\Home\project_data\mpxcx\avatar\01.jpg", 600, 800, 80);


            return "4343";
        }



        /// 无损压缩图片  
        /// <param name="sFile">原图片</param>  
        /// <param name="dFile">压缩后保存位置</param>  
        /// <param name="dHeight">高度</param>  
        /// <param name="dWidth"></param>  
        /// <param name="flag">压缩质量(数字越小压缩率越高) 1-100</param>  
        /// <returns></returns>  

        public static bool GetPicThumbnail(string sFile, string dFile, int dHeight, int dWidth, int flag)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            int sW = 0, sH = 0;

            //按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);

            if (tem_size.Width > dHeight || tem_size.Width > dWidth)
            {
                if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = (tem_size.Width * dHeight) / tem_size.Height;
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap ob = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();
            //以下代码为保存图片时，设置压缩质量  
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100  
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径  
                }
                else
                {
                    ob.Save(dFile, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                ob.Dispose();
            }
        }


    }
}

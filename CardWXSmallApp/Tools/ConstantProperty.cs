using CardWXSmallApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Tools
{
    public class ConstantProperty
    {
        public const string BaseDir = "/home/project_data/mpxcx/";
        public const string AvatarDir = "avatar/";
        public const string TempDir = "temp/";
        public const string AlbumDir = "album/";
        /// <summary>
        /// 省市区对象
        /// </summary>
        public static List<ProvinceCity> CityProvinceList{ get; set; }
    }
}

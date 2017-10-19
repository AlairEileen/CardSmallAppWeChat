using System;
using System.Collections.Generic;
using System.Text;

namespace WXSmallAppCommon.Models
{
  public  class WXAccountInfo
    {
        public string openId { get; set; }
        public string nickName { get; set; }
        public Int16 gender { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string avatarUrl { get; set; }
        public string unionId { get; set; }
        public WXWatermark watermark { get; set; }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using WXSmallAppCommon.Models;

namespace WXSmallAppCommon.WXInteractions
{
    public class WXLoginAction
    {
        const string Appid = "wx7c68d4919b2a0be2";
        const string Secret = "f9b316b0effe0c9cfb4d6dab8f9dca87";
        public static WXAccountInfo ProcessRequest(string code, string iv, string encryptedData)
        {
            Console.WriteLine("in&&&");
            string grant_type = "authorization_code";
            WXAccountInfo userInfo = new WXAccountInfo();
            //向微信服务端 使用登录凭证 code 获取 session_key 和 openid 
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid=" + Appid + "&secret=" + Secret + "&js_code=" + code + "&grant_type=" + grant_type;
            string type = "utf-8";

            WXAccountInfoGetter GetUsersHelper = new WXAccountInfoGetter();
            string j = WXAccountInfoGetter.GetUrltoHtml(url, type);//获取微信服务器返回字符串

            //将字符串转换为json格式
            JObject jo = (JObject)JsonConvert.DeserializeObject(j);

            WXResult res = new WXResult();
            try
            {
                //微信服务器验证成功
                res.openid = jo["openid"].ToString();
                res.session_key = jo["session_key"].ToString();
            }
            catch (Exception)
            {
                //微信服务器验证失败
                res.errcode = jo["errcode"].ToString();
                res.errmsg = jo["errmsg"].ToString();
            }
            if (!string.IsNullOrEmpty(res.openid))
            {
                //用户数据解密
                WXAccountInfoGetter.AesIV = iv;
                WXAccountInfoGetter.AesKey = res.session_key;
                Console.WriteLine("iv:{0},aeskey:{1}", iv, res.session_key);
                string result = GetUsersHelper.AESDecrypt(encryptedData);

                Console.WriteLine("result:" + result);
                //存储用户数据
                JObject _usrInfo = (JObject)JsonConvert.DeserializeObject(result);


                userInfo.openId = _usrInfo["openId"].ToString();
                Console.WriteLine("openId:" + userInfo.openId);

                try //部分验证返回值中没有unionId
                {
                    userInfo.unionId = _usrInfo["unionId"].ToString();
                }
                catch (Exception)
                {
                    userInfo.unionId = "unionId";
                }
                Console.WriteLine("unionId:" + userInfo.unionId);

                userInfo.nickName = _usrInfo["nickName"].ToString();
                userInfo.gender =Convert.ToInt16(_usrInfo["gender"].ToString());
                userInfo.city = _usrInfo["city"].ToString();
                userInfo.province = _usrInfo["province"].ToString();
                userInfo.country = _usrInfo["country"].ToString();
                userInfo.avatarUrl = _usrInfo["avatarUrl"].ToString();

                object watermark = _usrInfo["watermark"].ToString();
                object appid = _usrInfo["watermark"]["appid"].ToString();
                object timestamp = _usrInfo["watermark"]["timestamp"].ToString();
            }
            return userInfo;
        }
    }
}

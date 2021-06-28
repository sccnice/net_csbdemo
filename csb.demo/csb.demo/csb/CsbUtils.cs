using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace csb.demo.csb
{
    public class Constants
    {
        public  const String ACCESS_KEY = "aa";
    public const String SECRET_KEY = "a";
    public const String CSB_ADDR = "http://ip/CSB";

}


    public class CsbUtils
    {



        public static string GetResult_Get(string apiName,Dictionary<string, object[]> formParamDict) {

            //签名时间戳
            long timeStamp = DateTime.Now.ToUnixTimeMilliseconds();

            //            long timeStamp = 1592225468715;
            //form表单提交的签名串生成示例
            string signature = sign(apiName, "1.0.0", timeStamp, Constants.ACCESS_KEY, Constants.SECRET_KEY, formParamDict, null);

            string postData = InterfaceProxy.GetFormUrlencoded(formParamDict);
        
            Dictionary<string, string> headerDic = new Dictionary<string, string>();

            //以下公共header
            headerDic.Add("_api_timestamp", timeStamp.ToString());
            headerDic.Add("_api_name", apiName);  //getOrgs
            headerDic.Add("_api_signature", signature);
            headerDic.Add("_api_version", "1.0.0");
            headerDic.Add("_api_access_key", Constants.ACCESS_KEY);

            //get时额外header(参考java_sdk源码)
            headerDic.Add("Accept-Encoding", "gzip");
            headerDic.Add("_inner_ecsb_request_id", "c0a8694816245153618221001d41d0");  //请求id随机一个\
            headerDic.Add("_inner_ecsb_rpc_id", "0");
            headerDic.Add("_inner_ecsb_trace_id", "c0a8694816245141995961002d3ef4"); // _inner_ecsb_trace_id随机一个\

            var httpUrl = Constants.CSB_ADDR + "?" + postData;
            var resultStr= InterfaceProxy.GetResult(httpUrl, "application/json", "application/x-www-form-urlencoded;charset=UTF-8",  "GET", "", headerDic);

            return resultStr;



            //json或xml文本提交的签名串生成示例
            //string signature = sign("http2http11", "1.0.0", timeStamp, Constants.ACCESS_KEY, Constants.SECRET_KEY, null, "{\"name\":\"中文name1\", \"times\":\"123\" }");
        }

        /// <summary>
        /// 请求方式post,body为json
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static string GetResult_Json(string apiName, string postData)
        {

            //签名时间戳
            long timeStamp = DateTime.Now.ToUnixTimeMilliseconds();

            //Dictionary<string, object[]> formParamDict = new Dictionary<string, object[]>();
            //formParamDict.Add("name", new string[] { "中文name1" });
            //formParamDict.Add("times", new object[] { 123 });
            //formParamDict.Add("multiValues", new object[] { "abc", "efg" });
            //            long timeStamp = 1592225468715;
            //form表单提交的签名串生成示例
            string signature = sign(apiName, "1.0.0", timeStamp, Constants.ACCESS_KEY, Constants.SECRET_KEY, null, postData);

         //   string postData = InterfaceProxy.GetFormUrlencoded(formParamDict);

            Dictionary<string, string> headerDic = new Dictionary<string, string>();

            //            _api_timestamp: 1592225468715
            //_api_name: http2http11
            // _api_signature:签名值
            // _api_version:1.0.0
            //_api_access_key: ak123
            headerDic.Add("_api_timestamp", timeStamp.ToString());
            headerDic.Add("_api_name", apiName);  //getOrgs
            headerDic.Add("_api_signature", signature);
            headerDic.Add("_api_version", "1.0.0");
            headerDic.Add("_api_access_key", Constants.ACCESS_KEY);

            var resultStr = InterfaceProxy.GetResult(Constants.CSB_ADDR, "application/json", "application/json;charset=utf-8", "POST", postData, headerDic);

            return resultStr;



            //json或xml文本提交的签名串生成示例
            //string signature = sign("http2http11", "1.0.0", timeStamp, Constants.ACCESS_KEY, Constants.SECRET_KEY, null, "{\"name\":\"中文name1\", \"times\":\"123\" }");
        }


        /// <summary>
        /// 本方法生成http请求的csb签名值。
        /// 调用csb服务时，需要在httpheader中增加以下几个头信息：
        /// _api_name: csb服务名
        /// _api_version: csb服务版本号
        /// _api_access_key: csb上的凭证ak
        /// _api_timestamp: 时间戳
        /// _api_signature: 本方法返回的签名串
        /// </summary>
        /// <param name="apiName">csb服务名</param>
        /// <param name="apiVersion">csb服务版本号</param>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="accessKey">csb上的凭证ak</param>
        /// <param name="secretKey">csb上凭证的sk</param>
        /// <param name="formParamDict">form表单提交的参数列表(各参数值是还未urlEncoding编码的原始业务参数值)。如果是form提交，请使用 Content-Type= application/x-www-form-urlencoded </param>
        /// <param name="body">非form表单方式提交的请求内容，目前没有参与签名计算</param>
        /// <returns>签名串。</returns>
        public static string sign(string apiName, string apiVersion, long timeStamp, string accessKey, string secretKey, Dictionary<string, object[]> formParamDict, object body)
        {
            Dictionary<string, object[]> newDict = new Dictionary<string, object[]>();
            if (formParamDict != null)
            {
                foreach (KeyValuePair<string, object[]> pair in formParamDict)
                {
                    newDict.Add(pair.Key, pair.Value.Select(v => { return HttpUtility.UrlEncode(v.ToString()); }
                    ).ToArray());
                }
            }

            //设置csb要求的头参数
            newDict.Add("_api_name", new String[] { apiName });
            newDict.Add("_api_version", new String[] { apiVersion });
            newDict.Add("_api_access_key", new String[] { accessKey });
            newDict.Add("_api_timestamp", new object[] { timeStamp });

            //对所有参数进行排序
            //var sortedDict = from pair 
            //                 in newDict
            //                 orderby pair.Key
            //                 select pair;
          var  sortedDict = getSortByASCII(newDict);
           StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, object[]> pair in sortedDict)
            {
                foreach (object obj in pair.Value)
                {
                    builder.Append(string.Format("{0}={1}&", pair.Key, obj));
                }
            }
            string str = builder.ToString();
            if (str.EndsWith("&"))
            {
                str = str.Substring(0, str.Length - 1); //去掉最后一个多余的 & 符号
            }
            System.Security.Cryptography.HMACSHA1 hmacsha = new System.Security.Cryptography.HMACSHA1
            {
                Key = Encoding.UTF8.GetBytes(secretKey)
            };
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(hmacsha.ComputeHash(bytes));
        }


        public static Dictionary<string, object[]> getSortByASCII(Dictionary<string, object[]> dict) {

            var sortList = dict.ToList();
            //这种排序是错误的
            sortList.Sort((x, y) => {

                var r = string.Compare(x.Key, y.Key);
                return r;
            });
            //经过测试以下三种是正确的，会按照ASCII码排序
            sortList.Sort((x, y) => {

               return string.CompareOrdinal(x.Key, y.Key);
            });
            sortList.Sort((x, y) => {

                var r = string.Compare(x.Key, y.Key, StringComparison.Ordinal);
                return r;
            });

            Dictionary<string, object[]> sortDict = new Dictionary<string, object[]>();
            var keys = dict.Keys.ToArray();
            Array.Sort(keys, string.CompareOrdinal); //an
            foreach (var key in keys) {
                sortDict.Add(key, dict[key]);
            }
            return sortDict;

        }

        
    }
}

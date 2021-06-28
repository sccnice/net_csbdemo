using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace csb.demo.csb
{
    public class InterfaceProxy
    {
        public static string GetResult(string url, string accept, string contentType, string requestType, string parms,Dictionary<string,string> headerDic=null)
        {

            var result = string.Empty;

            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.Timeout = 300000;//100秒超时时间较短，需要调整成为300秒
                if (!string.IsNullOrEmpty(accept))
                {
                    myRequest.Accept = accept;
                }
                if (!string.IsNullOrEmpty(contentType))
                {
                    myRequest.ContentType = contentType;
                }
                //   myRequest.Headers.Add(new HttpRequestHeader(),"123");
                myRequest.Method = requestType;
                if (myRequest.Method == "POST")
                {
                    var reqStream = myRequest.GetRequestStream();
                    var encoding = Encoding.GetEncoding("utf-8");
                    var inData = encoding.GetBytes(parms);
                    reqStream.Write(inData, 0, inData.Length);
                    reqStream.Close();
                }
             
                if (headerDic != null) {
                    foreach (var h in headerDic) {
                        myRequest.Headers.Add(h.Key, h.Value);
                             }
                }
                //发送post请求到服务器并读取服务器返回信息    
                var res = (HttpWebResponse)myRequest.GetResponse();

                if (res.StatusCode != HttpStatusCode.OK) return result;

                var receiveStream = res.GetResponseStream();
                var encode = Encoding.GetEncoding("utf-8");
                if (receiveStream != null)
                {
                    var readStream = new StreamReader(receiveStream, encode);
                    var oResponseMessage = readStream.ReadToEnd();
                    res.Close();
                    readStream.Close();
                    result = oResponseMessage;
                }
                stopWatch.Stop();

            }
            catch (Exception e)
            {
                if (e is WebException && ((WebException)e).Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse errResp = ((WebException)e).Response;
                    using (Stream respStream = errResp.GetResponseStream())
                    {
                        var readStream = new StreamReader(respStream, Encoding.GetEncoding("utf-8"));
                        var strError = readStream.ReadToEnd();
                        Console.Out.WriteLine(strError);
                        // read the error response
                    }
                }
            }
            return result;
        }

        public static string GetFormUrlencoded(Dictionary<string, object[]> dict) {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, object[]> pair in dict)
            {
                foreach (object obj in pair.Value)
                {
                    builder.Append(string.Format("{0}={1}&", pair.Key, HttpUtility.UrlEncode(obj.ToString()) ));
                }
            }
            string str = builder.ToString();
            if (str.EndsWith("&"))
            {
                str = str.Substring(0, str.Length - 1); //去掉最后一个多余的 & 符号
            }
            return str;
        }
    }
}

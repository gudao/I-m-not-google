﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace GoogleDelegate.Controllers
{
    public class HomeController : Controller
    {
        private const string GoogleHost = "http://www.google.com.hk";
        //private const string GoogleHost = "http://www.baidu.com";
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Search()
        {
            List<string> removeParams = new List<string>();
            removeParams.Add("btnG");
            string rep = WebUtils.DoGet(GoogleHost + "/search?", Request.QueryString, removeParams);
            rep = ChineseConverter.ToSimplified(rep);
            rep = rep.Replace("zh-HK", "zh-CN");
            ViewBag.HtmlContent = rep;
            return View();
        }

        public new ActionResult Url(string q)
        {
            if (!string.IsNullOrEmpty(q))
                return Redirect(q);
            else
                return new EmptyResult();
        }
    }
    public class WebUtils
    {
        public static string DoGet(string url, NameValueCollection parameters,List<string> removeList)
        {
            if (parameters != null && parameters.Count > 0)
            {
                if (url.Contains("?"))
                {
                    url = url + "&" + BuildQuery(parameters,removeList);
                }
                else
                {
                    url = url + "?" + BuildQuery(parameters,removeList);
                }
            }

            HttpWebRequest req = GetWebRequest(url, "GET");
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

            HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
            Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);
            return GetResponseAsString(rsp, encoding);
        }
        

        private static HttpWebRequest GetWebRequest(string url, string method)
        {
            HttpWebRequest req = null;
            req = (HttpWebRequest)WebRequest.Create(url);
            req.ServicePoint.Expect100Continue = false;
            req.Method = method;
            req.KeepAlive = true;
            req.UserAgent = "MyAgent";
            req.Timeout = 100000;

            return req;
        }

        /// <summary>
        /// 把响应流转换为文本。
        /// </summary>
        /// <param name="rsp">响应流对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>响应文本</returns>
        private static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            System.IO.Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }

        /// <summary>
        /// 组装GET请求URL。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>带参数的GET请求URL</returns>
        public static string BuildGetUrl(string url, NameValueCollection parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                if (url.Contains("?"))
                {
                    url = url + "&" + BuildQuery(parameters);
                }
                else
                {
                    url = url + "?" + BuildQuery(parameters);
                }
            }
            return url;
        }
        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        private static string BuildQuery(NameValueCollection parameters,List<string> removeList=null)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;
            foreach (string key in parameters.AllKeys)
            {
                if (removeList != null && removeList.Contains(key))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(parameters[key]))
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }

                    postData.Append(key);
                    postData.Append("=");
                    postData.Append(HttpUtility.UrlEncode(parameters[key], Encoding.UTF8));
                    hasParam = true;
                }
            }
            return postData.ToString();
        }
    }
    public static class ChineseConverter
    {
        internal const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        internal const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        internal const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        /// <summary>
        /// 使用OS的kernel.dll做为简繁转换工具，只要有裝OS就可以使用，不用需额外引用dll，但只能做逐字转换，无法进行词意的转换
        /// <para>所以无法将电脑转成计算机</para>
        /// </summary>
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

        /// <summary>
        /// 繁体转简体
        /// </summary>
        /// <param name="pSource">要转换的繁体字：體</param>
        /// <returns>转换后的简体字：体</returns>
        public static string ToSimplified(string pSource)
        {
            if (string.IsNullOrEmpty(pSource) || pSource.Length < 1)
                return "";

            String tTarget = new String(' ', pSource.Length);
            int tReturn = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, pSource, pSource.Length, tTarget, pSource.Length);
            return tTarget;
        }

        /// <summary>
        /// 简体转繁体
        /// </summary>
        /// <param name="pSource">要转换的简体字：体</param>
        /// <returns>转换后的繁体字：體</returns>
        public static string ToTraditional(string pSource)
        {
            if (string.IsNullOrEmpty(pSource) || pSource.Length < 1)
                return "";

            String tTarget = new String(' ', pSource.Length);
            int tReturn = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, pSource, pSource.Length, tTarget, pSource.Length);
            return tTarget;
        }
    }

}

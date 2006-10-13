using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Utility
{
   public class URI
   {
      private URI()
      {
      }

      static string StripSchemeFromURI(string strScheme, string strURI)
      {
         string strPre = strScheme + "://";
         if (!strURI.StartsWith(strPre))
            throw new ApplicationException("The URI: \"" + strURI + "\" is not a valid URI for the " + strScheme + " protocol.");

         return strURI.Replace(strPre, "");
      }

      public static string HostFromURI(string strScheme, string strURI)
      {
         string strHost = StripSchemeFromURI(strScheme, strURI);
         if (strHost.IndexOf("/") != -1)
            strHost.Substring(0, strHost.IndexOf("/"));
         if (strHost.IndexOf("?") != -1)
            strHost.Substring(0, strHost.IndexOf("?"));

         return strHost;
      }

      public static string PathFromURI(string strScheme, string strURI)
      {
         string strPath = StripSchemeFromURI(strScheme, strURI);

         if (strPath.IndexOf("/") != -1)
            strPath.Substring(strPath.IndexOf("/") + 1);
         else
            return String.Empty;

         if (strPath.IndexOf("?") != -1)
            strPath.Substring(0, strPath.IndexOf("?"));

         return strPath;
      }

      public static string QueryFromURI(string strScheme, string strURI)
      {
         string strQuery = StripSchemeFromURI(strScheme, strURI);

         if (strQuery.IndexOf("?") != -1)
            return strQuery.Substring(strQuery.IndexOf("?") + 1);
         else
            return String.Empty;
      }


      static public NameValueCollection ParseURI(string strScheme, string strURI, ref string strHost, ref string strPath)
      {
         strHost = HostFromURI(strScheme, strURI);
         strPath = PathFromURI(strScheme, strURI);
         return HttpUtility.ParseQueryString(QueryFromURI(strScheme, strURI));
      }

      public static string CreateURI(string strScheme, string strHost, string strPath, NameValueCollection queryColl)
      {
         bool bFirst = true;
         string strURI = strScheme + "://" + strHost;
         if (!String.IsNullOrEmpty(strPath))
            strURI += "/" + strPath;

         string strQuery = String.Empty;
         for (int i = 0; i < queryColl.Count; i++)
         {
            if (!bFirst)
               strQuery += "&";
            else
               bFirst = false;
            
            strQuery += queryColl.GetKey(i) + "=" + HttpUtility.UrlEncode(queryColl.Get(i));
         }

         if (strQuery != String.Empty)
            strURI += "?" + strQuery;

         return strURI;
      }
   }
}
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Net;
using System.Data.SqlTypes;

public partial class StoredProcedures
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void SqlSMSService(string messageBody, string mobiles,string authkey,string sender, out SqlString retValue)
    {
        string sendSMSUri;
        StreamReader reader;
        UTF8Encoding encoding;
        HttpWebRequest httpWReq;
        byte[] data;
        HttpWebResponse response;
        StringBuilder sbPostData;
        sbPostData = new StringBuilder();
        sbPostData.AppendFormat("authkey={0}", authkey);
        sbPostData.AppendFormat("&mobiles={0}", mobiles);
        sbPostData.AppendFormat("&message={0}", HttpUtility.UrlEncode(messageBody));
        sbPostData.AppendFormat("&sender={0}", sender);
        sbPostData.AppendFormat("&route={0}", "4");

        try
        {
            //Call Send SMS API
            sendSMSUri = "http://api.msg91.com/api/sendhttp.php";
            //Create HTTPWebrequest
            httpWReq = (HttpWebRequest)WebRequest.Create(sendSMSUri);
            //Prepare and Add URL Encoded data
            encoding = new UTF8Encoding();
            data = encoding.GetBytes(sbPostData.ToString());
            //Specify post method
            httpWReq.Method = "POST";
            httpWReq.ContentType = "application/x-www-form-urlencoded";
            httpWReq.ContentLength = data.Length;
            using (Stream stream = httpWReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            //Get the response
            response = (HttpWebResponse)httpWReq.GetResponse();
            reader = new StreamReader(response.GetResponseStream());
            retValue = reader.ReadToEnd();
            // SMSTransactionRequest.ResponseString = ConvertHelper.ConvertToString(responseString);
            //Close the response
            reader.Close();
            response.Close();
        }
        catch (Exception ex)
        {
            retValue = "SMS Not Sent";
            // return "SMS Not Sent";
        }
    }
}

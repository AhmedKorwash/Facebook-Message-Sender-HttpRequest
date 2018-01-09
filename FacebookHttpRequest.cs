using System;
using System.IO;
using System.Net;
using System.Text;

namespace IMessangerBot
{
    public class FacebookHttpRequest
    {
        private string output_message;
        public string OutputMessage
        {
            get { return output_message; }
            set { output_message = value; }
        }
        public string SendMessage(AuthFaceBook client, string TextMessage, OptionMessage option)
        {
            if (client.IsLogin)
            {
                string Data = string.Format("fb_dtsg={0}&body={1}&send=Send&wwwupp=V3&ids%5B{2}%5D={3}&referrer=&ctype=&cver=legacy", option.DTSG, Uri.EscapeDataString(TextMessage), option.UserId, option.UserId);
                HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create("https://m.facebook.com/messages/send/?icm=1");
                getRequest.CookieContainer = new CookieContainer();
                getRequest.CookieContainer = client.Cookies; //recover cookies First request
                getRequest.Method = WebRequestMethods.Http.Post;
                getRequest.UserAgent = client.UserAgent;
                getRequest.AllowWriteStreamBuffering = true;
                getRequest.ProtocolVersion = HttpVersion.Version11;
                getRequest.AllowAutoRedirect = false;
                getRequest.ContentType = "application/x-www-form-urlencoded";
                getRequest.Referer = "https://m.facebook.com";
                getRequest.KeepAlive = false;
                getRequest.Timeout = 45000;
                byte[] byteArray1 = Encoding.ASCII.GetBytes(Data);
                getRequest.ContentLength = byteArray1.Length;
                Stream newStream2 = getRequest.GetRequestStream(); //open connection
                newStream2.Write(byteArray1, 0, byteArray1.Length); // Send the data.
                newStream2.Close();
                output_message = "Message Sent";
                return output_message;
            }
            else
            {
                output_message = "Facebook Auth Error";
                return output_message;
            }
        }
    }
}
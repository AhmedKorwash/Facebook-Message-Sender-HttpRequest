using System;
using System.IO;
using System.Net;

namespace IMessangerBot
{
    public class OptionMessage
    {
        private string tids;
        public string error = "";
        public string TIDS
        {
            get { return tids; }
            set { tids = value; }
        }
        private string dtsg;
        public string DTSG
        {
            get { return dtsg; }
            set { dtsg = value; }
        }
        private string userId;
        public string UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        private string linkReq = "";
        public OptionMessage(string uid, AuthFaceBook client)
        {
            try
            {
                userId = uid;
                linkReq = "https://m.facebook.com/messages/thread/" + userId;
                HttpWebRequest Arequest = (HttpWebRequest)WebRequest.Create(linkReq);
                Arequest.CookieContainer = new CookieContainer();
                Arequest.CookieContainer = client.Cookies;
                Arequest.UserAgent = client.UserAgent;
                Arequest.KeepAlive = false;
                Arequest.Timeout = 45000;
                HttpWebResponse Aresponse = (HttpWebResponse)Arequest.GetResponse();
                StreamReader Asr = new StreamReader(Aresponse.GetResponseStream());
                error += "Step 1 ";
                string Ahtml = Asr.ReadToEnd();
                var splitted_fb_dtsg = Ahtml.Split(new string[] { "type=\"hidden\" name=\"fb_dtsg" }, StringSplitOptions.RemoveEmptyEntries);
                string fb_dtsg_container = splitted_fb_dtsg[1];
                var splitted_tids = Ahtml.Split(new string[] { "tids" }, StringSplitOptions.RemoveEmptyEntries);
                string tids_container = "";
                if (splitted_tids.Length > 2)
                {
                    tids_container = splitted_tids[1];
                }
                else
                {
                    tids_container = splitted_tids[0];
                }
                error += "Step 2 ";

                var splitted1_fb_dtsg = fb_dtsg_container.Split('"');
                string fb_dtsg_rubsh = splitted1_fb_dtsg[2];
                for (int u = 0; u < fb_dtsg_rubsh.Length; u++)
                {
                    if (fb_dtsg_rubsh[u] != '"')
                        dtsg += fb_dtsg_rubsh[u];
                }
                error += "Step 3 ";
                var splitted1_tids = tids_container.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries);

                string tids_rubsh = splitted1_tids[1];
                for (int u = 0; u < tids_rubsh.Length; u++)
                {
                    if (tids_rubsh[u] != '"')
                        tids += tids_rubsh[u];
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

        }
    }
}
using Android.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace IMessangerBot
{
    public class AuthFaceBook
    {
        private bool islogin;
        public bool IsLogin
        {
            get { return islogin; }
            set { islogin = value; }
        }
        private string username;
        public string UserName
        {
            get { return username; }
            set { username = value; }
        }
        private string pass;
        public string Password
        {
            get { return pass; }
            set { pass = value; }
        }
        private CookieContainer cookies;
        public CookieContainer Cookies
        {
            get { return cookies; }
            set { cookies = value; }
        }
        private string useragent = "Mozilla/5.0 (BlackBerry; U; BlackBerry 9900; en) AppleWebKit/534.11+ (KHTML, like Gecko) Version/7.1.0.346 Mobile Safari/534.11+";//"Mozilla/5.0 (Windows NT x.y; Win64; x64; rv:10.0) Gecko/20100101 Firefox/10.0";
        public AuthFaceBook(string username, string password)
        {
            string xx = Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
            if (!File.Exists(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), "usersFile.dat")))
            {
                File.Create(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), "usersFile.dat"));
            }
            List<string> usersFile = new List<string>();
            using (StreamReader reader = new StreamReader(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), "usersFile.dat")))
            {
                string line;
                while(true)
                {
                    line = reader.ReadLine();
                    if (line == null)
                        break;
                    usersFile.Add(line);
                }
            }

            if (usersFile.Any(s => s.Contains(username)))
            {
                string cookefile = usersFile.ToList().Where(s => s.Contains(username)).First().Split('|')[1];
                if (File.Exists(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), cookefile)))
                {
                    var Cookie = ReadCookiesFromDisk(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), cookefile));
                    if (Cookie != null)
                    {
                        this.username = username;
                        this.pass = password;
                        this.cookies = Cookie;
                        this.islogin = true;
                    }
                    else
                    {
                        IsLogin = false;
                        File.Delete(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), cookefile));
                    }
                }
                else
                {
                    AuthFaceBookNow(username, password);
                    if (IsLogin)
                    {
                        FileStream fs = new FileStream(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)),
                                        "usersFile.dat"), FileMode.OpenOrCreate, FileAccess.Write);
                        fs.Seek(0, SeekOrigin.End);
                        StreamWriter sw = new StreamWriter(fs);
                        bool res = WriteCookiesToDisk(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), cookefile), this.Cookies);
                        if (res)
                        {
                            sw.WriteLine(username + "|" + Guid.NewGuid().ToString() + ".dat");
                            sw.Flush();
                            sw.Close();
                            fs.Close();
                        }
                        else
                        {
                            IsLogin = false;
                            File.Delete(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), cookefile));
                        }
                    }
                }

            }
            else
            {
                AuthFaceBookNow(username, password);
                if (IsLogin)
                {
                    FileStream fs = new FileStream(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)),
                                    "usersFile.dat"), FileMode.OpenOrCreate, FileAccess.Write);
                    fs.Seek(0, SeekOrigin.End);
                    StreamWriter sw = new StreamWriter(fs);
                    string cookefile = Guid.NewGuid().ToString() + ".dat";
                    bool res = WriteCookiesToDisk(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), cookefile), this.Cookies);
                    if (res)
                    {
                        sw.WriteLine(username + "|" + Guid.NewGuid().ToString() + ".dat");
                        sw.Flush();
                        sw.Close();
                        fs.Close();
                    }
                    else
                    {
                        IsLogin = false;
                        File.Delete(Path.Combine(Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)), cookefile));
                    }
                }
            }
        }

        public AuthFaceBook()
        {
            // TODO: Complete member initialization
        }
        public void AuthFaceBookNow(string username, string password)
        {
            UserName = username;
            Password = password;
            cookies = new CookieContainer();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.facebook.com/");
                request.CookieContainer = new CookieContainer();
                request.CookieContainer = cookies;
                request.UserAgent = useragent;
                request.KeepAlive = false;
                request.Timeout = 45000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                cookies.Add(response.Cookies);
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string html = sr.ReadToEnd();
                html = html.Substring(html.IndexOf("login_form"));
                html = html.Remove(html.IndexOf("/form"));
                Regex reg = new Regex(@"name=""[^""]+"" value=""[^""]+""");
                MatchCollection mc = reg.Matches(html);
                List<string> values = new List<string>();
                for (int k = 0; k < mc.Count; k++)
                {
                    if (k != 0)
                        postData += "&";
                    if (k == 1)
                    {
                        postData += "email=" + this.username + "&pass=" + this.pass + "&";
                    }
                    string m = mc[k].Value.Replace("\"", "");
                    m = m.Replace("name=", "");
                    m = m.Replace(" value=", "=");
                    postData += m;
                }


                string getUrl = "https://www.facebook.com/login.php?login_attempt=1";
                HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(getUrl);
                getRequest.CookieContainer = new CookieContainer();
                getRequest.CookieContainer = cookies; //recover cookies First request
                getRequest.Method = WebRequestMethods.Http.Post;
                getRequest.UserAgent = useragent;
                getRequest.AllowWriteStreamBuffering = true;
                getRequest.ProtocolVersion = HttpVersion.Version11;
                getRequest.AllowAutoRedirect = false;
                getRequest.ContentType = "application/x-www-form-urlencoded";
                getRequest.Referer = "https://www.facebook.com";
                getRequest.KeepAlive = false;
                getRequest.Timeout = 45000;

                byte[] byteArray = Encoding.ASCII.GetBytes(postData);
                getRequest.ContentLength = byteArray.Length;
                Stream newStream = getRequest.GetRequestStream(); //open connection
                newStream.Write(byteArray, 0, byteArray.Length); // Send the data.
                newStream.Close();

                HttpWebResponse getResponse = (HttpWebResponse)getRequest.GetResponse();
                cookies.Add(getResponse.Cookies);
                if (getResponse.Cookies.Count > 6)
                    islogin = true;
                else
                    islogin = false;

            }
            catch { }

        }
        public string postData { get; set; }
        public void ReloadAuth()
        {
            AuthFaceBookNow(username, pass);
        }
        public string UserAgent
        {
            get { return useragent; }
            set { useragent = value; }
        }

        public CookieContainer ReadCookiesFromDisk(string file)
        {

            try
            {
                using (Stream stream = File.Open(file, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (CookieContainer)formatter.Deserialize(stream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public bool WriteCookiesToDisk(string file, CookieContainer cookieJar)
        {
            using (Stream stream = File.Create(file))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, cookieJar);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
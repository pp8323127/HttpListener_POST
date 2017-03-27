using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Net;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace HttpListener_POST
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        static HttpListener listener;
        private Thread listenThread1;



        /** method stolen from an SO thread. sorry can't remember the author **/
        static void AddAddress(string address, string domain, string user)
        {
            string args = string.Format(@"http add urlacl url={0}", address) + " user=\"" + domain + "\\" + user + "\"";

            ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;

            Process.Start(psi).WaitForExit();
        }


        public MainWindow()
        {
            InitializeComponent();





            //AddAddress("http://163.18.42.211:1508/", Environment.UserDomainName, Environment.UserName);

            listener = new HttpListener();
            try
            {
                ///listener.Prefixes.Add("http://localhost:8000/");
                //listener.Prefixes.Add("http://127.0.0.1:8000/");
                //listener.Prefixes.Add("http://163.18.42.211:8000/");
                listener.Prefixes.Add("http://163.18.42.211:1508/");
                listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

                listener.Start();
                this.listenThread1 = new Thread(new ParameterizedThreadStart(startlistener));
                listenThread1.Start();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            //finally
            //{
            //    listener.Stop();
            //}
        }

        private void MainWindow_Closing()
        {
            listener.Stop();
        }



        private void startlistener(object s)
        {

            while (true)
            {

                ////blocks until a client has connected to the server
                ProcessRequest();

            }

        }


        private void ProcessRequest()
        {

            var result = listener.BeginGetContext(ListenerCallback, listener);
            result.AsyncWaitHandle.WaitOne();

        }

        private void ListenerCallback(IAsyncResult result)
        {
            var context = listener.EndGetContext(result);
            Thread.Sleep(1000);
            var data_text = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();

            //functions used to decode json encoded data.
            //JavaScriptSerializer js = new JavaScriptSerializer();
            //var data1 = Uri.UnescapeDataString(data_text);
            //string da = Regex.Unescape(data_text);
            // var unserialized = js.Deserialize(data_text, typeof(String));

            var cleaned_data = System.Web.HttpUtility.UrlDecode(data_text);
            //MessageBox.Show(cleaned_data);

            if (cleaned_data != "")
            {
                string str = cleaned_data;
                str.Substring(cleaned_data.Length - 10, 10);
                //MessageBox.Show(str);
                //MessageBox.Show(cleaned_data);

                DateTime mNow = DateTime.Now;
                //MessageBox.Show(mNow.ToString("yyyy-MM-dd HH:mm:ss"));

                string strInvoice = mNow.ToString("yyyy-MM-dd HH:mm:ss") + ", " + cleaned_data.Substring(cleaned_data.Length - 10, 10) + ", " + Environment.NewLine;
                //MessageBox.Show(strInvoice);

                ////bool b = File.Exists("tmp.txt");        // 判定檔案是否存在
                ////File.Create("tmp.txt");             // 建立檔案
                //string text = File.ReadAllText("tmp.txt");  // 讀取檔案內所有文字

                //File.WriteAllText("tmp.txt", text + "/n" + cleaned_data);     // 將 text 寫入檔案



                string path = @"tmp.txt";
                File.AppendAllText(path, strInvoice);



                //// Open the file to read from.
                //using (StreamReader sr = File.OpenText("tmp.txt"))
                //{
                //    string s = "";
                //    while ((s = sr.ReadLine()) != null)
                //    {
                //        Console.WriteLine(s);
                //    }
                //}

            }



            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            //use this line to get your custom header data in the request.
            //var headerText = context.Request.Headers["mycustomHeader"];

            //use this line to send your response in a custom header
            //context.Response.Headers["mycustomResponseHeader"] = "mycustomResponse";

            context.Response.Close();
        }


    }
}

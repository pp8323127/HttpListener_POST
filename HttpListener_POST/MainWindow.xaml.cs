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

using System.ComponentModel;

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
            showTextBox();



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
            var post_raw_data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();

            //functions used to decode json encoded data.
            //JavaScriptSerializer js = new JavaScriptSerializer();
            //var data1 = Uri.UnescapeDataString(post_raw_data);
            //string da = Regex.Unescape(post_raw_data);
            // var unserialized = js.Deserialize(post_raw_data, typeof(String));

            var post_data = System.Web.HttpUtility.UrlDecode(post_raw_data);
            //MessageBox.Show(post_data);

            if (post_data != "")
            {
                string str = post_data;
                str.Substring(post_data.Length - 10, 10);
                //MessageBox.Show(str);
                //MessageBox.Show(post_data);

                DateTime mNow = DateTime.Now;
                //MessageBox.Show(mNow.ToString("yyyy-MM-dd HH:mm:ss"));

                //string strInvoice = mNow.ToString("yyyy-MM-dd HH:mm:ss") + ", " + post_data.Substring(post_data.Length - 10, 10) + ", " + Environment.NewLine;
                string strInvoice = mNow.ToString("yyyy-MM-dd HH:mm:ss") + ", " + post_data + ", " + Environment.NewLine;
                

                //MessageBox.Show(strInvoice);

                ////bool b = File.Exists("tmp.txt");        // 判定檔案是否存在
                ////File.Create("tmp.txt");             // 建立檔案
                //string text = File.ReadAllText("tmp.txt");  // 讀取檔案內所有文字

                //File.WriteAllText("tmp.txt", text + "/n" + post_data);     // 將 text 寫入檔案



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

        private void showTextBox()
        {
            textBox.Text = File.ReadAllText("tmp.txt");
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            //listener.Close();
            listener.Stop();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            showTextBox();
        }
    }
}

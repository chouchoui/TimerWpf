using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace TimerVisit
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dispatcherTimer;
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            VisitSubjectService(txtUrl.Text);
            label.Text = "上次执行时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private async void VisitSubjectService(string url)
        {
            try
            {
                var list = url.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in list)
                {
                    var web = await GetStringAsync(item);
                }
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现错误\r\n" + ex.Message);
                Environment.Exit(0);
            }
        }


        internal static Task<string> GetStringAsync(string url)
        {

            Task<WebResponse> task = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            task = Task.Factory.FromAsync(
                request.BeginGetResponse,
                asyncResult => request.EndGetResponse(asyncResult),
                null);

            return task.ContinueWith(t =>
                  ReadStreamFromResponse(t.Result));


        }

        internal static string ReadStreamFromResponse(WebResponse response)
        {
            using (var responseStream = response.GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                var content = reader.ReadToEnd();
                return content;
            }
        }

        private void btnVisit_Click(object sender, RoutedEventArgs e)
        {
            var time = Convert.ToInt32(string.IsNullOrWhiteSpace(txtTime.Text) ? "20" : txtTime.Text);
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, time, 0);
            dispatcherTimer.Start();
            btnStop.Visibility = Visibility.Visible;
            btnVisit.Visibility = Visibility.Collapsed;
            InitialTray();
            VisitSubjectService(txtUrl.Text);
            label.Text = "上次执行时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            btnStop.Visibility = Visibility.Collapsed;
            btnVisit.Visibility = Visibility.Visible;
            label.Text = "";
            txtTime.Text = "20";
            notifyIcon.Dispose();
            StateChanged -= MainWindow_StateChanged;
            txtUrl.Text = "http://";
        }

        private void InitialTray()
        {
            //隐藏主窗体
            Visibility = Visibility.Hidden;
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            //设置托盘的各个属性
            notifyIcon.BalloonTipText = "已最小化到系统托盘";
            notifyIcon.Text = "systray";
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(1000);
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;

            //窗体状态改变时候触发
            StateChanged += MainWindow_StateChanged;
        }

        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (Visibility == Visibility.Visible)
                {
                    Visibility = Visibility.Hidden;
                }
                else
                {
                    Visibility = Visibility.Visible;
                    Activate();
                }
            }
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && notifyIcon != null)
            {
                Visibility = Visibility.Hidden;
            }
        }

        private void txtTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key < Key.D0 || e.Key > Key.D9)
            {
                if (e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
                {
                    if (e.Key != Key.Back)
                    {
                        e.Handled = true;
                    }
                }
            }
            if (e.Key == Key.Decimal || e.Key == Key.Add || e.Key == Key.Subtract || e.Key == Key.Multiply || e.Key == Key.Divide)
            {
                e.Handled = false;
            }
        }
    }
}

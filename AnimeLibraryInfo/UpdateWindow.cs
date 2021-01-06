using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AnimeLibraryInfo
{
    public partial class UpdateWindow : Form
    {
        Thread UpdateThread;
        bool DownloadCompleted = false; 
        public UpdateWindow()
        {
            InitializeComponent();
        }

        private void UpdateWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !ControlBox;
        }

        private void UpdateWindow_Load(object sender, EventArgs e)
        {
            Log("Checking for updates...");
        }

        void Log(string message)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new LogMessage(Log), message);
            }
            else
            {
                listBox1.Items.Add(message);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
        }

        delegate void LogMessage(string message);

        private void progressBar1_Click(object sender, EventArgs e)
        {
            //add debug keybind
            if (ModifierKeys.HasFlag(Keys.Shift) && ModifierKeys.HasFlag(Keys.Control))
            {
                ControlBox = true;
            }
        }

        private void UpdateWindow_Shown(object sender, EventArgs e)
        {
            UpdateThread = new Thread(new ThreadStart(UpdateApp));
            UpdateThread.Name = "UpdateThread";
            UpdateThread.Priority = ThreadPriority.AboveNormal;
            Log("Starting update process...");
            UpdateThread.Start();
        }

        void UpdateApp()
        {
            Log("Checking internet connectivity...");
            Ping p = new Ping();
            var result = p.SendPingAsync("github.com").Result;
            if (result.Status != IPStatus.Success)
            {
                Log("No internet connectivity.");
                return;
            }
            Log("Internet connectivity available.");
            Log("Checking for new versions of the software...");
            var client = new GitHubClient(new ProductHeaderValue("animelib-updater"));
            var releases = client.Repository.Release.GetAll("adryzz", "animelib").Result;
            var latest = releases[0];
            Log("Found version tagged " + latest.TagName);
            Log("Starting download...");
            using (var dlclient = new WebClient())
            {
                dlclient.DownloadProgressChanged += Dlclient_DownloadProgressChanged;
                dlclient.DownloadFileCompleted += Dlclient_DownloadFileCompleted;
                dlclient.DownloadFileAsync(new Uri(latest.Assets.FirstOrDefault().BrowserDownloadUrl), "Update.zip");
                while(!DownloadCompleted)
                {
                    Thread.Sleep(100);
                }
            }
            var releases1 = client.Repository.Release.GetAll("adryzz", "animelib-updater").Result;
            var latest1 = releases[0];
            using (var dlclient = new WebClient())
            {
                dlclient.DownloadProgressChanged += Dlclient_DownloadProgressChanged;
                dlclient.DownloadFileCompleted += Dlclient_DownloadFileCompleted;
                dlclient.DownloadFileAsync(new Uri(latest.Assets.FirstOrDefault().BrowserDownloadUrl), "animelib-updater.exe");
                while (!DownloadCompleted)
                {
                    Thread.Sleep(100);
                }
            }
            MessageBox.Show("Now close the program and run animelib-updater.exe");
            Invoke(new UpdateUI(() => ControlBox = true));
        }

        private void Dlclient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateProgress(e.ProgressPercentage);
        }

        private void Dlclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Log("Download completed.");
        }

        void UpdateProgress(int value)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new UpdateBar(UpdateProgress), value);
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Value = value;
            }
        }

        delegate void UpdateBar(int value);

        delegate void UpdateUI();
    }
}

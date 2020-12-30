using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using AnimeLibWin;
using AnimeLibWin.Collections;

namespace AnimeLibraryInfo
{
    public partial class Form1 : Form
    {
        AnimeLibrary Library;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (File.Exists("library.json"))
            {
                string json = File.ReadAllText("library.json");
                Library = AnimeLibrary.ImportFromJson(json);
                ReloadUI();
            }
            else
            {
                MessageBox.Show("Welcome to AnimeLibraryInfo!\nThis program helps you sort your local anime library!\nTo start, go to File > Index library");
            }
        }

        private void indexLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimeIndexer indexer = new AnimeIndexer(Library);
            indexer.ShowDialog();
            if (indexer.IsOK) 
            {
                Library = indexer.Library;
                File.WriteAllText("library.json", Library.ExportToJson(true));
                ReloadUI();
            }
        }

        private void ReloadUI()
        {
            int series = 0;
            int videos = 0;
            foreach(AnimeSeries s in Library.Library)
            {
                series++;

                foreach(AnimeSeason ss in s.Seasons)
                {
                    videos += ss.Episodes.Count;
                }
            }
            label1.Text = $"Anime in the library: {series}\r\nTotal number of videos in the library: {videos}\r\nLibrary size: {Utils.BytesToString(Library.LibrarySize)}";
        }
    }
}

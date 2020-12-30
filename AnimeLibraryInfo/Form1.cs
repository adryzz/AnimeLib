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
        List<AnimeSeries> Searched = new List<AnimeSeries>();
        AnimeSeries SelectedSeries;
        AnimeSeason SelectedSeason;
        AnimeEpisode SelectedEpisode;
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
            string selected = "";
            if (listBox1.SelectedIndex >= 0)
            {
                selected = listBox1.SelectedItem.ToString();
            }
            listBox1.Items.Clear();
            Searched.Clear();
            int index = 0;
            foreach (AnimeSeries s in Library.Library)
            {
                series++;

                foreach(AnimeSeason ss in s.Seasons)
                {
                    videos += ss.Episodes.Count;
                }

                if (s.Name.ToLower().Contains(textBox1.Text.ToLower()))
                {
                    Searched.Add(s);
                    listBox1.Items.Add(s.Name);
                    if (s.Name.Equals(selected))
                    {
                        listBox1.SelectedIndex = index;
                    }
                }
                index++;
            }
            label1.Text = $"Anime in the library: {series}\r\nTotal number of videos in the library: {videos}\r\nLibrary size: {Utils.BytesToString(Library.LibrarySize)}";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string selected = "";
            if (listBox1.SelectedIndex >= 0)
            {
                selected = listBox1.SelectedItem.ToString();
            }
            int index = 0;
            Searched.Clear();
            listBox1.Items.Clear();
            foreach(AnimeSeries s in Library.Library)
            {
                if (s.Name.ToLower().Contains(textBox1.Text.ToLower()))
                {
                    Searched.Add(s);
                    listBox1.Items.Add(s.Name);
                    if (s.Name.Equals(selected))
                    {
                        listBox1.SelectedIndex = index-1;
                    }
                }
                index++;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            if (listBox1.SelectedIndex >= 0)
            {
                SelectedSeries = Searched[listBox1.SelectedIndex];
                long size = 0;
                foreach(AnimeSeason s in SelectedSeries.Seasons)
                {
                    size += s.Size;
                    comboBox1.Items.Add(s.SeasonPath.Name);
                }
                label3.Text = $"Name:{SelectedSeries.Name}\r\nSeasons:{SelectedSeries.Seasons.Count}\r\nSize: {Utils.BytesToString(size)}";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            if (comboBox1.SelectedIndex >= 0)
            {
                SelectedSeason = SelectedSeries.Seasons[comboBox1.SelectedIndex];
                foreach(AnimeEpisode ep in SelectedSeason.Episodes)
                {
                    listBox2.Items.Add(ep.EpisodePath.Name);
                }
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex >= 0)
            {
                SelectedEpisode = SelectedSeason.Episodes[listBox2.SelectedIndex];
                label6.Text = $"Name:{SelectedEpisode.EpisodePath.Name}\r\nResolution: {SelectedEpisode.EpisodeInfo.VideoWidth}x{SelectedEpisode.EpisodeInfo.VideoHeight}\r\nFrame rate: {SelectedEpisode.EpisodeInfo.VideoFrameRate} fps\r\nBitrate: {SelectedEpisode.EpisodeInfo.VideoBitrate} kbps\r\nSize: {Utils.BytesToString(SelectedEpisode.EpisodePath.Length)}\r\nDuration: {SelectedEpisode.EpisodeInfo.Duration}\r\nDescription: {SelectedEpisode.EpisodeInfo.Description}";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (SelectedEpisode.EpisodePath != null)
            {
                System.Diagnostics.Process.Start(SelectedEpisode.EpisodePath.FullName);
            }
        }
    }
}

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
                MessageBox.Show("Welcome to AnimeLibraryInfo!\nThis program helps you sort your local anime library!\nTo start, go to File > Index library", "AnimeLibraryInfo");
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

                foreach (AnimeSeason ss in s.Seasons)
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
            foreach (AnimeSeries s in Library.Library)
            {
                if (s.Name.ToLower().Contains(textBox1.Text.ToLower()))
                {
                    Searched.Add(s);
                    listBox1.Items.Add(s.Name);
                    if (s.Name.Equals(selected))
                    {
                        //listBox1.SelectedItem = selected;
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
                foreach (AnimeSeason s in SelectedSeries.Seasons)
                {
                    size += s.Size;
                    comboBox1.Items.Add(s.SeasonPath.Name);
                }
                label3.Text = $"Name:{SelectedSeries.Name}\r\nSeasons:{SelectedSeries.Seasons.Count}\r\nSize: {Utils.BytesToString(size)}";
                comboBox1.SelectedIndex = 0;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            if (comboBox1.SelectedIndex >= 0)
            {
                SelectedSeason = SelectedSeries.Seasons[comboBox1.SelectedIndex];
                foreach (AnimeEpisode ep in SelectedSeason.Episodes)
                {
                    listBox2.Items.Add(ep.EpisodePath.Name);
                }
            }
            if (SelectedSeason.Episodes.Count != 0)
            {
                listBox2.SelectedIndex = 0;
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex >= 0)
            {
                SelectedEpisode = SelectedSeason.Episodes[listBox2.SelectedIndex];
                label6.Text = $"Name:{SelectedEpisode.EpisodePath.Name}\r\nResolution: {SelectedEpisode.EpisodeInfo.VideoWidth}x{SelectedEpisode.EpisodeInfo.VideoHeight}\r\nFrame rate: {SelectedEpisode.EpisodeInfo.VideoFrameRate} fps\r\nBitrate: {SelectedEpisode.EpisodeInfo.VideoBitrate} kbps\r\nSize: {Utils.BytesToString(SelectedEpisode.EpisodePath.Length)}\r\nDuration: {SelectedEpisode.EpisodeInfo.Duration}\r\nDescription: {SelectedEpisode.EpisodeInfo.Description}";
                checkBox1.Checked = SelectedEpisode.Watched;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (SelectedEpisode.EpisodePath != null)
            {
                SelectedEpisode.Watched = true;
                checkBox1.Checked = true;
                System.Diagnostics.Process.Start(SelectedEpisode.EpisodePath.FullName);

            }
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateWindow u = new UpdateWindow();
            u.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented");
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SelectedEpisode.Watched = checkBox1.Checked;
            for (int i = 0; i < Library.Library.Count; i++)
            {
                for (int j = 0; j < Library.Library[i].Seasons.Count; j++)
                {
                    for (int w = 0; w < Library.Library[i].Seasons[j].Episodes.Count; w++)
                    {
                        if (Library.Library[i].Seasons[j].Episodes[w].EpisodePath.FullName.Equals(SelectedEpisode.EpisodePath.FullName))
                        {
                            Library.Library[i].Seasons[j].Episodes[w] = SelectedEpisode;
                        }
                    }
                }
            }
            File.WriteAllText("library.json", Library.ExportToJson(true));
        }

        private void markSelectedSeriesAsWatchedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Library.Library.Count; i++)
            {
                if (Library.Library[i].Name.Equals(SelectedSeries.Name))
                {
                    for (int j = 0; j < Library.Library[i].Seasons.Count; j++)
                    {
                        for (int w = 0; w < Library.Library[i].Seasons[j].Episodes.Count; w++)
                        {
                            var v = Library.Library[i].Seasons[j].Episodes[w];
                            v.Watched = true;
                            Library.Library[i].Seasons[j].Episodes[w] = v;
                        }
                    }
                }
            }
            File.WriteAllText("library.json", Library.ExportToJson(true));
        }

        private void markSelectedSeasonAsWatchedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Library.Library.Count; i++)
            {
                for (int j = 0; j < Library.Library[i].Seasons.Count; j++)
                {
                    if (Library.Library[i].Seasons[j].SeasonPath.FullName.Equals(SelectedSeason.SeasonPath.FullName))
                    {
                        for (int w = 0; w < Library.Library[i].Seasons[j].Episodes.Count; w++)
                        {
                            var v = Library.Library[i].Seasons[j].Episodes[w];
                            v.Watched = true;
                            Library.Library[i].Seasons[j].Episodes[w] = v;
                        }
                    }
                }
            }
            File.WriteAllText("library.json", Library.ExportToJson(true));
        }

        private void listBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            Graphics g = e.Graphics;
            if (e.State.HasFlag(DrawItemState.Selected))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(0, 120, 215)), e.Bounds);
            }
            else
            {
                if (SelectedSeason.Episodes[e.Index].Watched)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(86, 199, 0)), e.Bounds);
                }
                else
                {
                    g.FillRectangle(new SolidBrush(Color.White), e.Bounds);
                }
            }
            ListBox lb = (ListBox)sender;
            g.DrawString(lb.Items[e.Index].ToString(), e.Font, new SolidBrush(Color.Black), new PointF(e.Bounds.X, e.Bounds.Y));

            e.DrawFocusRectangle();
        }
    }
}

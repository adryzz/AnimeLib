using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AnimeLibWin;
using AnimeLibWin.Collections;
using AnimeLibWin.Types;
using TagLib;

namespace AnimeLibraryInfo
{
    public partial class AnimeIndexer : Form
    {
        public bool IsOK = false;
        public AnimeLibrary Library;
        public AnimeLibrary OldLibrary;
        public AnimeIndexer(AnimeLibrary existing)
        {
            InitializeComponent();
            if (existing != null)
            {
                textBox1.Text = existing.LibraryPath.FullName;
                OldLibrary = existing;
            }
        }

        private void AnimeIndexer_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog d = new FolderBrowserDialog())
            {
                var res = d.ShowDialog();
                if (res == DialogResult.OK)
                {
                    textBox1.Text = d.SelectedPath;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(Index)).Start();
        }

        void Index()
        {
            string path = textBox1.Text;
            if (!Directory.Exists(path))
            {
                MessageBox.Show("The specified directory does not exist");
                return;
            }
            try
            {
                Library = new AnimeLibrary(new DirectoryInfo(path), false);
                List<AnimeSeason> Seasons = new List<AnimeSeason>();
                foreach (DirectoryInfo d in Library.LibraryPath.ToDirectoryInfo().EnumerateDirectories())
                {
                    long size = 0;
                    List<AnimeEpisode> episodes = new List<AnimeEpisode>();
                    foreach (FileInfo i in d.EnumerateFiles())
                    {
                        listBox1.Invoke(new Log(() => { listBox1.Items.Add("Scanned " + i.FullName); listBox1.TopIndex = listBox1.Items.Count - 1; }));
                        if (i.Extension.Equals(".mp4") || i.Extension.Equals(".mkv")) //check if it's a video file
                        {
                            TagLib.File file = TagLib.File.Create(i.FullName);
                            TagLib.Mpeg.VideoHeader header = new TagLib.Mpeg.VideoHeader();
                            foreach (ICodec codec in file.Properties.Codecs)
                            {
                                if (codec is TagLib.Mpeg.VideoHeader)
                                {
                                    header = (TagLib.Mpeg.VideoHeader)codec;
                                }
                            }
                            SerializableFileInfo info = new SerializableFileInfo(i.FullName);
                            episodes.Add(new AnimeEpisode
                            {
                                EpisodePath = info,
                                EpisodeInfo = header
                            });
                            size += info.Length;
                        }
                    }
                    //order episodes by number
                    //this is tricky but not that hard to do
                    string[] remove = { "144p", "240p", "360p", "480p", "720p", "1080p", "1440p", "2160p", "mp4" };
                    SortedDictionary<int, AnimeEpisode> episodes2 = new SortedDictionary<int, AnimeEpisode>();
                    foreach (AnimeEpisode e in episodes)
                    {
                        string name = e.EpisodePath.Name;
                        foreach (string s in remove)
                        {
                            name = name.Replace(s, "");
                        }
                        episodes2.Add(int.Parse(String.Join("", name.Where(char.IsDigit))), e);
                    }
                    episodes.Clear();
                    episodes.AddRange(episodes2.Values);

                    Seasons.Add(new AnimeSeason()
                    {
                        Episodes = episodes,
                        SeasonPath = new SerializableDirectoryInfo(d.FullName),
                        Size = size
                    });
                    Library.LibrarySize += size;
                }

                //sort per series

                //get all names
                string[] names = new string[Seasons.Count];
                for (int i = 0; i < Seasons.Count; i++)
                {
                    names[i] = Seasons[i].SeasonPath.Name;
                }

                //remove "OVA" from all names because it can hurt consistency
                for (int i = 0; i < names.Length; i++)
                {
                    names[i].Replace(" OVA", ""); //remove with the space at the beginning
                }

                bool done = false;
                int currentIndex = 0;
                List<string> nameList = new List<string>(names);
                List<AnimeSeason> seasons = Seasons;
                while (!done)
                {
                    Tuple<AnimeSeries, int> current = GenerateTree(names[currentIndex], nameList.ToArray(), seasons);
                    nameList.RemoveAt(0);
                    seasons.RemoveAt(0);
                    currentIndex += current.Item2;
                    Library.Library.Add(current.Item1);
                    if (currentIndex >= names.Length)
                    {
                        done = true;
                    }
                }
                //remove empty animes
                List<AnimeSeries> nodesToRemove = new List<AnimeSeries>();
                foreach (AnimeSeries t in Library.Library)
                {
                    if (t.Seasons.Count == 0)
                    {
                        nodesToRemove.Add(t);
                    }
                }
                foreach (AnimeSeries t in nodesToRemove)
                {
                    Library.Library.Remove(t);
                }

                //mark episodes as completed
                if (OldLibrary != null)
                {
                    foreach(AnimeSeries series in OldLibrary.Library)
                    {
                        foreach(AnimeSeason season in series.Seasons)
                        {
                            foreach(AnimeEpisode e in season.Episodes)
                            {
                                for (int i = 0; i < Library.Library.Count; i++)
                                {
                                    for (int j = 0; j < Library.Library[i].Seasons.Count; j++)
                                    {
                                        for (int w = 0; w < Library.Library[i].Seasons[j].Episodes.Count; w++)
                                        {
                                            if (Library.Library[i].Seasons[j].Episodes[w].EpisodePath.FullName.Equals(e.EpisodePath.FullName))
                                            {
                                                AnimeEpisode ep = Library.Library[i].Seasons[j].Episodes[w];
                                                ep.Watched = e.Watched;
                                                ep.EpisodeNumber = w + 1;
                                                Library.Library[i].Seasons[j].Episodes[w] = ep;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                IsOK = true;
                Invoke(new Log(() => { MessageBox.Show("Done indexing!", "AnimeLibraryInfo", MessageBoxButtons.OK, MessageBoxIcon.Information); Close(); }));
            }
            catch(Exception e)
            {
                MessageBox.Show("The folder structure isn't set up correctly.\nCan't index library.", "AnimeLibraryInfo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static Tuple<AnimeSeries, int> GenerateTree(string current, string[] names, List<AnimeSeason> seasons)
        {
            //create first subnode
            AnimeSeries series = new AnimeSeries() { Name = current.Replace(" Season 1", "").Replace(" season 1", "").Replace(" 1", ""), Seasons = new List<AnimeSeason>() };

            int max = names.Length; //this is the index of the last item processed so the one not added to the tree

            //check if any of the names are at least 50% similiar to the first node
            for (int i = 0; i < names.Length; i++)
            {
                string s = names[i];
                int distance = CalcLevenshteinDistance(current, s);
                float perc = 100f - ((distance / (float)s.Length) * 100f);
                if (perc >= 50) //50%
                {
                    series.Seasons.Add(seasons[i]);
                }
                else
                {
                    max = i;
                    break;
                }
            }
            return new Tuple<AnimeSeries, int>(series, max);
        }

        private static int CalcLevenshteinDistance(string a, string b)
        {
            if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (String.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (String.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++);
            for (int j = 0; j <= lengthB; distances[0, j] = j++);

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }

        delegate void Log();
    }
}

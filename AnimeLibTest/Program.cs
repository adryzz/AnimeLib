using System;
using System.Collections.Generic;
using System.IO;
using AnimeLib;
using AnimeLib.Collections;
using AnimeLib.Types;
using TagLib;

namespace AnimeLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            AnimeLibrary Library = new AnimeLibrary(new DirectoryInfo(@"D:\Anime Downloaded"), false);
            List<AnimeSeason> Seasons = new List<AnimeSeason>();
            foreach(DirectoryInfo d in Library.LibraryPath.ToDirectoryInfo().EnumerateDirectories())
            {
                long size = 0;
                List<AnimeEpisode> episodes = new List<AnimeEpisode>();
                foreach(FileInfo i in d.EnumerateFiles())
                {
                    Console.WriteLine("Scanned " + i.FullName);
                    if (!i.Extension.Equals(".ass"))//check if it's a subtitle file
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
            for(int i = 0; i < Seasons.Count; i++)
            {
                names[i] = Seasons[i].SeasonPath.Name;
            }

            //remove "OVA" from all names because it can hurt consistency
            for (int i = 0; i < names.Length; i++)
            {
                names[i].Replace(" OVA", "");//remove with the space at the beginning
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
            System.IO.File.WriteAllText("library.json", Library.ExportToJson(true));
        }

        static Tuple<AnimeSeries, int> GenerateTree(string current, string[] names, List<AnimeSeason> seasons)
        {
            //create first subnode
            AnimeSeries series = new AnimeSeries() { Name = current, Seasons = new List<AnimeSeason>() };

            int max = names.Length;//this is the index of the last item processed so the one not added to the tree

            //check if any of the names are at least 50% similiar to the first node
            for (int i = 0; i < names.Length; i++)
            {
                string s = names[i];
                int distance = CalcLevenshteinDistance(current, s);
                float perc = 100f - ((distance / (float)s.Length) * 100f);
                if (perc >= 50)//50%
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
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

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
    }
}

using AnimeLibWin.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TagLib.Mpeg;

namespace AnimeLibWin.Collections
{
    public struct AnimeEpisode
    {
        public SerializableFileInfo EpisodePath;
        public VideoHeader EpisodeInfo;
    }
}

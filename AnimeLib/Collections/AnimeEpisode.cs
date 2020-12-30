using AnimeLib.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TagLib.Mpeg;

namespace AnimeLib.Collections
{
    public struct AnimeEpisode
    {
        public SerializableFileInfo EpisodePath;
        public VideoHeader EpisodeInfo;
    }
}

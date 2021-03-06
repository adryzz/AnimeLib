﻿using AnimeLibWin.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnimeLibWin.Collections
{
    public struct AnimeSeason
    {
        public SerializableDirectoryInfo SeasonPath;
        public List<AnimeEpisode> Episodes;
        public long Size;
    }
}

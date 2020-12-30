using AnimeLib.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using AnimeLib.Types;

namespace AnimeLib
{
    public class AnimeLibrary
    {
        public SerializableDirectoryInfo LibraryPath;

        public List<AnimeSeries> Library;

        public long LibrarySize = 0;


        public AnimeLibrary(DirectoryInfo path, bool tryAutoGenerate)
        {
            LibraryPath = new SerializableDirectoryInfo(path.FullName);
            Library = new List<AnimeSeries>();
            if (!tryAutoGenerate) return;

        }

        public string ExportToJson(bool indented)
        {
            return JsonConvert.SerializeObject(this, indented ? Formatting.Indented : Formatting.None);
        }

        public static AnimeLibrary ImportFromJson(string json)
        {
            return JsonConvert.DeserializeObject<AnimeLibrary>(json);
        }

        public string ExportToXml()
        {
            throw new NotImplementedException();
        }

        public static AnimeLibrary ImportFromXml()
        {
            throw new NotImplementedException();
        }
    }
}

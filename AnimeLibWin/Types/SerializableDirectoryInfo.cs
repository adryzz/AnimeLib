using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnimeLibWin.Types
{
    [Serializable]
    public class SerializableDirectoryInfo
    {
        public string Name { get; set; }

        public string FullName { get; set; }

        public DateTime CreationTime { get; set; }

        /// <summary>
        /// An empty ctor is needed for serialization.
        /// </summary>
        public SerializableDirectoryInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="test.MyFileInfo"/> class.
        /// </summary>
        /// <param name="fileInfo">File info.</param>
        public SerializableDirectoryInfo(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            this.Name = dirInfo.Name;
            this.FullName = dirInfo.FullName;
            this.CreationTime = dirInfo.CreationTime;
            // TODO: add and initilize other members
        }

        public DirectoryInfo ToDirectoryInfo()
        {
            return new DirectoryInfo(FullName);
        }
    }
}

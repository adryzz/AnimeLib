using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnimeLib.Types
{
    [Serializable]
    public class SerializableFileInfo
    {
        public string Name { get; set; }

        public string FullName { get; set; }

        public long Length { get; set; }

        public DateTime CreationTime { get; set; }

        /// <summary>
        /// An empty ctor is needed for serialization.
        /// </summary>
        public SerializableFileInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="test.MyFileInfo"/> class.
        /// </summary>
        /// <param name="fileInfo">File info.</param>
        public SerializableFileInfo(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            this.Length = fileInfo.Length;
            this.Name = fileInfo.Name;
            this.FullName = fileInfo.FullName;
            this.CreationTime = fileInfo.CreationTime;
            // TODO: add and initilize other members
        }

        public FileInfo ToFileInfo()
        {
            return new FileInfo(FullName);
        }
    }
}

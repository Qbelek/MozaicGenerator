using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GUIv2.Models
{
    [Serializable]
    class ImagesDatabase
    {
        [field:NonSerialized]
        public event EventHandler<ProgressChangedEventHandler> ProgressChanged;

        private readonly object _lockResources = new object();
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".BMP", ".PNG" };
        public string DBPath { get; private set; }
        public string OutputPath { get; private set; }
        public List<ImageItem> Images { get; private set; }
        public int NumOfImages { get; private set; }
        public int ImagesSize { get; private set; }


        
        public class ProgressChangedEventHandler : EventArgs
        {
            public int Progress { get; private set; }

            public ProgressChangedEventHandler(int progress)
            {
                Progress = progress;
            }
        }


        public ImagesDatabase() { }


        public void CreateDatabase(string path, string outputPath, int newSize)
        {
            double count = 0;
            int j = 0;

            Images = new List<ImageItem>();
            DBPath = path;
            OutputPath = outputPath;
            ImagesSize = newSize;

            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            var files = Directory.GetFiles(DBPath, "*", SearchOption.AllDirectories);
            NumOfImages = files.Length;

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 5 };
            Parallel.ForEach(files, options, (file) =>
            {
                if (ImageExtensions.Contains(Path.GetExtension(file).ToUpperInvariant()) && !Path.GetDirectoryName(file).Equals(OutputPath))
                {
                    var item = new ImageItem(file, OutputPath, ImagesSize);
                    lock (_lockResources)
                    {
                        count++;
                        if (count > j)
                        {
                            double dprogress = (count / NumOfImages) * 100;
                            ProgressChanged?.Invoke(this, new ProgressChangedEventHandler((int)dprogress));
                            j += 250;
                        }
                        Images.Add(item);
                    }
                }
            });
        }
    }
}

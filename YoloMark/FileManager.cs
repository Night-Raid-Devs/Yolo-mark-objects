using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace YoloMark
{
    public sealed class FileManager
    {
        private static FileManager instance;

        private string ImageFolder = AppDomain.CurrentDomain.BaseDirectory + @"data\img\";
        private string TrainFilename = AppDomain.CurrentDomain.BaseDirectory + @"data\train.txt";
        private string ObjNamesFilename = AppDomain.CurrentDomain.BaseDirectory + @"data\obj.names";

        private string[] ImageFilrnames;
        private string[] ObjectFilenames;

        private BitmapImage[] PreviewImages;

        private string[] YoloObjectNames;

        private FileManager()
        {
        }

        public static FileManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FileManager();
                }

                return instance;
            }
        }

        public List<YoloObject> YoloObjects { get; set; } = new List<YoloObject>();

        public int ImagesCount
        {
            get
            {
                return this.ImageFilrnames.Length;
            }
        }

        public int YoloObjectsCount
        {
            get
            {
                return this.YoloObjectNames.Length;
            }
        }

        public BitmapImage[] GetPreviewImages(int previewImagesCount, out bool[] isChecked)
        {
            if (this.ImageFilrnames == null)
            {
                this.Initialize();
            }

            int startIndx = this.GetStartImageNumber();
            return this.GetPreviewImages(previewImagesCount, startIndx, out isChecked);
        }

        public BitmapImage[] GetPreviewImages(int previewImagesCount, int currentImageNumber, out bool[] isChecked)
        {
            isChecked = new bool[previewImagesCount];
            if (this.ImageFilrnames == null)
            {
                this.Initialize();
            }
            
            this.PreviewImages = new BitmapImage[previewImagesCount];

            if (currentImageNumber > 0)
            {
                this.PreviewImages[0] = new BitmapImage(new Uri(ImageFilrnames[currentImageNumber - 1]));
                if (File.Exists(Path.GetFileNameWithoutExtension(ImageFilrnames[currentImageNumber - 1]) + ".txt"))
                {
                    isChecked[0] = true;
                }
                else
                {
                    isChecked[0] = false;
                }
            }
            
            int lastPreviewImageNumber = Math.Min(previewImagesCount, ImageFilrnames.Length - currentImageNumber);
            for (int i = 1; i < lastPreviewImageNumber; i++, currentImageNumber++)
            {
                this.PreviewImages[i] = new BitmapImage(new Uri(ImageFilrnames[currentImageNumber]));
                if (File.Exists(Path.GetFileNameWithoutExtension(ImageFilrnames[currentImageNumber]) + ".txt"))
                {
                    isChecked[i] = true;
                }
                else
                {
                    isChecked[i] = false;
                }
            }

            return this.PreviewImages;
        }

        public void Initialize()
        {
            List<string> imageNames = new List<string>(Directory.GetFiles(ImageFolder, "*.jpg"));
            imageNames.Sort();
            this.ImageFilrnames = imageNames.ToArray();

            List<string> objectFileNames = new List<string>(Directory.GetFiles(ImageFolder, "*.txt"));
            objectFileNames.Sort();
            this.ObjectFilenames = objectFileNames.ToArray();

            List<string> objNames = new List<string>();
            foreach (string line in File.ReadLines(this.ObjNamesFilename))
            {
                if (!String.IsNullOrWhiteSpace(line))
                {
                    objNames.Add(line);
                }
            }

            this.YoloObjectNames = objNames.ToArray();

            this.CreateTrainFile();
        }

        public string GetYoloObjectName(int objectNumber)
        {
            return this.YoloObjectNames[objectNumber];
        }

        public int GetStartImageNumber()
        {
            int indx = 0;
            for (indx = 0; indx < this.ImageFilrnames.Length && indx < this.ObjectFilenames.Length; indx++)
            {
                if (Path.GetFileNameWithoutExtension(this.ImageFilrnames[indx]) != Path.GetFileNameWithoutExtension(this.ObjectFilenames[indx]))
                {
                    break;
                }
            }

            if (indx < this.ImageFilrnames.Length && indx < ObjectFilenames.Length)
            {
                return indx;
            }

            return 0;
        }

        public void AddYoloObject(int imageNumber, int objectId, Point point1, double rectWidth, double rectHeight, double imageHeight, double imageWidth)
        {
            this.YoloObjects.Add(new YoloObject(objectId, point1, rectWidth, rectHeight, imageHeight, imageWidth));

            this.RewriteObjectFile(imageNumber);   
        }

        public void ClearYoloObjects()
        {
            this.YoloObjects.Clear();
        }

        public void RemoveYoloObject(int currentImageNumber)
        {
            this.YoloObjects.Clear();
            if (File.Exists(ImageFolder + currentImageNumber + ".txt"))
            {
                File.Delete(ImageFolder + currentImageNumber + ".txt");
            }
        }

        public void RemoveLastYoloObject(int currentImageNumber)
        {
            this.YoloObjects.RemoveAt(this.YoloObjects.Count - 1);

            this.RewriteObjectFile(currentImageNumber);
        }

        private void CreateTrainFile()
        {
            try
            {
                Debug.WriteLine("Create train file");
                StreamWriter fout = new StreamWriter(TrainFilename);
                foreach (string str in ImageFilrnames)
                {
                    fout.WriteLine(str);
                }

                fout.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void RewriteObjectFile(int imageNumber)
        {
            StreamWriter fout = new StreamWriter(Path.GetFileNameWithoutExtension(this.ImageFilrnames[imageNumber]) + ".txt");
            foreach (YoloObject yoloObj in this.YoloObjects)
            {
                fout.WriteLine(yoloObj.ToString());
            }

            fout.Close();
        }
    }
}

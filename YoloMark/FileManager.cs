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
        private string NamesFilename = AppDomain.CurrentDomain.BaseDirectory + @"data\obj.names";

        private string[] ImageNames;

        private BitmapImage[] PreviewImages;

        private string[] YoloObjectNames;

        private List<YoloObject> yoloObjects = new List<YoloObject>();

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

        public BitmapImage[] GetPreviewImages(int previewImagesCount, out bool[] isChecked)
        {
            if (this.ImageNames == null)
            {
                this.Initialize();
            }

            int startIndx = this.GetStartImageNumber();
            return this.GetPreviewImages(previewImagesCount, startIndx, out isChecked);
        }

        public BitmapImage[] GetPreviewImages(int previewImagesCount, int currentImageNumber,out bool[] isChecked)
        {
            isChecked = new bool[previewImagesCount];
            if (this.ImageNames == null)
            {
                this.Initialize();
            }
            
            this.PreviewImages = new BitmapImage[previewImagesCount];

            if (currentImageNumber > 0)
            {
                this.PreviewImages[0] = new BitmapImage(new Uri(ImageNames[currentImageNumber - 1]));
                if (File.Exists(this.ImageFolder + ImageNames[currentImageNumber - 1] + ".txt"))
                {
                    isChecked[0] = true;
                }
                else
                {
                    isChecked[0] = false;
                }
            }

            int lastPreviewImageNumber = Math.Min(previewImagesCount, ImageNames.Length - currentImageNumber);
            for (int i = 1; i < lastPreviewImageNumber; i++)
            {
                this.PreviewImages[i] = new BitmapImage(new Uri(ImageNames[i]));
                if (File.Exists(this.ImageFolder + ImageNames[i] + ".txt"))
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
            this.ImageNames = imageNames.ToArray();

            this.CreateTrainFile();
        }

        public int GetStartImageNumber()
        {
            List<string> imageNames = new List<string>(Directory.GetFiles(ImageFolder, "*.jpg"));
            imageNames.Sort();

            List<string> objectFileNames = new List<string>(Directory.GetFiles(ImageFolder, "*.txt"));
            objectFileNames.Sort();

            int indx = 0;
            for (indx = 0; indx < imageNames.Count && indx < objectFileNames.Count; indx++)
            {
                if (Path.GetFileNameWithoutExtension(imageNames[indx]) != Path.GetFileNameWithoutExtension(objectFileNames[indx]))
                {
                    break;
                }
            }

            if (indx < imageNames.Count && indx < objectFileNames.Count)
            {
                return indx;
            }

            return 0;
        }

        public void AddYoloObject(int imageNumber, int objectId, Point point1, Point point2, double imageHeight, double imageWidth)
        {
            this.yoloObjects.Add(new YoloObject(objectId, point1, point2, imageHeight, imageWidth));

            StreamWriter fout = new StreamWriter(this.ImageFolder + this.ImageNames[imageNumber] + ".txt");
            foreach (YoloObject yoloObj in this.yoloObjects)
            {
                fout.WriteLine(yoloObj.ToString());
            }

            fout.Close();
        }

        public void ClearYoloObjects()
        {
            this.yoloObjects.Clear();
        }

        public void RemoveYoloObject(int currentImageNumber)
        {
            this.yoloObjects.Clear();
            if (File.Exists(ImageFolder + currentImageNumber + ".txt"))
            {
                File.Delete(ImageFolder + currentImageNumber + ".txt");
            }
        }

        private void CreateTrainFile()
        {
            try
            {
                Debug.WriteLine("Create train file");
                StreamWriter fout = new StreamWriter(TrainFilename);
                foreach (string str in ImageNames)
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
    }
}

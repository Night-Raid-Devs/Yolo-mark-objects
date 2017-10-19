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

        private string[] ImageNames;

        private BitmapImage[] PreviewImages;

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

        public BitmapImage[] GetPreviewImages
        {
            get
            {
                if (this.PreviewImages == null)
                {
                    this.Initialize();
                }

                return this.PreviewImages;
            }
        }

        public void Initialize(int previewImagesCount = 5)
        {
            List<string> imageNames = new List<string>(Directory.GetFiles(ImageFolder, "*.jpg"));
            imageNames.Sort();
            this.ImageNames = imageNames.ToArray();

            this.PreviewImages = new BitmapImage[previewImagesCount];

            int startIndx = this.GetStartImageNumber();
            if (startIndx > 0)
            {
                this.PreviewImages[0] = new BitmapImage(new Uri(imageNames[startIndx - 1]));
            }

            int lastPreviewImageNumber = Math.Min(previewImagesCount, imageNames.Count - startIndx);
            for (int i = 1; i < lastPreviewImageNumber; i++)
            {
                this.PreviewImages[i] = new BitmapImage(new Uri(imageNames[i]));
            }
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

        public void CreateTrainFile()
        {
            try
            {
                Debug.WriteLine("Create train file");
                string[] imgFiles = Directory.GetFiles(ImageFolder, "*.jpg");
                StreamWriter fout = new StreamWriter(TrainFilename);
                foreach (string str in imgFiles)
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
            StreamWriter sw = new StreamWriter(this.ImageFolder + this.ImageNames[currentImageNumber] + ".txt", false);
            sw.Close();
        }
    }
}

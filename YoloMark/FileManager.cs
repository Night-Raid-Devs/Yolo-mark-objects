using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace YoloMark
{
    public sealed class FileManager
    {
        private static FileManager instance;

        private string ImageFolder = AppDomain.CurrentDomain.BaseDirectory + @"data\img\";
        private string TrainFileName = AppDomain.CurrentDomain.BaseDirectory + @"data\train.txt";
        private string ObjNamesFileName = AppDomain.CurrentDomain.BaseDirectory + @"data\obj.names";

        private string[] ImageFileNames;
        private string[] ObjectFileNames;
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
                return this.ImageFileNames.Length;
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
            if (this.ImageFileNames == null)
            {
                this.Initialize();
            }

            int startIndx = this.GetStartImageNumber();
            return this.GetPreviewImages(previewImagesCount, startIndx, out isChecked);
        }

        public BitmapImage[] GetPreviewImages(int previewImagesCount, int currentImageNumber, out bool[] isChecked)
        {
            if (this.ImageFileNames == null)
            {
                this.Initialize();
            }
            
            BitmapImage[] previewImages = new BitmapImage[previewImagesCount];
            isChecked = new bool[previewImagesCount];
            for (int i = 0; i < previewImagesCount; i++)
            {
                int imageNumber = i + currentImageNumber - 1;
                if (imageNumber < 0 || imageNumber >= this.ImageFileNames.Length || !File.Exists(this.ImageFileNames[imageNumber]))
                {
                    continue;
                }

                previewImages[i] = new BitmapImage(new Uri(ImageFileNames[imageNumber]));
                if (File.Exists(ImageFolder + GetFileName(ImageFileNames[imageNumber]) + ".txt"))
                {
                    isChecked[i] = true;
                }
            }

            this.YoloObjects = this.GetYoloObjectsFromFile(currentImageNumber);
            return previewImages;
        }

        public List<YoloObject> GetYoloObjectsFromFile(int currentImageNumber)
        {
            List<YoloObject> yoloObjects = new List<YoloObject>();
            string textFileName = ImageFolder + GetFileName(this.ImageFileNames[currentImageNumber]) + ".txt";
            if (File.Exists(textFileName))
            {
                try
                {
                    foreach (string line in File.ReadLines(textFileName))
                    {
                        if (!String.IsNullOrWhiteSpace(line))
                        {
                            string[] data = line.Trim().Split(' ');
                            yoloObjects.Add(new YoloObject(Convert.ToInt32(data[0]),
                                Convert.ToDouble(data[1], CultureInfo.InvariantCulture),
                                Convert.ToDouble(data[2], CultureInfo.InvariantCulture),
                                Convert.ToDouble(data[3], CultureInfo.InvariantCulture),
                                Convert.ToDouble(data[4], CultureInfo.InvariantCulture)
                                ));
                        }
                    }
                }
                catch (Exception)
                {
                    return yoloObjects;
                }
            }

            return yoloObjects;
        }

        private static string[] GetImageFileNames(string imageFolder, params string[] extensions)
        {
            List<string> imageFileNames = new List<string>();
            foreach(string extension in extensions)
            {
                imageFileNames.AddRange(Directory.GetFiles(imageFolder, extension));
            }

            return imageFileNames.ToArray();
        }

        public void Initialize()
        {
            string[] imageFileNames = GetImageFileNames(ImageFolder, "*.jpg", ".JPG", "*.jpeg", "*.JPEG", "*.png", "*.bmp", "*.gif");
            Array.Sort(imageFileNames);
            this.ImageFileNames = imageFileNames;

            string[] objectFileNames = Directory.GetFiles(ImageFolder, "*.txt");
            Array.Sort(objectFileNames);
            this.ObjectFileNames = objectFileNames;

            List<string> objNames = new List<string>();
            foreach (string line in File.ReadLines(this.ObjNamesFileName))
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
            for (int i = 0; i < this.ImageFileNames.Length; i++)
            {
                if (i >= this.ObjectFileNames.Length || GetFileName(this.ImageFileNames[i]) != GetFileName(this.ObjectFileNames[i]))
                {
                    return i;
                }
            }

            return 0;
        }

        public void AddYoloObject(int imageNumber, int objectNumber, Point point1, double rectWidth, double rectHeight, double imageWidth, double imageHeight)
        {
            this.YoloObjects.Add(new YoloObject(objectNumber, point1, rectWidth, rectHeight, imageWidth, imageHeight));

            this.RewriteObjectFile(imageNumber);   
        }

        public void ClearYoloObjects()
        {
            this.YoloObjects.Clear();
        }

        public void RemoveYoloFile(int currentImageNumber)
        {
            this.YoloObjects.Clear();
            string textFileName = ImageFolder + GetFileName(this.ImageFileNames[currentImageNumber]) + ".txt";
            if (File.Exists(textFileName))
            {
                File.Delete(textFileName);
            }
        }

        public void RemoveLastYoloObject(int currentImageNumber)
        {
            this.YoloObjects.RemoveAt(this.YoloObjects.Count - 1);
            this.RewriteObjectFile(currentImageNumber);
            if (this.YoloObjects.Count == 0)
            {
                this.RemoveYoloFile(currentImageNumber);
            }
        }

        private void CreateTrainFile()
        {
            try
            {
                Debug.WriteLine("Create train file");
                StreamWriter fout = new StreamWriter(TrainFileName);
                foreach (string str in ImageFileNames)
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
            StreamWriter fout = new StreamWriter(ImageFolder + GetFileName(this.ImageFileNames[imageNumber]) + ".txt");
            foreach (YoloObject yoloObj in this.YoloObjects)
            {
                fout.WriteLine(yoloObj.ToString());
            }

            fout.Close();
        }

        private static string GetFileName(string fullFileName)
        {
            return Path.GetFileNameWithoutExtension(fullFileName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace YoloMark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ImgFolder = AppDomain.CurrentDomain.BaseDirectory + @"data\img\";
        private string TrainFilename = AppDomain.CurrentDomain.BaseDirectory + @"data\train.txt";
        private string NamesFilename = AppDomain.CurrentDomain.BaseDirectory + @"data\obj.names";

        private BitmapImage mainImage;
        private Polygon selectionRect;
        private Point selectionRectStartPos;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string[] imgFiles = Directory.GetFiles(ImgFolder, "*.jpg");
            StreamWriter fout = new StreamWriter(TrainFilename);
            foreach (string str in imgFiles)
            {
                fout.WriteLine(str);
            }
            fout.Close();

            this.mainImage = new BitmapImage(new Uri(imgFiles[0]));
            mainCanvas.Background = new ImageBrush(this.mainImage);
        }

        private void mainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            textBoxWidth.Text = e.NewSize.Width.ToString();
            textBoxHeight.Text = e.NewSize.Height.ToString();
        }

        private void resizeButton_Click(object sender, RoutedEventArgs e)
        {
            int width = Convert.ToInt32(textBoxWidth.Text) + (int)mainCanvas.Margin.Left + (int)mainCanvas.Margin.Right + 16;
            int height = Convert.ToInt32(textBoxHeight.Text) + (int)mainCanvas.Margin.Top + (int)mainCanvas.Margin.Bottom + 39;
            mainWindow.Width = width;
            mainWindow.Height = height;
        }

        private void mainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.selectionRect = new Polygon()
            {
                Stroke = new SolidColorBrush(Colors.Blue),
                ////Fill = new SolidColorBrush(Color.FromArgb(20, 0, 0, 255))
            };
            this.selectionRectStartPos = Mouse.GetPosition(mainCanvas);
            this.selectionRect.Points.Add(new Point(this.selectionRectStartPos.X, this.selectionRectStartPos.Y));
            this.selectionRect.Points.Add(new Point(this.selectionRectStartPos.X, this.selectionRectStartPos.Y));
            this.selectionRect.Points.Add(new Point(this.selectionRectStartPos.X, this.selectionRectStartPos.Y));
            this.selectionRect.Points.Add(new Point(this.selectionRectStartPos.X, this.selectionRectStartPos.Y));
            mainCanvas.Children.Add(this.selectionRect);
        }

        private void mainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.selectionRect != null)
            {
                Point mousePos = Mouse.GetPosition(mainCanvas);
                this.selectionRect.Points[1] = new Point(this.selectionRect.Points[1].X, mousePos.Y);
                this.selectionRect.Points[2] = new Point(mousePos.X, mousePos.Y);
                this.selectionRect.Points[3] = new Point(mousePos.X, this.selectionRect.Points[3].Y);
            }
        }

        private void mainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.selectionRect != null)
            {
                this.selectionRect = null;
            }
        }

        private void mainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.selectionRect != null)
            {
                mainCanvas.Children.Remove(this.selectionRect);
                this.selectionRect = null;
            }
        }
    }
}

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
        private const int PreviewImagesCount = 5;
        private const int SelectBoxBorderWidth = 5;
        private const int SelectBoxObjectNameFontSize = 30;

        private string ImgFolder = AppDomain.CurrentDomain.BaseDirectory + @"data\img\";
        private string TrainFilename = AppDomain.CurrentDomain.BaseDirectory + @"data\train.txt";
        private string NamesFilename = AppDomain.CurrentDomain.BaseDirectory + @"data\obj.names";

        private Polygon selectBox;
        private Point selectBoxStartPos;
        private TextBlock selectBoxObjectName;
        private Random random = new Random();
        private int currentImageNumber = 0;
        private int currentObjectNumber = 0;
        private Image[] previewImages = new Image[PreviewImagesCount];
        private Image[] previewImagesCheck = new Image[PreviewImagesCount];

        public MainWindow()
        {
            InitializeComponent();
            this.previewImages[0] = ImagePreview1;
            this.previewImages[1] = ImagePreview2;
            this.previewImages[2] = ImagePreview3;
            this.previewImages[3] = ImagePreview4;
            this.previewImages[4] = ImagePreview5;
            this.previewImagesCheck[0] = ImagePreview1Check;
            this.previewImagesCheck[1] = ImagePreview2Check;
            this.previewImagesCheck[2] = ImagePreview3Check;
            this.previewImagesCheck[3] = ImagePreview4Check;
            this.previewImagesCheck[4] = ImagePreview5Check;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string[] imgFiles = Directory.GetFiles(ImgFolder, "*.jpg");
            StreamWriter fout = new StreamWriter(TrainFilename);
            foreach (string str in imgFiles)
            {
                fout.WriteLine(str);
            }
            fout.Close();

            MainCanvas.Background = new ImageBrush(new BitmapImage(new Uri(imgFiles[0])));


        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBoxWidth.Text = e.NewSize.Width.ToString();
            TextBoxHeight.Text = e.NewSize.Height.ToString();
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e)
        {
            int width = Convert.ToInt32(TextBoxWidth.Text) + (int)MainCanvas.Margin.Left + (int)MainCanvas.Margin.Right + 16;
            int height = Convert.ToInt32(TextBoxHeight.Text) + (int)MainCanvas.Margin.Top + (int)MainCanvas.Margin.Bottom + 39;
            MainWnd.Width = width;
            MainWnd.Height = height;
        }

        private void AddSelectedRect()
        {
            if (this.selectBox != null)
            {
                this.selectBox = null;
                return;
            }


        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.AddSelectedRect();
            SolidColorBrush boxColor = new SolidColorBrush(Color.FromRgb((byte)(random.Next() % 256), (byte)(random.Next() % 256), (byte)(random.Next() % 256)));
            this.selectBox = new Polygon()
            {
                StrokeThickness = SelectBoxBorderWidth,
                Stroke = boxColor
            };
            this.selectBoxStartPos = Mouse.GetPosition(MainCanvas);
            this.selectBox.Points.Add(new Point(this.selectBoxStartPos.X, this.selectBoxStartPos.Y));
            this.selectBox.Points.Add(new Point(this.selectBoxStartPos.X, this.selectBoxStartPos.Y));
            this.selectBox.Points.Add(new Point(this.selectBoxStartPos.X, this.selectBoxStartPos.Y));
            this.selectBox.Points.Add(new Point(this.selectBoxStartPos.X, this.selectBoxStartPos.Y));
            MainCanvas.Children.Add(this.selectBox);

            this.selectBoxObjectName = new TextBlock()
            {
                Text = "test",
                Foreground = boxColor,
                FontSize = SelectBoxObjectNameFontSize
            };
            Canvas.SetLeft(this.selectBoxObjectName, this.selectBoxStartPos.X + SelectBoxBorderWidth);
            Canvas.SetTop(this.selectBoxObjectName, this.selectBoxStartPos.Y + SelectBoxBorderWidth - 10);
            MainCanvas.Children.Add(this.selectBoxObjectName);
        }

        private static Point GetLeftTopPoint(PointCollection points)
        {
            Point res = new Point(int.MaxValue, int.MaxValue);
            foreach (Point point in points)
            {
                if ((int)point.X < (int)res.X || ((int)point.X == (int)res.X && (int)point.Y < (int)res.Y))
                {
                    res = point;
                }
            }

            return res;
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.selectBox != null)
            {
                Point mousePos = Mouse.GetPosition(MainCanvas);
                if (mousePos.X < 0 || mousePos.Y < 0 || mousePos.X > MainCanvas.Width || mousePos.Y > MainCanvas.Height)
                {
                    return;
                }

                this.selectBox.Points[1] = new Point(this.selectBox.Points[0].X, mousePos.Y);
                this.selectBox.Points[2] = new Point(mousePos.X, mousePos.Y);
                this.selectBox.Points[3] = new Point(mousePos.X, this.selectBox.Points[0].Y);

                Point leftTopPoint = GetLeftTopPoint(this.selectBox.Points);
                Canvas.SetLeft(this.selectBoxObjectName, leftTopPoint.X + SelectBoxBorderWidth);
                Canvas.SetTop(this.selectBoxObjectName, leftTopPoint.Y + SelectBoxBorderWidth - 10);
            }
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.AddSelectedRect();
        }

        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.selectBox != null)
            {
                MainCanvas.Children.Remove(this.selectBox);
                this.selectBox = null;
            }
        }

        private void SliderImageNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.currentImageNumber = (int)e.NewValue;
        }

        private void SliderObjectNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.currentObjectNumber = (int)e.NewValue;
        }
    }
}

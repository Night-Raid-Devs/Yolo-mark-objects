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
using System.Diagnostics;

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
        private const int MinSelectBoxWidth = 15;
        private const int MinSelectBoxHeight = 15;

        private Dictionary<Key, int> keyObjectBinding = new Dictionary<Key, int>()
        {
            { Key.D3, 0 },
            { Key.D4, 1 },
            { Key.D5, 2 },
            { Key.D6, 3 },
            { Key.D8, 4 },
            { Key.D9, 5 },
            { Key.NumPad3, 0 },
            { Key.NumPad4, 1 },
            { Key.NumPad5, 2 },
            { Key.NumPad6, 3 },
            { Key.NumPad8, 4 },
            { Key.NumPad9, 5 },
            { Key.A, 6 },
            { Key.B, 7 },
            { Key.C, 8 },
            { Key.D, 9 },
            { Key.E, 10 },
            { Key.H, 11 },
            { Key.J, 12 },
            { Key.K, 13 },
            { Key.M, 14 },
            { Key.N, 15 },
            { Key.P, 16 },
            { Key.R, 17 },
            { Key.S, 18 },
            { Key.T, 19 },
            { Key.U, 20 },
            { Key.V, 21 },
            { Key.W, 22 },
            { Key.X, 23 },
            { Key.Y, 24 },
        };

        private Rectangle selectBox;
        private Point selectBoxStartPos;
        private TextBlock selectBoxTextBlock;
        private Random random = new Random();
        private Image[] previewImages = new Image[PreviewImagesCount];
        private Image[] previewImagesCheck = new Image[PreviewImagesCount];
        private List<Rectangle> currentBoxes = new List<Rectangle>();
        private List<TextBlock> currentTextBlocks = new List<TextBlock>();

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
            FileManager.Instance.Initialize();
            SliderImageNumber.Maximum = FileManager.Instance.ImagesCount;
            SliderObjectNumber.Maximum = FileManager.Instance.YoloObjectsCount;
            SliderImageNumber.Value = FileManager.Instance.GetStartImageNumber();
            this.ChangeImages((int)SliderImageNumber.Value);
        }

        private void ChangeImages(int currentImageNumber)
        {
            BitmapImage[] previewBitmapImages = FileManager.Instance.GetPreviewImages(PreviewImagesCount, currentImageNumber, out bool[] isCheched);
            for (int i = 0; i < previewImages.Length; i++)
            {
                this.previewImages[i].Source = previewBitmapImages[i];
                this.previewImagesCheck[i].Visibility = isCheched[i] ? Visibility.Visible : Visibility.Hidden;
            }

            MainCanvas.Background = new ImageBrush(previewBitmapImages[1]);
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e)
        {
            int width = Convert.ToInt32(TextBoxWidth.Text) + (int)MainCanvas.Margin.Left + (int)MainCanvas.Margin.Right + 16;
            int height = Convert.ToInt32(TextBoxHeight.Text) + (int)MainCanvas.Margin.Top + (int)MainCanvas.Margin.Bottom + 39;
            MainWnd.Width = width;
            MainWnd.Height = height;
        }

        private static Point GetTopLeftPoint(PointCollection points)
        {
            Point res = new Point(int.MaxValue, int.MaxValue);
            foreach (Point point in points)
            {
                if ((int)point.Y < (int)res.Y || ((int)point.Y == (int)res.Y && (int)point.X < (int)res.X))
                {
                    res = point;
                }
            }

            return res;
        }

        private static Point GetBottomRightPoint(PointCollection points)
        {
            Point res = new Point(int.MinValue, int.MinValue);
            foreach (Point point in points)
            {
                if ((int)point.Y > (int)res.Y || ((int)point.Y == (int)res.Y && (int)point.X > (int)res.X))
                {
                    res = point;
                }
            }

            return res;
        }

        private void MainWnd_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.selectBox != null)
            {
                Point mousePos = Mouse.GetPosition(MainCanvas);
                mousePos.X = mousePos.X < 0 ? 0 : mousePos.X;
                mousePos.Y = mousePos.Y < 0 ? 0 : mousePos.Y;
                mousePos.X = mousePos.X > MainCanvas.ActualWidth ? MainCanvas.ActualWidth : mousePos.X;
                mousePos.Y = mousePos.Y > MainCanvas.ActualHeight ? MainCanvas.ActualHeight : mousePos.Y;

                var pointCollection = new PointCollection()
                {
                    this.selectBoxStartPos,
                    mousePos,
                    new Point(this.selectBoxStartPos.X, mousePos.Y),
                    new Point(mousePos.X, this.selectBoxStartPos.Y)
                };
                Point topLeftPoint = GetTopLeftPoint(pointCollection);
                Point bottomRightPoint = GetBottomRightPoint(pointCollection);
                Canvas.SetLeft(this.selectBox, topLeftPoint.X);
                Canvas.SetTop(this.selectBox, topLeftPoint.Y);
                this.selectBox.Width = bottomRightPoint.X - topLeftPoint.X;
                this.selectBox.Height = bottomRightPoint.Y - topLeftPoint.Y;

                Canvas.SetLeft(this.selectBoxTextBlock, topLeftPoint.X + SelectBoxBorderWidth);
                Canvas.SetTop(this.selectBoxTextBlock, topLeftPoint.Y + SelectBoxBorderWidth - 10);
            }
        }

        private bool AddSelectBox()
        {
            if (this.selectBox != null)
            {
                if (this.selectBox.Width < MinSelectBoxWidth || this.selectBox.Height < MinSelectBoxHeight)
                {
                    this.RemoveSelectBox();
                    return false;
                }

                this.currentBoxes.Add(this.selectBox);
                this.currentTextBlocks.Add(this.selectBoxTextBlock);
                double left = Canvas.GetLeft(this.selectBox);
                double top = Canvas.GetTop(this.selectBox);
                ////FileManager.Instance.AddYoloObject((int)SliderImageNumber.Value, (int)SliderObjectNumber.Value,
                ////    new Point(left, top),
                ////    new Point());
                this.selectBox = null;
                return true;
            }

            return false;
        }

        private void AddBox(Point leftTopPoint, double width, double height)
        {

        }

        private bool RemoveSelectBox()
        {
            if (this.selectBox != null)
            {
                MainCanvas.Children.Remove(this.selectBox);
                MainCanvas.Children.Remove(this.selectBoxTextBlock);
                this.selectBox = null;
                return true;
            }

            return false;
        }

        private void RemoveBox(int objectNumber)
        {
            MainCanvas.Children.Remove(this.currentBoxes[objectNumber]);
            MainCanvas.Children.Remove(this.currentTextBlocks[objectNumber]);
            this.currentBoxes.RemoveAt(objectNumber);
            this.currentTextBlocks.RemoveAt(objectNumber);
        }

        private void RemoveCurrentBoxes()
        {
            for (int i = 0; i < this.currentBoxes.Count; i++)
            {
                MainCanvas.Children.Remove(this.currentBoxes[i]);
                MainCanvas.Children.Remove(this.currentTextBlocks[i]);
            }

            this.currentBoxes.Clear();
            this.currentTextBlocks.Clear();
            FileManager.Instance.ClearYoloObjects();
        }

        private void MainWnd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.AddSelectBox())
            {
                return;
            }

            Point mousePos = Mouse.GetPosition(MainCanvas);
            if (mousePos.X < 0 || mousePos.Y < 0 || mousePos.X > MainCanvas.ActualWidth || mousePos.Y > MainCanvas.ActualHeight)
            {
                return;
            }

            SolidColorBrush boxColor = new SolidColorBrush(Color.FromRgb((byte)(random.Next() % 256), (byte)(random.Next() % 256), (byte)(random.Next() % 256)));
            this.selectBox = new Rectangle()
            {
                StrokeThickness = SelectBoxBorderWidth,
                Stroke = boxColor
            };
            this.selectBoxStartPos = mousePos;
            Canvas.SetLeft(this.selectBox, this.selectBoxStartPos.X);
            Canvas.SetTop(this.selectBox, this.selectBoxStartPos.Y);
            this.selectBox.Width = 0;
            this.selectBox.Height = 0;
            MainCanvas.Children.Add(this.selectBox);

            this.selectBoxTextBlock = new TextBlock()
            {
                Text = SliderObjectNumber.Value.ToString(),
                Foreground = boxColor,
                FontSize = SelectBoxObjectNameFontSize
            };
            Canvas.SetLeft(this.selectBoxTextBlock, this.selectBoxStartPos.X + SelectBoxBorderWidth);
            Canvas.SetTop(this.selectBoxTextBlock, this.selectBoxStartPos.Y + SelectBoxBorderWidth - 10);
            MainCanvas.Children.Add(this.selectBoxTextBlock);
        }

        private void MainWnd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.AddSelectBox();
        }

        private void MainWnd_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.RemoveSelectBox();
        }

        private void MainWnd_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.keyObjectBinding.ContainsKey(e.Key))
            {
                SliderObjectNumber.Value = this.keyObjectBinding[e.Key];
            }
        }

        private void MainWnd_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    SliderImageNumber.Value--;
                    this.RemoveCurrentBoxes();
                    break;
                case Key.Right:
                    SliderImageNumber.Value++;
                    this.RemoveCurrentBoxes();
                    break;
                case Key.Back:
                    if (this.currentBoxes.Count > 0)
                    {
                        this.RemoveBox(this.currentBoxes.Count - 1);
                    }

                    break;
                case Key.Delete:
                    this.RemoveCurrentBoxes();
                    FileManager.Instance.RemoveYoloObject((int)SliderImageNumber.Value);
                    break;
            }
        }

        private void SliderImageNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ChangeImages((int)e.NewValue);
        }
    }
}

using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace YoloMark
{
    public class YoloObject
    {
        public int Number { get; private set; }

        public double X { get; private set; } // relative center of rectangle

        public double Y { get; private set; } // relative center of rectangle

        public double Width { get; private set; } // relative width of rectangle

        public double Height { get; private set; } // relative height of rectangle

        public YoloObject(int number, double x, double y, double width, double height)
        {
            this.Number = number;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public YoloObject(int number, Point upperLeftPoint, double rectWidth, double rectHeight, double imageWidth, double imageHeight)
        {
            Debug.WriteLine("Initialize YoloObject");
            this.Number = number;
            double leftX = upperLeftPoint.X;
            double topY = upperLeftPoint.Y;

            double rightX = upperLeftPoint.X + rectWidth;
            double bottomY = upperLeftPoint.Y + rectHeight;
     

            this.X = ((rightX + leftX) / 2) / imageWidth;
            this.Y = ((topY + bottomY) / 2) / imageHeight;

            this.Height = rectHeight / imageHeight;
            this.Width = rectWidth / imageWidth;
        }
         
        public void GetRectangle(out Point upperLeftCorner, out double rectWidth, out double rectHeight, double imageWidth, double imageHeight)
        {
            rectWidth = imageWidth * this.Width;
            rectHeight = imageHeight * this.Height;
            
            double leftX = imageWidth * this.X - rectWidth / 2;

            double topY = imageHeight * this.Y - rectHeight / 2;

            upperLeftCorner = new Point(leftX, topY);
        }

        public override string ToString() ////returns <x> <y> <width> <height>
        {
            return this.Number + " " + this.X.ToString("f6", CultureInfo.InvariantCulture) + " " + this.Y.ToString("f6", CultureInfo.InvariantCulture) +
                   " " + this.Width.ToString("f6", CultureInfo.InvariantCulture) + " " + this.Height.ToString("f6", CultureInfo.InvariantCulture);
        }
    }
}

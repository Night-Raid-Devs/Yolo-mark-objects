using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace YoloMark
{
    public class YoloObject
    {
        public int Id { get; private set; }

        public double X { get; private set; } ////relative center of rectangle

        public double Y { get; private set; } ////relative center of rectangle

        public double Height { get; private set; } ////relative height of rectangle

        public double Width { get; private set; } ////relative width of rectangle

        public YoloObject(int id, Point upperLeftPoint, double rectWidth, double rectHeight, double imageHeight, double imageWidth)
        {
            Debug.WriteLine("Initialize YoloObject");
            this.Id = id;
            double leftX = upperLeftPoint.X;
            double topY = upperLeftPoint.Y;

            double rightX = upperLeftPoint.X + rectWidth;
            double bottomY = upperLeftPoint.Y + rectHeight;
     

            this.X = ((rightX + leftX) / 2) / imageWidth;
            this.Y = ((topY + bottomY) / 2) / imageHeight;

            this.Height = rectHeight / imageHeight;
            this.Width = rectWidth / imageWidth;
        }
         
        public void GetRectangle(out Point upperLeftCorner, out double rectWidth, out double rectHeight, double imageHeight, double imageWidth)
        {
            rectWidth = imageWidth * this.Width;
            rectHeight = imageHeight * this.Height;
            
            double leftX = imageWidth * this.X - rectWidth / 2;

            double topY = imageHeight * this.Y + rectHeight / 2;

            upperLeftCorner = new Point(leftX, topY);
        }

        public override string ToString() ////returns <x> <y> <width> <height>
        {
            return this.Id + " " + this.X.ToString("f6", CultureInfo.InvariantCulture) + " " + this.Y.ToString("f6", CultureInfo.InvariantCulture) +
                   " " + this.Width.ToString("f6", CultureInfo.InvariantCulture) + " " + this.Height.ToString("f6", CultureInfo.InvariantCulture);
        }
    }
}

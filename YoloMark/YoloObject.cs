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

        public YoloObject(int id, Point point1, Point point2, double imageHeight, double imageWidth)
        {
            Debug.WriteLine("Initialize YoloObject");
            this.Id = id;
            double leftX = point1.X < point2.X ? point1.X : point2.X;
            double downY = point1.Y < point2.Y ? point1.Y : point2.Y;
            double rightX = point1.X > point2.X ? point1.X : point2.X;
            double topY = point1.Y > point2.Y ? point1.Y : point2.Y;
            double rectHeight = topY - downY;
            double rectWidth = rightX - leftX;

            this.X = ((rightX + leftX) / 2) / imageWidth;
            this.Y = ((topY + downY) / 2) / imageHeight;

            this.Height = rectHeight / imageHeight;
            this.Width = rectWidth / imageWidth;
        }

        public override string ToString() ////returns <x> <y> <width> <height>
        {
            return this.Id + " " + this.X.ToString("f6", CultureInfo.InvariantCulture) + " " + this.Y.ToString("f6", CultureInfo.InvariantCulture) +
                   " " + this.Width.ToString("f6", CultureInfo.InvariantCulture) + " " + this.Height.ToString("f6", CultureInfo.InvariantCulture);
        }
    }
}

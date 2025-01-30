using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using CrosshairWindow.Converters;
using System.Windows.Media.Imaging;

using Point     = System.Drawing.Point;
using Brush     = System.Drawing.Brush;
using Color     = System.Drawing.Color;
using Pen       = System.Drawing.Pen;
using Rectangle = System.Drawing.Rectangle;




namespace CrosshairWindow.Model
{

    /*

    class CrosshairSettings
    {

        private float _scale = 1F;
        private float _angle = 0F;

        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;
        public float RotationSpeed { get; set; } = 0F;
        public bool Visible { get; set; } = true;

        public CenterDot CenterDot { get; set; } = new();
        public Lines Lines { get; set; } = new();
        //public Outline Outlines { get; set; } = new Outline();


        public bool IsImage { get; set; } = false;
        public Bitmap? CrosshairBitmap { get; set; }


        public float Scale
        {
            get { return _scale; }
            set { _scale = Math.Max(0, value); }
        }
        public float Angle
        {
            get { return _angle; }
            set { _angle = value % 360; }
        }


        public CrosshairSettings()
        {
        }

        public CrosshairSettings(string identifier, string imageString = "")
        {
            IsImage = string.IsNullOrEmpty(imageString)  ? false : true;
            DeserializeSettings(identifier);
        }



        // Serialize the settings into a compact string format
        public string GetSettingsIdentifier()
        {
            var sb = new StringBuilder();

            // Crosshair basic properties
            sb.Append(Scale).Append(',');
            sb.Append(OffsetX).Append(',');
            sb.Append(OffsetY).Append(',');
            sb.Append(Angle).Append(',');
            sb.Append(RotationSpeed).Append(',');
            sb.Append(Visible ? "1" : "0").Append(',');

            // CenterDot settings
            sb.Append(CenterDot.Length).Append(',');
            sb.Append(CenterDot.Height).Append(',');
            sb.Append((int)CenterDot.Shape).Append(',');
            sb.Append(CenterDot.Color.ToArgb()).Append(','); // Serialize the color as ARGB integer
            sb.Append(CenterDot.Opacity).Append(',');
            sb.Append(CenterDot.Angle).Append(',');
            sb.Append(CenterDot.Outline.Thickness).Append(',');
            sb.Append(CenterDot.Outline.Color.ToArgb());
            sb.Append(CenterDot.Outline.Opacity).Append(',');
            sb.Append(CenterDot.Outline.Visible ? "1" : "0").Append(',');
            sb.Append(CenterDot.OffsetX).Append(',');
            sb.Append(CenterDot.OffsetY).Append(',');
            sb.Append(CenterDot.Visible ? "1" : "0").Append(',');

            // Lines settings
            sb.Append(Lines.Thickness).Append(',');
            sb.Append(Lines.Length).Append(',');
            sb.Append((int)Lines.Shape).Append(',');
            sb.Append(Lines.Color.ToArgb()).Append(','); // Serialize the color as ARGB integer
            sb.Append(Lines.Opacity).Append(',');
            sb.Append(Lines.GapX).Append(',');
            sb.Append(Lines.GapY).Append(',');
            sb.Append(Lines.Angle).Append(',');
            sb.Append(Lines.Outline.Thickness).Append(',');
            sb.Append(Lines.Outline.Color.ToArgb());
            sb.Append(Lines.Outline.Opacity).Append(',');
            sb.Append(Lines.Outline.Visible ? "1" : "0").Append(',');
            sb.Append(Lines.OffsetX).Append(',');
            sb.Append(Lines.OffsetY).Append(',');
            sb.Append(Lines.Visible ? "1" : "0").Append(',');
            sb.Append(string.Join("", Lines.IsDirectionMap.Values.Select(v => v ? "1" : "0"))).Append(',');



            // Base64 encode the resulting string for compactness
            string serializedString = sb.ToString();
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedString));

        }


        // Deserialize the settings from the compact string
        public static CrosshairSettings DeserializeSettings(string identifier)
        {

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(identifier));
            var values = decoded.Split(',');
            int i = 0;

            CrosshairSettings cs = new();

                // Crosshair basic properties
                cs.Scale = float.Parse(values[i++]);
                cs.OffsetX = int.Parse(values[i++]);
                cs.OffsetY = int.Parse(values[i++]);
                cs.Angle = float.Parse(values[i++]);
                cs.RotationSpeed = float.Parse(values[i++]);
                cs.Visible = values[i++] == "1";

                // CenterDot settings
                cs.CenterDot.Length = int.Parse(values[i++]);
                cs.CenterDot.Height = int.Parse(values[i++]);
                cs.CenterDot.Shape = (CenterDot.ShapeEnum)int.Parse(values[i++]);
                cs.CenterDot.Color = Color.FromArgb(int.Parse(values[i++]));
                cs.CenterDot.Opacity = float.Parse(values[i++]);
                cs.CenterDot.Angle = float.Parse(values[i++]);
                cs.CenterDot.Outline.Thickness = int.Parse(values[i++]);
                cs.CenterDot.Outline.Color = Color.FromArgb(int.Parse(values[i++]));
                cs.CenterDot.Outline.Opacity = float.Parse(values[i++]);
                cs.CenterDot.Outline.Visible = values[i++] == "1";
                cs.CenterDot.OffsetX = int.Parse(values[i++]);
                cs.CenterDot.OffsetY = int.Parse(values[i++]);
                cs.CenterDot.Visible = values[i++] == "1";

                // Lines settings
                cs.Lines.Length = int.Parse(values[i++]);
                cs.Lines.Thickness = int.Parse(values[i++]);
                cs.Lines.Shape = (Lines.ShapeEnum)int.Parse(values[i++]);
                cs.Lines.Color = Color.FromArgb(int.Parse(values[i++]));
                cs.Lines.Opacity = float.Parse(values[i++]);
                cs.Lines.Angle = float.Parse(values[i++]);
                cs.Lines.GapX = int.Parse(values[i++]);
                cs.Lines.GapY = int.Parse(values[i++]);
                cs.Lines.Outline.Thickness = int.Parse(values[i++]);
                cs.Lines.Outline.Color = Color.FromArgb(int.Parse(values[i++]));
                cs.Lines.Outline.Opacity = float.Parse(values[i++]);
                cs.Lines.Outline.Visible = values[i++] == "1";
                cs.Lines.OffsetX = int.Parse(values[i++]);
                cs.Lines.OffsetY = int.Parse(values[i++]);
                cs.Lines.Visible = values[i++] == "1";

                string directionMap = values[i++];
                var directions = Enum.GetValues(typeof(Lines.Direction)).Cast<Lines.Direction>().ToArray();

            for (int j = 0; j < directionMap.Length; ++j)
                cs.Lines.IsDirectionMap[directions[j]] = directionMap[j] == '1';


            return cs;

        }


        public void Draw(Graphics graphics)
        {
            // T R S

			// T
			graphics.TranslateTransform(OffsetX, OffsetY);
    		// R
	        //graphics.RotateTransform(45);
	        graphics.RotateTransform(Angle);
            // S
            graphics.ScaleTransform(Scale, Scale);

            if(IsImage)
            {
                if(CrosshairBitmap != null)
                    graphics.DrawImage(CrosshairBitmap, new Point(-(int)(CrosshairBitmap.Width/2F), -(int)(CrosshairBitmap.Height/2F) ));
            }
            else
            {
        	    CenterDot.Draw(graphics);
                Lines.Draw(graphics);
            }


			// S
			graphics.ScaleTransform(1F / Scale, 1F / Scale);
			// R
			graphics.RotateTransform(-Angle);
            // T
			graphics.TranslateTransform(-OffsetX, -OffsetY);

        }


    }

    internal class Outline
    {
        private int _thickness;
        private float _opacity = 1F;

        public int Thickness
        {
            get { return _thickness; }
            set { _thickness = Math.Max(0, value); }
        }
        public Color Color { get; set; } = Color.White;
        public float Opacity
        {
            get { return _opacity; }
            set { _opacity = Math.Clamp(value, 0, 1); }
        }

        public bool Visible { get; set; } = false;

    }

    internal class CenterDot
    {
        private int _length = 1;
        private int _height = 1;
        private float _opacity = 1F;
        private float _angle = 0F;


        public enum ShapeEnum
        {
            None,
            Triangle,
            Rectangle,
            Cross,
            Circle
        }

        public int Length
        {
            get { return _length; }
            set { _length = Math.Max(1, value); }
        }
        public int Height
        {
            get { return _height; }
            set { _height = Math.Max(1, value); }
        }

        public ShapeEnum Shape { get; set; } = ShapeEnum.None;

        public Color Color { get; set; } = Color.White;
        public float Opacity
        {
            get { return _opacity; }
            set { _opacity = Math.Clamp(value, 0, 1); }
        }
        public float Angle
        {
            get { return _angle; }
            set { _angle = value % 360; }
        }

        public Outline Outline { get; set; } = new();

        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;


        public bool Visible { get; set; } = false;



        public CenterDot()
        {
            Console.WriteLine("WOW");
        }


        public CenterDot(ShapeEnum shape, int width, int height, Color color, float opacity = 1.0F, float angle = 0.0F, Outline? outline = null, int offsetX = 0, int offsetY = 0, bool visibility = true)
        {
            Length = width;
            Height = height;
            Shape = shape;
            Color = color;
            Opacity = opacity;
            Angle = angle;
            Outline = outline ?? new();
            OffsetX = offsetX;
            OffsetY = offsetY;
            Visible = visibility;
        }


        public void Draw(Graphics graphics)
        {
            if (Shape == ShapeEnum.None || Visible == false)
                return;

            Color colorOpacity = Color.FromArgb((int)(Opacity * 255), Outline.Color);
            var outlinePen = new Pen(colorOpacity, Outline.Thickness);
            var brush = new SolidBrush(Color);


            switch (Shape)
            {
                case ShapeEnum.Triangle:
                    {
                        //Origin = centriod of equilateral triangle
                        //centriod to bottom Y: y -= tan(30) * width / 2
                        //centriod to top    Y: y += width / ( 3^(1/2) )
                        // TopY = 2 * BottomY

                        float topY = Length / (float)Math.Sqrt(3.0F);
                        float bottomY = 0.5F * topY;

                        var points = new List<Point>
                    {
                        new(-(int)(Length / 2.0F),  -(int)bottomY),
                        new( (int)(Length / 2.0F),  -(int)bottomY),
                        new(                   0,   (int)topY)
                    };


                        foreach (var point in points)
                            point.Offset(OffsetX, OffsetY);


                        graphics.FillPolygon(brush, points.ToArray());

                        if (Outline.Visible)
                            graphics.DrawPolygon(outlinePen, points.ToArray());


                    }
                    break;
                case ShapeEnum.Rectangle:
                    {
                        var rectangle = new Rectangle((int)(-Length / 2.0F), 0, Length, Height);

                        rectangle.Offset(OffsetX, OffsetY);


                        graphics.FillRectangle(brush, rectangle);

                        if (Outline.Visible)
                            graphics.DrawRectangle(outlinePen, rectangle);

                    }
                    break;
                case ShapeEnum.Cross:
                    {

                        var rectangles = new List<Rectangle>()
                    {
                        new((int)(-Length  / 2.0F), (int)(-Height / 2.0F), Length, Height),
                        new((int)(-Height / 2.0F), (int)(-Length  / 2.0F), Height, Length)
                    };

                        foreach (var rectangle in rectangles)
                            rectangle.Offset(OffsetX, OffsetY);



                        graphics.FillRectangles(brush, rectangles.ToArray());

                        if (Outline.Visible)
                            graphics.DrawRectangles(outlinePen, rectangles.ToArray());

                    }
                    break;
                case ShapeEnum.Circle:
                    {
                        var rectangleElipse = new Rectangle((int)(-Length / 2.0F), (int)(-Height / 2.0F), Length, Height);

                        rectangleElipse.Offset(OffsetX, OffsetY);


                        graphics.FillEllipse(brush, rectangleElipse);

                        if (Outline.Visible)
                            graphics.DrawEllipse(outlinePen, rectangleElipse);

                    }
                    break;
            }

        }



    }

    internal class Lines
    {
        public enum ShapeEnum
        {
            None,
            Triangle,
            Rectangle,
            Circle
        }

        public enum Direction
        {
            Right,
            RightDown,
            Down,
            LeftDown,
            Left,
            LeftUp,
            Up,
            RightUp
        }

        private int _thickness = 3;
        private int _length = 7;
        private float _opacity = 1F;
        private float _angle = 0F;
        private int _gapX = 0;
        private int _gapY = 0;


        public int Thickness
        {
            get { return _thickness; }
            set { _thickness = Math.Max(0, value); }
        }

        public int Length
        {
            get { return _length; }
            set { _length = Math.Max(0, value); }
        }
        public ShapeEnum Shape { get; set; } = ShapeEnum.Rectangle;

        public Color Color { get; set; } = Color.White;

        public float Opacity
        {
            get { return _opacity; }
            set { _opacity = Math.Clamp(value, 0, 1); }
        }

        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value % 360;
            }
        }
        public int GapX
        {
            get { return _gapX; }
            set { _gapX = Math.Max(0, value); }
        }

        public int GapY
        {
            get { return _gapY; }
            set { _gapY = Math.Max(0, value); }
        }

        public Outline Outline { get; set; } = new();

        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;

        public bool Visible { get; set; } = false;




        public Dictionary<Direction, bool> IsDirectionMap { get; private set; } = [];


        public Lines()
        {
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                IsDirectionMap[direction] = false;
        }


        public void ToggleDirection(Direction direction)
        {
            IsDirectionMap[direction] = !IsDirectionMap[direction];
        }

        public void Draw(Graphics graphics)
        {
            if (Shape == ShapeEnum.None || Visible == false)
                return;

            //Right,
            //RightDown,
            //Down,
            //LeftDown,
            //Left,
            //LeftUp,
            //Up,
            //RightUp

            // temp
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                IsDirectionMap[direction] = true;


            Color colorOpacity = Color.FromArgb((int)(Opacity * 255), Outline.Color);
            var outlinePen = new Pen(colorOpacity, Outline.Thickness);
            //var linesBrush = new SolidBrush(colorOpacity);


            // T R S, T
            // no S bc we dont need / want scaling for this

            // R1, set angle, rotate and thand translate, so that there is not orbiting but just pivoting
            graphics.RotateTransform(-Angle);

            // T1
            graphics.TranslateTransform(OffsetX, OffsetY);

            //const int angleIncrement = 0;
            const int angleIncrement = 45;

            int i = -1;

            foreach (var directionPair in IsDirectionMap)
            {
                ++i;
                float currentAngle = i * angleIncrement;

                if (directionPair.Value == false)
                    continue;

                // R
                graphics.RotateTransform(currentAngle);

                // T2,1 Get To Center
                graphics.TranslateTransform(Length / 2, 0);



                // T2,2 Create gap
                // GapX is used for X and Y
                graphics.TranslateTransform(GapX, 0);


                switch (Shape)
                {
                    case ShapeEnum.Triangle:
                        {
                            Brush brush = new SolidBrush(Color);

                            //Origin = centriod of equilateral triangle
                            //centriod to bottom Y: y -= tan(30) * width / 2
                            //centriod to top    Y: y += width / ( 3^(1/2) )
                            // TopY = 2 * BottomY

                            float topY = Length / (float)Math.Sqrt(3.0F);
                            float bottomY = 0.5F * topY;

                            var points = new List<Point>
                            {
                                new(-Length / 2, -(int)bottomY),
                                new( Length / 2, -(int)bottomY),
                                new(         0,  (int)topY)
                            };


                            graphics.FillPolygon(brush, points.ToArray());

                            if (Outline.Visible)
                                graphics.DrawPolygon(outlinePen, points.ToArray());

                        }
                        break;
                    case ShapeEnum.Rectangle:
                        {
                            Brush brush = new SolidBrush(Color);
                            var rectangle = new Rectangle(-Length / 2, -Thickness / 2, Length, Thickness);


                            graphics.FillRectangle(brush, rectangle);

                            if (Outline.Visible)
                                graphics.DrawRectangle(outlinePen, rectangle);

                        }
                        break;
                    case ShapeEnum.Circle:
                        {
                            Brush brush = new SolidBrush(Color);
                            var rectangleElipse = new Rectangle(-Length / 2, -Thickness / 2, Length, Thickness);


                            graphics.FillEllipse(brush, rectangleElipse);

                            if (Outline.Visible)
                                graphics.DrawEllipse(outlinePen, rectangleElipse);

                        }
                        break;
                }


                // T2, 2
                graphics.TranslateTransform(-GapX, 0);

                // T2, 1
                graphics.TranslateTransform(-Length / 2, 0);

                // R
                graphics.RotateTransform(-currentAngle);
            }

            // T1
            graphics.TranslateTransform(-OffsetX, -OffsetY);

            // R1
            graphics.RotateTransform(Angle);

        }


    }


*/

}

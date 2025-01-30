﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

using CrosshairWindow.Converters;
using Point = System.Drawing.Point;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Pen   = System.Drawing.Pen;
using Rectangle = System.Drawing.Rectangle;
using System.Diagnostics.Eventing.Reader;
using System.ComponentModel.DataAnnotations;

// BudgetArms


namespace CrosshairWindow.Model
{

    class Crosshair : ObservableObject
    {
        //  Folder/File locations
        private static string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BudgetArmsCrosshairOverlay");
        private static string CrosshairFile = Path.Combine(AppDataFolder, "Crosshair.png");
        private static string SettingsFile = Path.Combine(AppDataFolder, "Settings.txt");

        public string Name { get; set; } = "DefaultCrosshair";

        //private readonly CrosshairSettings _crosshairSettings = new CrosshairSettings();

        // Crosshair Settings
        private float _scale    = 1F;
        private float _opacity  = 1F;
        private float _angle    = 0F;
        private string _imageUrl = "";
        private Bitmap? _crosshairBitmap = null;


        public float Scale
        {
            get { return _scale; }
            set { _scale = Math.Max(0, value); }
        }
        public float Opacity
        {
            get { return _opacity; }
            set { _opacity = Math.Clamp(value, 0, 1); }
        }
        public float Angle
        {
            get { return _angle; }
            set { _angle = value % 360; OnPropertyChanged(); }
        }
        public float RotationSpeed { get; set; } = 0F;

        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;
        public bool Visible { get; set; } = true;
        public bool UsesImage { get; set; } = false; 

        // no image:
        public CenterDot CenterDot { get; set; } = new();
        public Lines Lines { get; set; } = new();

        // The image the user selected
        public Bitmap? CrosshairBitmap
        {
            get
            {
                return _crosshairBitmap;
            }
            set
            {
                _crosshairBitmap = value;
                Console.WriteLine("CROSSHAIRBITMAP SETTER");
                string test = ImageToBase64Converter.BitmapToBase64String(value);
                OnPropertyChanged();
            }
        }

        // Image shown on screen
        public string ImageUrl
        {
            get
            {
                return _imageUrl;
            }
            set
            {
                _imageUrl = value;
                OnPropertyChanged();
            }
        }


        // load settings.txt and crosshair.png
        public Crosshair() :
            this("CrosshairName", GetStoredSettings(), GetStoredCrosshair())
        {
        }

        public Crosshair(string name, string crosshairSettings, string crosshairImage = "") :
            this(name, crosshairSettings, StringToImageConverter.Base64StringToBitmap(crosshairImage))
        {
            Console.WriteLine($"Crosshair Created with string");

        }

        public Crosshair(string name, string crosshairSettings, Bitmap? crosshairBitmap)
        {
            Console.WriteLine($"Crosshair {name} Created!");

            Name = name;
            CrosshairBitmap = crosshairBitmap;
            SetSettings(crosshairSettings);
            Draw();
        }



        public void Update(float elapsedSec)
        {
            if (RotationSpeed == 0F)
                return;

            Angle += RotationSpeed * elapsedSec;

            Draw();

        }

        private void Draw()
        {
            // Logic to render CrosshairSettings to a bitmap image
            const int width  = 1000;
            const int height = 1000;

            using (var bitmap = new Bitmap(width, height))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // Make everything (0, 0) center
                    graphics.TranslateTransform(width / 2, height / 2);

                    // Draw the crosshair shapes (center dot, lines, etc.) on the bitmap
                    // T R S

                    // T
                    graphics.TranslateTransform(OffsetX, OffsetY);
                    // R
                    graphics.RotateTransform(Angle);
                    // S
                    graphics.ScaleTransform(Scale, Scale);

                    if (UsesImage)
                    {
                        //if (CrosshairBitmap != null)
                            //graphics.DrawImage(CrosshairBitmap, new Point(-(int)(CrosshairBitmap.Width / 2F), -(int)(CrosshairBitmap.Height / 2F)));
                    }
                    else
                    {
                        //CenterDot.Draw(graphics);
                        //Lines.Draw(graphics);
                    }

                    Color color = Color.Yellow;
                    Brush brush = new SolidBrush(color);

                    //Origin = centriod of equilateral triangle
                    //centriod to bottom Y: y -= tan(30) * width / 2
                    //centriod to top    Y: y += width / ( 3^(1/2) )
                    // TopY = 2 * BottomY

                    float Length = 100;
                    float topY = Length / (float)Math.Sqrt(3.0F);
                    float bottomY = 0.5F * topY;

                    var points = new List<Point>
                            {
                                new(-(int)(Length / 2F), -(int)bottomY),
                                new( (int)(Length / 2F), -(int)bottomY),
                                new(         0,  (int)topY)
                            };


                    graphics.FillPolygon(brush, points.ToArray());


                    // S
                    graphics.ScaleTransform(1F / Scale, 1F / Scale);
                    // R
                    graphics.RotateTransform(-Angle);
                    // T
                    graphics.TranslateTransform(-OffsetX, -OffsetY);




                    // Save bitmap to a MemoryStream, then convert to Base64 or save it as a file
                    using (var memoryStream = new MemoryStream())
                    {
                        bitmap.Save(memoryStream, ImageFormat.Png);
                        var base64String = Convert.ToBase64String(memoryStream.GetBuffer());

                        // Use the base64String as the ImageUrl (this can be used as an ImageSource in XAML)
                        ImageUrl = $"data:image/png;base64,{base64String}";
                    }

                }

            }
        }

        // Get Stored settings
        public static string GetStoredSettings()
        {
            //if (File.Exists(SettingsFile) && new FileInfo(SettingsFile).Length != 0)
            //{
            //    // loads settings
            //    string settings = File.ReadAllText(SettingsFile);

            //    return "FUCK";
            //}
            //else
            {
                // Creates file
                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    // set settings in file
                    string settings = Crosshair.GetDefaultSettings();
                    writer.Write(settings);

                    return settings;
                }

            }
        }

        // Store Settings with setting string
        public static void StoreSettings(string settings)
        {
            // Sets settings in file
            using (StreamWriter writer = new StreamWriter(SettingsFile))
            {
                writer.Write(settings);
            }

        }


        // Get Stored Crosshair
        public static Bitmap GetStoredCrosshair()
        {
            if (File.Exists(CrosshairFile))
                return new Bitmap(CrosshairFile);
            else
                return StoreEmptyCrosshair();
        }

        // Store Crosshair with imagePath
        public static void StoreCrosshair(string imagePath)
        {

            if (!File.Exists(imagePath))
            {
                Console.WriteLine("Error in function: " + System.Reflection.MethodBase.GetCurrentMethod()?.Name);
                Console.WriteLine("Imagepath does not exist");
                return;
            }

            using (var bitmap = new Bitmap(imagePath))
            {
                bitmap.Save(CrosshairFile, ImageFormat.Png);
            }


            /*
             
            // This was done to prevent locking
            // Read the image into a MemoryStream to avoid locking the file
            byte[] imageBytes = File.ReadAllBytes(imagePath);

            using (var memoryStream = new MemoryStream(imageBytes))
            {
                // Load bitmap from the stream
                using (var newBitmap = new Bitmap(memoryStream))
                {
                    // Ensure format compatibility
                    using (var savedBitmap = new Bitmap(newBitmap))
                    {
                        try
                        {
                            savedBitmap.Save(CrosshairFile, ImageFormat.Png);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception: ", ex.Message);
                        }
                    }
                }
            }

            */


        }

        // Store Crosshair with image (Bitmap)
        public static void StoreCrosshair(Bitmap image)
        {
            // Save Crossahir
            image.Save(CrosshairFile, ImageFormat.Png); 
        }

        // Store Empty Crosshair
        private static Bitmap StoreEmptyCrosshair()
        {
            int width = 1000;
            int height = 1000;

            using (var bitmap = new Bitmap(width, height))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    Brush brush = new SolidBrush(Color.Red);
                   
                    graphics.Clear(Color.Transparent); // Transparent background

                    // Make everything (0, 0) center
                    graphics.TranslateTransform(width / 2, height / 2);

                    graphics.DrawRectangle(new System.Drawing.Pen(brush, 2), new Rectangle(-40, -40, 80, 80));

                }

                bitmap.Save(CrosshairFile, ImageFormat.Png);
                return bitmap;
               
            }

        }

        // CURSED
        public static string GetDefaultSettings()
        {
            string settings = "1,0,0,0,0,1" + "," 
                            + "5,5,0,1,0,0,0,1,0,1,1,0,0,0,1,1,0,0,1" + "," 
                            + "4,10,0,1,0,0,0,1,3,3,0,2,1,0,0,0,1,1,0,0,1,0,0,0,0,0,0";

            var sb = new StringBuilder();

            // Crosshair basic properties
            sb.Append(1F).Append(',');  // Scale
            sb.Append(1F).Append(',');  // Opacity
            sb.Append(0F).Append(',');  // Angle
            sb.Append(0F).Append(',');  // RotationSpeed
            sb.Append(0).Append(',');  // OffsetX
            sb.Append(0).Append(',');  // OffsetY
            sb.Append('1').Append(','); // Visible
            sb.Append('0').Append(','); // UsesImage

            CenterDot   centerDot = new();
            Lines       lines     = new();
            centerDot.Color = Color.Blue;
            centerDot.Outline.Thickness = 2;

            // CenterDot settings
            sb.Append(centerDot.Length).Append(',');
            sb.Append(centerDot.Height).Append(',');
            sb.Append((int)centerDot.Shape).Append(',');
            sb.Append(centerDot.Color.ToArgb()).Append(','); // Serialize the color as ARGB integer
            sb.Append(centerDot.Opacity).Append(',');
            sb.Append(centerDot.Angle).Append(',');
            sb.Append(centerDot.OffsetX).Append(',');
            sb.Append(centerDot.OffsetY).Append(',');
            sb.Append(centerDot.Visible ? "1" : "0").Append(',');
            sb.Append(centerDot.Outline.Thickness).Append(',');
            sb.Append(centerDot.Outline.Color.ToArgb()).Append(',');
            sb.Append(centerDot.Outline.Opacity).Append(',');
            sb.Append(centerDot.Outline.Visible ? "1" : "0").Append(',');

            // 10,4,0,1,0,0,0,1,3,3,0,2,1,0,0,0,1,1,0,0,1,0,0,0,0,0,0
            // Lines settings
            sb.Append(lines.Length).Append(',');
            sb.Append(lines.Thickness).Append(',');
            sb.Append((int)lines.Shape).Append(',');
            sb.Append(lines.Color.ToArgb()).Append(','); // Serialize the color as ARGB integer
            sb.Append(lines.Opacity).Append(',');
            sb.Append(lines.Angle).Append(',');
            sb.Append(lines.GapX).Append(',');
            sb.Append(lines.GapY).Append(',');
            sb.Append(lines.OffsetX).Append(',');
            sb.Append(lines.OffsetY).Append(',');
            sb.Append(lines.Visible ? "1" : "0").Append(',');
            sb.Append(string.Join("", lines.DirectionMap.Values.Select(v => v ? "1" : "0"))).Append(',');
            sb.Append(lines.Outline.Thickness).Append(',');
            sb.Append(lines.Outline.Color.ToArgb()).Append(',');
            sb.Append(lines.Outline.Opacity).Append(',');
            sb.Append(lines.Outline.Visible ? "1" : "0"); // end line doesn't need append

            string serializedString = sb.ToString();
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedString));

        }

        // Serialize the settings into a compact string format
        public string GetSettings()
        {
            var sb = new StringBuilder();

            // 1,1,0,0.0,0,0,1,0
            // Crosshair basic properties
            sb.Append(Scale).Append(',');
            sb.Append(Opacity).Append(',');
            sb.Append(Angle).Append(',');
            sb.Append(RotationSpeed).Append(',');
            sb.Append(OffsetX).Append(',');
            sb.Append(OffsetY).Append(',');
            sb.Append(Visible ? "1" : "0").Append(',');
            sb.Append(UsesImage ? "1" : "0").Append(',');

            // 5,5,0,1,0,0,0,1,0,1,1,0,0,0,1,1,0,0,1
            // CenterDot settings
            sb.Append(CenterDot.Length).Append(',');
            sb.Append(CenterDot.Height).Append(',');
            sb.Append((int)CenterDot.Shape).Append(',');
            sb.Append(CenterDot.Color.ToArgb()).Append(','); // Serialize the color as ARGB integer
            sb.Append(CenterDot.Opacity).Append(',');
            sb.Append(CenterDot.Angle).Append(',');
            sb.Append(CenterDot.OffsetX).Append(',');
            sb.Append(CenterDot.OffsetY).Append(',');
            sb.Append(CenterDot.Visible ? "1" : "0").Append(',');
            sb.Append(CenterDot.Outline.Thickness).Append(',');
            sb.Append(CenterDot.Outline.Color.ToArgb()).Append(',');
            sb.Append(CenterDot.Outline.Opacity).Append(',');
            sb.Append(CenterDot.Outline.Visible ? "1" : "0").Append(',');

            // 10,4,0,1,0,0,0,1,3,3,0,2,1,0,0,0,1,1,0,0,1,0,0,0,0,0,0
            // Lines settings
            sb.Append(Lines.Length).Append(',');
            sb.Append(Lines.Thickness).Append(',');
            sb.Append((int)Lines.Shape).Append(',');
            sb.Append(Lines.Color.ToArgb()).Append(','); // Serialize the color as ARGB integer
            sb.Append(Lines.Opacity).Append(',');
            sb.Append(Lines.Angle).Append(',');
            sb.Append(Lines.GapX).Append(',');
            sb.Append(Lines.GapY).Append(',');
            sb.Append(Lines.OffsetX).Append(',');
            sb.Append(Lines.OffsetY).Append(',');
            sb.Append(Lines.Visible ? "1" : "0").Append(',');
            sb.Append(string.Join("", Lines.DirectionMap.Values.Select(v => v ? "1" : "0"))).Append(',');
            sb.Append(Lines.Outline.Thickness).Append(',');
            sb.Append(Lines.Outline.Color.ToArgb()).Append(',');
            sb.Append(Lines.Outline.Opacity).Append(',');
            sb.Append(Lines.Outline.Visible ? "1" : "0"); // no append needed at end


            // Base64 encode the resulting string for compactness
            string serializedString = sb.ToString();
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedString));

        }

        // Deserialize the settings from the compact string
        public void SetSettings(string identifier)
        {

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(identifier));
            var values = decoded.Split(',');
            int i = 0;

            // Crosshair basic properties
            Scale       = float.Parse(values[i++]);
            Opacity     = float.Parse(values[i++]);
            Angle       = float.Parse(values[i++]);
            RotationSpeed = float.Parse(values[i++]);
            OffsetX     = int.Parse(values[i++]);
            OffsetY     = int.Parse(values[i++]);
            Visible     = values[i++] == "1";
            UsesImage   = values[i++] == "1";

            // CenterDot settings
            CenterDot.Length    = int.Parse(values[i++]);
            CenterDot.Height    = int.Parse(values[i++]);
            CenterDot.Shape     = (CenterDot.ShapeEnum)int.Parse(values[i++]);
            CenterDot.Color     = Color.FromArgb(int.Parse(values[i++]));
            CenterDot.Opacity   = float.Parse(values[i++]);
            CenterDot.Angle     = float.Parse(values[i++]);
            CenterDot.OffsetX   = int.Parse(values[i++]);
            CenterDot.OffsetY   = int.Parse(values[i++]);
            CenterDot.Visible   = values[i++] == "1";
            CenterDot.Outline.Thickness = int.Parse(values[i++]);
            CenterDot.Outline.Color     = Color.FromArgb(int.Parse(values[i++]));
            CenterDot.Outline.Opacity   = float.Parse(values[i++]);
            CenterDot.Outline.Visible   = values[i++] == "1";

            // Lines settings
            Lines.Length    = int.Parse(values[i++]);
            Lines.Thickness = int.Parse(values[i++]);
            Lines.Shape     = (Lines.ShapeEnum)int.Parse(values[i++]);
            Lines.Color     = Color.FromArgb(int.Parse(values[i++]));
            Lines.Opacity   = float.Parse(values[i++]);
            Lines.Angle     = float.Parse(values[i++]);
            Lines.GapX      = int.Parse(values[i++]);
            Lines.GapY      = int.Parse(values[i++]);
            Lines.OffsetX   = int.Parse(values[i++]);
            Lines.OffsetY   = int.Parse(values[i++]);
            Lines.Visible   = values[i++] == "1";
            // should look like 00000000 ( 8 bools )
            string directionMap = values[i++];
            var directions = Enum.GetValues(typeof(Lines.Direction)).Cast<Lines.Direction>().ToArray();

            for (int j = 0; j < directionMap.Length; ++j)
                Lines.DirectionMap[directions[j]] = (directionMap[j] == '1');

            //Lines.IsDirectionMap.Keys.Select(key => )
            Lines.Outline.Thickness = int.Parse(values[i++]);
            Lines.Outline.Color     = Color.FromArgb(int.Parse(values[i++]));
            Lines.Outline.Opacity   = float.Parse(values[i++]);
            Lines.Outline.Visible   = values[i++] == "1";


            Console.WriteLine("Loaded Settings");

        }


        // Bitmap is directly accessible with the CrosshairBitmap property

    }



    internal class Outline
    {
        private int _thickness = 1;
        private float _opacity = 1F;

        public int Thickness
        {
            get { return _thickness; }
            set { _thickness = Math.Max(0, value); }
        }
        public Color Color { get; set; } = Color.Black;
        public float Opacity
        {
            get { return _opacity; }
            set { _opacity = Math.Clamp(value, 0, 1); }
        }

        public bool Visible { get; set; } = true;

    }

    internal class CenterDot
    {
        private int _length = 1;
        private int _height = 1;
        private float _opacity = 1F;
        private float _angle = 0F;
        public enum ShapeEnum
        {
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

        public ShapeEnum Shape { get; set; } = ShapeEnum.Circle;

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

        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;
        public bool Visible { get; set; } = true;
        public Outline Outline { get; set; } = new();


        public CenterDot()
        {
        }

        public CenterDot(ShapeEnum shape, int width, int height, Color color, float opacity = 1.0F, float angle = 0.0F, Outline? outline = null, int offsetX = 0, int offsetY = 0, bool visibility = true)
        {
            Length  = width;
            Height  = height;
            Shape   = shape;
            Color   = color;
            Opacity = opacity;
            Angle   = angle;
            Outline = outline ?? new();
            OffsetX = offsetX;
            OffsetY = offsetY;
            Visible = visibility;
        }


        public void Draw(Graphics graphics)
        {
            if (Opacity == 0F || Visible == false)
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

        private int _length = 7;
        private int _thickness = 3;
        private float _opacity = 1F;
        private float _angle = 0F;
        private int _gapX = 0;
        private int _gapY = 0;


        public int Length
        {
            get { return _length; }
            set { _length = Math.Max(0, value); }
        }
        public int Thickness
        {
            get { return _thickness; }
            set { _thickness = Math.Max(0, value); }
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

        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;

        public bool Visible { get; set; } = true;
        public Dictionary<Direction, bool> DirectionMap { get; } = new();
        public Outline Outline { get; set; } = new();






        public Lines()
        {
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                DirectionMap[direction] = true;
        }


        public void SetDirection(Direction direction, bool isEnabled)
        {
            DirectionMap[direction] = isEnabled;
        }
 
        public void ToggleDirection(Direction direction)
        {
            DirectionMap[direction] = !DirectionMap[direction];
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
                DirectionMap[direction] = true;


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

            foreach (var directionPair in DirectionMap)
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



}

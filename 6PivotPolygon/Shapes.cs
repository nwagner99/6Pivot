using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Drawing;

namespace _6PivotPolygon.Controllers
    {
    /// <summary>
    /// Important:  if this list changes, change the measurement type switch statement in Measurement.Parse()
    /// </summary>
    internal enum eShapeType2d
        {
        None, Circle, Oval, Triangle, Square, Rectangle, Parallelogram, Pentagon, Hexagon, Heptagon, Octagon, Ellipse
        }

    /// <summary>
    /// Important:  if this list changes, change the measurement type switch statement in Measurement.Parse()
    /// </summary>
    internal enum eShapeType3d
        {
        None, Cube
        }

    /// <summary>
    /// A list of parameter types for drawing various shapes.
    /// </summary>
    internal enum eParamType
        {
        Radius, Height, Width, Side, Offset, RadiusX, RadiusY, OriginX, OriginY, Rotation, Depth
        }

    enum eTriangleType
        {
        Equilateral, Isosceles
        }

    internal abstract class Shape
        {
        public Point[] points;
        public bool Is3D = false;

        abstract public JSONShape Emit(ref string errMsg);
        }

    internal class Square : Shape
        {
        public int Height, Depth;

        public Square(int height, int depth)
            {
            if (height <= 0) throw new ArgumentException("height cannot be negative or zero");
            Height = height;
            Depth = depth;
            }

        public override JSONShape Emit(ref string errMsg)
            {
            JSONShape retval = new Controllers.JSONShape();
            retval.type = "square";
            retval.height = Height;
            List<Point> temp = new List<Point>() { new Point(0, 0), new Point(Height, 0), new Point(Height, Height), new Point(0, Height) };
            retval.points = temp.ToArray();
            retval.status = true;
            retval.is3d = Is3D;
            retval.depth = Depth;
            return retval;
            }
        }

    internal class Rectangle : Shape
        {
        public int Height, Width;

        public Rectangle(int height, int width)
            {
            if (height <= 0 || width <= 0) throw new ArgumentNullException("The height and width cannot be negative or zero.");
            Height = height;
            Width = width;
            }

        public override JSONShape Emit(ref string errMsg)
            {
            JSONShape retval = new Controllers.JSONShape();
            retval.type = "rectangle";
            retval.height = Height;
            retval.width = Width;
            retval.points = new Point[4] { new Point(0, 0), new Point(Width, 0), new Point(Width, Height), new Point(0, Height) };
            retval.status = true;
            retval.is3d = Is3D;
            return retval;
            }
        }

    /// <summary>
    /// Covers the n-sided polygons, currently from 5 to 9.
    /// </summary>
    internal class Polygon : Shape
        {
        int Radius, Sides = 0;
        Point origin = new Point(0, 0);

        public Polygon(int radius, int sides)
            {
            if (radius <= 0)
                {
                throw new ArgumentException("The radius cannot be negative or zero.");
                }
            if (sides < 5 || sides > 8)
                {
                throw new ArgumentException("The number of sides must be between 5 and 8.");
                }
            Radius = radius;
            Sides = sides;
            }

        public override JSONShape Emit(ref string errMsg)
            {
            const double twoPi = Math.PI * 2.0;

            List<Point> _temp = new List<Point>();

            JSONShape retval = new Controllers.JSONShape();
            retval.type = string.Format("{0} sided polygon", Sides);

            // Set the origin of the pentagon in the middle of the canvas.
            origin.X = Properties.Settings.Default.MaxImageSize / 2;
            origin.Y = origin.X;

            for (int i = 0; i < Sides; i++)
                {
                int x1 = origin.X + (int)Math.Round((Radius * Math.Cos(twoPi * i / Sides)));
                int y1 = origin.Y + (int)Math.Round((Radius * Math.Sin(twoPi * i / Sides)));
                _temp.Add(new Point(x1, y1));
                }
            retval.points = _temp.ToArray();
            retval.radius = Radius;
            retval.sides = Sides;
            retval.status = true;
            retval.is3d = Is3D;

            return retval;
            }
        }

    internal class Triangle : Shape
        {
        public int Height, Width;
        public eTriangleType TriangleType = eTriangleType.Equilateral;

        public Triangle(int height, int width)
            {
            if (height <= 0 || width <= 0) throw new ArgumentException("The height and width cannot be negative or zero.");
            Height = height;
            Width = width;
            TriangleType = (Height == Width) ? eTriangleType.Equilateral : eTriangleType.Isosceles;
            }

        public override JSONShape Emit(ref string errMsg)
            {
            JSONShape retval = new Controllers.JSONShape();
            retval.type = string.Format("{0} triangle", TriangleType.ToString());
            retval.height = Height;
            retval.width = Width;
            retval.points = new Point[] { new Point(Width / 2, 0), new Point(Width, Height), new Point(0, Height) };
            retval.status = true;
            retval.is3d = Is3D;
            return retval;
            }
        }

    internal class Parallelogram : Shape
        {
        public int Height, Width;
        public int Offset = 0;
        public double Angle = Math.PI / 2.0;

        public Parallelogram(int height, int width, int offset)
            {
            if (height <= 0 || width <= 0 || offset <= 0) throw new ArgumentException("The parameters cannot be negative or zero.");
            Height = height;
            Width = width;
            Offset = offset;
            }

        public override JSONShape Emit(ref string errMsg)
            {
            JSONShape retval = new JSONShape();
            retval.type = "parallelogram";
            retval.height = Height;
            retval.width = Width;
            retval.offset = Offset;
            retval.points = new Point[] { new Point(Offset, 0), new Point(Offset + Width, 0), new Point(Width, Height), new Point(0, Height) };
            retval.status = true;
            retval.is3d = Is3D;

            return retval;
            }
        }

    internal class Ellipse : Shape
        {
        public int OriginX, OriginY, RadiusX, RadiusY;
        public double Rotation = 0;

        public Ellipse(int originX, int originY, int radiusX, int radiusY, double rotation)
            {
            if (originX <= 0 || originY <= 0 || radiusX <= 0 || radiusY <= 0) throw new ArgumentException("Ellipse parameters cannot be negative or zero.");
            OriginX = originX;
            OriginY = originY;
            RadiusX = radiusX;
            RadiusY = radiusY;
            Rotation = rotation * (Math.PI / 180.0);
            }

        public override JSONShape Emit(ref string errMsg)
            {
            JSONShape retval = new Controllers.JSONShape();
            retval.type = "ellipse";
            retval.radiusX = RadiusX;
            retval.radiusY = RadiusY;
            retval.originX = OriginX;
            retval.originY = OriginY;
            retval.rotation = Rotation;
            retval.status = true;
            retval.is3d = Is3D;
            return retval;
            }
        }
    internal class Circle : Shape
        {
        public int Radius;

        public Circle(int radius)
            {
            if (radius <= 0) throw new ArgumentException("The radius cannot be negative or zero.");
            Radius = radius;
            }

        public override JSONShape Emit(ref string errMsg)
            {
            JSONShape retval = new Controllers.JSONShape() { type = "circle", radius = Radius };
            retval.status = true;
            retval.is3d = Is3D;
            return retval;
            }
        }

    /// <summary>
    /// The object returned back from the polygon controller.
    /// </summary>
    public class JSONShape
        {
        public string type = string.Empty;
        public bool is3d = false;

        /// <summary>
        /// If false, then the polygon could not be rendered, and the error message
        /// should contain the reason why.
        /// </summary>
        public bool status = false;

        /// <summary>
        /// The reason the shape couldn't be rendered.
        /// </summary>  
        public string errorMessage = string.Empty;

        public int radius, sides, depth;
        public int height, width, offset;
        public Point[] points;

        // Specific to an ellipse
        public int radiusX, radiusY, originX, originY;
        public double rotation = 0;

        public string description = string.Empty;
        }

    /// <summary>
    /// Class used to parse the input string into a codified shape object.
    /// </summary>
    internal class ShapeRequest
        {
        private Shape _shape;

        public eShapeType2d ShapeType = eShapeType2d.None;
        public List<Measurement> Measurements = new List<Measurement>();
        public bool Is3D = false;

        private static List<string> shapeNames = new List<string>();
        private static List<string> measurementNames = new List<string>();
        private static List<string> triTypes = new List<string>();

        public ShapeRequest()
            {

            }

        public ShapeRequest(string shType, bool is3d)
            : base()
            {
            Is3D = is3d;

            switch (shType)
                {
                case "triangle":
                    ShapeType = eShapeType2d.Triangle;
                    break;
                case "square":
                    ShapeType = eShapeType2d.Square;
                    break;
                case "rectangle":
                    ShapeType = eShapeType2d.Rectangle;
                    break;
                case "parallelogram":
                    ShapeType = eShapeType2d.Parallelogram;
                    break;
                case "pentagon":
                    ShapeType = eShapeType2d.Pentagon;
                    break;
                case "hexagon":
                    ShapeType = eShapeType2d.Hexagon;
                    break;
                case "heptagon":
                    ShapeType = eShapeType2d.Heptagon;
                    break;
                case "octagon":
                    ShapeType = eShapeType2d.Octagon;
                    break;
                case "circle":
                    ShapeType = eShapeType2d.Circle;
                    break;
                case "oval":
                    ShapeType = eShapeType2d.Oval;
                    break;
                case "ellipse":
                    ShapeType = eShapeType2d.Ellipse;
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid shape type ({0})", shType));
                }
            }
        static ShapeRequest()
            {
            // Build a list of names from the enumerated types.
            foreach (var name in Enum.GetNames(typeof(eShapeType2d))) shapeNames.Add(name.ToLower());
            foreach (var m in Enum.GetNames(typeof(eParamType))) measurementNames.Add(m.ToLower());
            foreach (var t in Enum.GetNames(typeof(eTriangleType))) triTypes.Add(t.ToLower());
            }

        /// <summary>
        /// Parse the request string into a shape request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="errMsg"></param>
        /// <returns>A shape request or null on error.</returns>
        public static ShapeRequest Parse(string request, ref string errMsg)
            {
            ShapeRequest retval = null;
            bool is3d = false;

            // First token is the request
            string[] words = request.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Use this to check for 3d
            List<string> wordList = words.ToList();

            string shape = string.Empty;
            string triangleType = string.Empty;
            List<Measurement> measurements = new List<Measurement>();

            // First, check if we are rendering a 3d shape - wordList[2] = 3d
            // If we are set the flag, remove it and move on.
            if (wordList.Count > 2)
                {
                if (wordList[2] == "3d")
                    {
                    is3d = true;
                    wordList.RemoveAt(2);
                    words = wordList.ToArray();
                    }
                else if (wordList[2] == "2d")
                    {
                    is3d = false;
                    wordList.RemoveAt(2);
                    words = wordList.ToArray();
                    }
                }

            // Set to 3 or 4 if we are drawing a triangle (two words).
            int index = 3;

            // The first word must be 'draw'.
            if (words.Length == 0)
                {
                errMsg = "empty request";
                return null;
                }

            // We need at least eight words.
            if (words.Length < 8)
                {
                errMsg = "incomplete request";
                return null;
                }

            // First word 'draw'
            if (words[0] != "draw")
                {
                errMsg = string.Format("Sorry - I can't '{0}', I can only draw.", words[0]);
                return null;
                }

            // A or an
            if (words[1] != "a" && words[1] != "an")
                {
                errMsg = string.Format("Sorry - I don't understand {0}", words[1]);
                return null;
                }

            // Shape
            // Are we drawing a triangle?
            if (triTypes.Contains(words[2]))
                {
                if (words[3] != "triangle")
                    {
                    errMsg = string.Format("Sorry - I don't know how to draw a {0} {1}", words[2], words[3]);
                    return null;
                    }
                triangleType = words[2];
                shape = words[3];
                index = 4;
                }
            else
                {
                shape = words[2];
                }

            if (!shapeNames.Contains(shape))
                {
                errMsg = string.Format("Sorry - I can't draw a {0}", shape);
                return null;
                }

            // At this point we have a number of conjuctive clauses.  Each one must be five words.
            if ((words.Length - index) % 5 != 0)
                {
                errMsg = "Sorry - I don't understand this request.";
                return null;
                }

            // Create the request and set the shape type
            retval = new ShapeRequest(shape, is3d);

            // Now we can parse the measurement parameters.
            for (int i = index; i < words.Length; i += 5)
                {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int j = i; j < i + 5; j++)
                    {
                    sb.Append(words[j] + " ");
                    }
                var m = Measurement.Parse(sb.ToString(), ref errMsg);
                if (m == null) return null;

                // See if it is approprate, and not a duplicate.
                var qry = from _m in measurements where _m.parameterType == m.parameterType select _m;
                if (qry != null && qry.Count() > 0)
                    {
                    errMsg = string.Format("Sorry - duplicate measurement type ({0})", m.parameterType.ToString());
                    }

                if (!retval.IsValidMeasurement(m.parameterType.ToString().ToLower()))
                    {
                    errMsg = string.Format("Sorry - invalid measurement type ({0}) for {1}", m.parameterType.ToString(), retval.ShapeType.ToString());
                    return null;
                    }

                measurements.Add(m);
                }

            retval.Measurements = measurements;
            if (retval.CreateShape(ref errMsg) == null) return null;
            return retval;
            }

        public JSONShape Emit(ref string errMsg)
            {
            if (_shape == null) return null;
            return _shape.Emit(ref errMsg);
            }

        private bool IsPolygon
            {
            get
                {
                return ShapeType == eShapeType2d.Heptagon || ShapeType == eShapeType2d.Hexagon ||
                    ShapeType == eShapeType2d.Octagon || ShapeType == eShapeType2d.Pentagon;
                }
            }

        public List<Point> CalcPolygon(int radius, int sides)
            {
            List<Point> retval = new List<Point>();
            const double twoPi = 2.0 * Math.PI;

            // Use these for a moveable origin.
            int x = 0, y = 0;

            if (radius <= 0 || sides < 3) return null;

            for (int i = 0; i < sides; i++)
                {
                int x1 = x + (int)Math.Round((radius * Math.Cos(twoPi * i / sides)));
                int y1 = y + (int)Math.Round((radius * Math.Sin(twoPi * i / sides)));
                retval.Add(new Point(x1, y1));
                }
            return retval;
            }

        /// <summary>
        /// Is the measurement type valid for this shape.
        /// </summary>
        /// <param name="mname">e.g. radius</param>
        /// <returns>True if it's valid.</returns>
        public bool IsValidMeasurement(string mname)
            {
            List<string> allowedTypes = new List<string>();

            if (!measurementNames.Contains(mname)) return false;
            switch (ShapeType)
                {
                case eShapeType2d.Circle:
                    allowedTypes.Add("radius");
                    break;
                case eShapeType2d.Heptagon:
                case eShapeType2d.Hexagon:
                case eShapeType2d.Octagon:
                case eShapeType2d.Pentagon:
                    allowedTypes.Add("radius");
                    allowedTypes.Add("side");
                    break;
                case eShapeType2d.Oval:
                    break;
                case eShapeType2d.Parallelogram:
                    allowedTypes.Add("height");
                    allowedTypes.Add("width");
                    allowedTypes.Add("offset");
                    break;
                case eShapeType2d.Rectangle:
                    allowedTypes.Add("height");
                    allowedTypes.Add("width");
                    break;
                case eShapeType2d.Square:
                    allowedTypes.Add("height");
                    allowedTypes.Add("width");
                    allowedTypes.Add("side");
                    break;
                case eShapeType2d.Triangle:
                    allowedTypes.Add("height");
                    allowedTypes.Add("width");
                    allowedTypes.Add("side");
                    break;
                case eShapeType2d.Ellipse:
                    allowedTypes.Add("originx");
                    allowedTypes.Add("originy");
                    allowedTypes.Add("radiusx");
                    allowedTypes.Add("radiusy");
                    allowedTypes.Add("rotation");
                    break;
                default:
                    return false;
                }

            if (Is3D) allowedTypes.Add("depth");

            return allowedTypes.Contains(mname);
            }

        /// <summary>
        /// Get the value of the given measurement type.
        /// </summary>
        /// <param name="mtype"></param>
        /// <returns>The value, or -1 if it doesn't exist.</returns>
        private int getMeasurementValue(eParamType mtype)
            {
            if (Measurements == null || Measurements.Count == 0) return -1;
            var qry = from _m in Measurements where _m.parameterType == mtype select _m;
            if (qry != null && qry.Count() > 0) return qry.First().Value;
            return -1;
            }

        /// <summary>
        /// Assuming we have all the correct parameters, create the specific shape
        /// </summary>
        /// <param name="errMsg">If we can't create the shape, this is the reason.</param>
        /// <returns>The created shape</returns>
        /// <remarks>The shape is also stored internally (in _shape)</remarks>
        public Shape CreateShape(ref string errMsg)
            {
            try
                {

                int height, width, radius, sides = 0, offset, depth = 0;
                int rx, ry, ox, oy;

                switch (ShapeType)
                    {
                    case eShapeType2d.Square:
                        height = getMeasurementValue(eParamType.Height);
                        if (Is3D) depth = getMeasurementValue(eParamType.Depth);
                        if (depth < 0) depth = 0;
                        if (height <= 0) height = getMeasurementValue(eParamType.Width);
                        if (height <= 0) height = getMeasurementValue(eParamType.Side);
                        if (height >= 0)
                            {
                            _shape = new Square(height, depth);
                            }
                        else
                            {
                            errMsg = string.Format("Invalid or missing size ({0})", height);
                            return null;
                            }
                        break;
                    case eShapeType2d.Pentagon:
                    case eShapeType2d.Hexagon:
                    case eShapeType2d.Heptagon:
                    case eShapeType2d.Octagon:
                        if (ShapeType == eShapeType2d.Pentagon)
                            sides = 5;
                        else if (ShapeType == eShapeType2d.Hexagon)
                            sides = 6;
                        else if (ShapeType == eShapeType2d.Heptagon)
                            sides = 7;
                        else
                            sides = 8;
                        if (sides <= 2)
                            {
                            errMsg = string.Format("Invalid number of sides ({0})", sides);
                            return null;
                            }
                        radius = getMeasurementValue(eParamType.Radius);
                        if (radius <= 0)
                            {
                            int slen = getMeasurementValue(eParamType.Side);
                            radius = (int)Math.Round(calcRadius(slen, sides));
                            }
                        _shape = new Polygon(radius, sides);
                        break;
                    case eShapeType2d.Triangle:
                        sides = getMeasurementValue(eParamType.Side);
                        if (sides > 0)
                            {
                            height = sides;
                            width = sides;
                            }
                        else
                            {
                            height = getMeasurementValue(eParamType.Height);
                            width = getMeasurementValue(eParamType.Width);
                            }
                        if (height > 0 && width > 0)
                            {
                            _shape = new Triangle(height, width);
                            }
                        else
                            {
                            errMsg = "Invalid or missing size";
                            return null;
                            }
                        break;
                    case eShapeType2d.Rectangle:
                        height = getMeasurementValue(eParamType.Height);
                        width = getMeasurementValue(eParamType.Width);
                        _shape = new Rectangle(height, width);
                        break;
                    case eShapeType2d.Circle:
                        radius = getMeasurementValue(eParamType.Radius);
                        _shape = new Circle(radius);
                        break;
                    case eShapeType2d.Parallelogram:
                        height = getMeasurementValue(eParamType.Height);
                        width = getMeasurementValue(eParamType.Width);
                        offset = getMeasurementValue(eParamType.Offset);
                        _shape = new Parallelogram(height, width, offset);
                        break;
                    case eShapeType2d.Ellipse:
                        rx = getMeasurementValue(eParamType.RadiusX);
                        ry = getMeasurementValue(eParamType.RadiusY);
                        ox = getMeasurementValue(eParamType.OriginX);
                        oy = getMeasurementValue(eParamType.OriginY);
                        var rot = getMeasurementValue(eParamType.Rotation);
                        if (rot < 0) rot = 0;
                        _shape = new Ellipse(ox, oy, rx, ry, rot);
                        break;
                    }

                if (_shape != null) _shape.Is3D = Is3D;
                return _shape;
                }
            catch (Exception ex)
                {
                errMsg = ex.Message;
                return null;
                }
            }

        private static double calcRadius(int sideLength, int sides)
            {
            if (sideLength <= 0) throw new ArgumentException("The length of a side cannot be negative or zero");
            if (sides < 3) throw new ArgumentException("The number of sides cannot be less than 3");

            double angle = Math.PI / sides;
            return sideLength / (2 * Math.Sin(angle));
            }
        }

    /// <summary>
    /// Defines a parameter for a shape, e.g. height.
    /// </summary>
    internal class Measurement
        {
        public eParamType parameterType;
        public int Value = 0;

        /// <summary>
        /// Parse a measurement clause into a measurement object.
        /// </summary>
        /// <param name="clause">A clause of the form
        /// with|and a(n) 'measurement' of 'value'
        /// </param>
        /// <param name="errMsg"></param>
        /// <returns>A decoded measurement</returns>
        public static Measurement Parse(string clause, ref string errMsg)
            {
            Measurement m = null;

            string[] words = clause.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length != 5)
                {
                errMsg = "Sorry - invalid measurement clause";
                return null;
                }
            if (words[0] != "with" && words[0] != "and")
                {
                errMsg = string.Format("Sorry - invalid conjuctive ({0})", words[0]);
                return null;
                }
            if (words[1] != "a" && words[1] != "an")
                {
                errMsg = "Sorry - invalid measurement clause";
                return null;
                }

            m = new Measurement();
            switch (words[2])
                {
                case "radius":
                    m.parameterType = eParamType.Radius;
                    break;
                case "height":
                    m.parameterType = eParamType.Height;
                    break;
                case "width":
                    m.parameterType = eParamType.Width;
                    break;
                case "side":
                    m.parameterType = eParamType.Side;
                    break;
                case "offset":
                    m.parameterType = eParamType.Offset;
                    break;
                case "radiusx":
                    m.parameterType = eParamType.RadiusX;
                    break;
                case "radiusy":
                    m.parameterType = eParamType.RadiusY;
                    break;
                case "originx":
                    m.parameterType = eParamType.OriginX;
                    break;
                case "originy":
                    m.parameterType = eParamType.OriginY;
                    break;
                case "rotation":
                    m.parameterType = eParamType.Rotation;
                    break;
                case "depth":
                    m.parameterType = eParamType.Depth;
                    break;
                default:
                    errMsg = string.Format("Sorry - invalid measurement type ({0})", words[2]);
                    return null;
                }
            if (words[3] != "of" && words[3] != "=")
                {
                errMsg = "Sorry - invalid measurement clause";
                return null;
                }
            if (!int.TryParse(words[4], out m.Value))
                {
                errMsg = string.Format("Sorry - invalid value ({0})", words[4]);
                return null;
                }
            if (m.Value <= 0)
                {
                errMsg = "Sorry - negative or zero values are not allowed.";
                return null;
                }
            return m;
            }
        }
    }
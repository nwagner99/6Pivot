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
    /// A list of supported shape types.
    /// </summary>
    internal enum eShapeType
        {
        Circle, Oval, Triangle, Square, Rectangle, Parallelogram, Pentagon, Hexagon, Heptagon, Octagon, Ellipse,
        Cube, Sphere, Prism
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
        abstract public bool Is3D { get; }

        /// <summary>
        /// Based on the parameters for the particular shape, create
        /// the JSON object that will be returned to the client.
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        abstract public JSONShape Emit(ref string errMsg);
        }

    internal class Square : Shape
        {
        public int Height;

        public Square(int height)
            {
            if (height <= 0) throw new ArgumentException("height cannot be negative or zero");
            Height = height;
            }

        public override bool Is3D
            {
            get
                {
                return false;
                }
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

        public override bool Is3D
            {
            get
                {
                return false;
                }
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
    /// Covers the n-sided polygons, currently from 5 to 8.
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

        public override bool Is3D
            {
            get
                {
                return false;
                }
            }
        public override JSONShape Emit(ref string errMsg)
            {
            JSONShape retval = new Controllers.JSONShape();
            retval.type = string.Format("{0} sided polygon", Sides);

            // Set the origin of the polygon in the middle of the canvas, since the calculated vertices can go negative.
            origin.X = Properties.Settings.Default.MaxImageSize / 2;
            origin.Y = origin.X;

            retval.points = CalcPolygon(origin, Radius, Sides).ToArray();
            retval.radius = Radius;
            retval.sides = Sides;
            retval.status = true;
            retval.is3d = Is3D;

            return retval;
            }

        /// <summary>
        /// Calculate the vertices of the given polygon
        /// </summary>
        /// <param name="origin">Origin of the shape, since the vertices can go negative.</param>
        /// <param name="radius">Distince from the origin to a vertex.</param>
        /// <param name="sides">Number of sides, must be greater than 2</param>
        /// <returns></returns>
        public List<Point> CalcPolygon(Point origin, int radius, int sides)
            {
            List<Point> retval = new List<Point>();
            const double twoPi = 2.0 * Math.PI;

            if (radius <= 0 || sides < 3) return null;

            for (int i = 0; i < sides; i++)
                {
                int x1 = origin.X + (int)Math.Round((radius * Math.Cos(twoPi * i / sides)));
                int y1 = origin.Y + (int)Math.Round((radius * Math.Sin(twoPi * i / sides)));
                retval.Add(new Point(x1, y1));
                }
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

        public override bool Is3D
            {
            get
                {
                return false;
                }
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

        public override bool Is3D
            {
            get
                {
                return false;
                }
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

        public override bool Is3D
            {
            get
                {
                return false;
                }
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

        public override bool Is3D
            {
            get
                {
                return false;
                }
            }
        public override JSONShape Emit(ref string errMsg)
            {
            JSONShape retval = new Controllers.JSONShape() { type = "circle", radius = Radius };
            retval.status = true;
            retval.is3d = Is3D;
            return retval;
            }
        }

    internal class Cube : Shape
        {
        private int Height = 0;

        public override bool Is3D
            {
            get
                {
                return true;
                }
            }

        public Cube(int size)
            {
            if (size <= 0) throw new ArgumentException("cube size cannot be negative or zero");
            Height = size;
            }
        public override JSONShape Emit(ref string errMsg)
            {
            return new Controllers.JSONShape() { type = "cube", status = true, depth = Height, is3d = true };
            }
        }

    internal class Sphere : Shape
        {
        public int Radius = 0;

        public override bool Is3D
            {
            get
                {
                return true;
                }
            }

        public Sphere(int radius)
            {
            if (radius <= 0) throw new ArgumentException("radius cannot be negative or zero");
            Radius = radius;
            }

        public override JSONShape Emit(ref string errMsg)
            {
            return new JSONShape() { type = "sphere", status = true, radius = Radius, is3d = true };
            }
        }

    /// <summary>
    /// The object returned back from the shape controller.  This is the object
    /// that is sent to the client.
    /// </summary>
    public class JSONShape
        {
        public string type = string.Empty;
        public bool is3d = false;

        /// <summary>
        /// If false, then the shape could not be rendered, and the error message
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

        public eShapeType ShapeType;
        public List<Measurement> Measurements = new List<Measurement>();

        private static List<string> shapeNames = new List<string>();
        private static List<string> measurementNames = new List<string>();
        private static List<string> triTypes = new List<string>();
        private List<string> allowedTypes = null;

        public ShapeRequest(string shType)
            : base()
            {
            if (!shapeNames.Contains(shType)) throw new ArgumentException(string.Format("Unknown shape type ({0})", shType));
            ShapeType = (eShapeType)shapeNames.IndexOf(shType);
            }

        static ShapeRequest()
            {
            // Build a list of names from the enumerated types.
            foreach (var name in Enum.GetNames(typeof(eShapeType))) shapeNames.Add(name.ToLower());
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
            try
                {

                ShapeRequest retval = null;

                // First token is the request
                string[] words = request.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Use this to check for 3d
                List<string> wordList = words.ToList();

                string shape = string.Empty;
                string triangleType = string.Empty;
                List<Measurement> measurements = new List<Measurement>();

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

                // At this point we have a number of conjunctive clauses.  Each one must be five words.
                if ((words.Length - index) % 5 != 0)
                    {
                    errMsg = "Sorry - I don't understand this request.";
                    return null;
                    }

                // Create the request and set the shape type
                try
                    {
                    retval = new ShapeRequest(shape);
                    }
                catch (ArgumentException e)
                    {
                    errMsg = e.Message;
                    return null;
                    }

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
            catch (Exception ex)
                {
                errMsg = ex.Message;
                return null;
                }
            }

        public JSONShape Emit(ref string errMsg)
            {
            if (_shape == null)
                {
                errMsg = "no shape defined";
                return null;
                }
            return _shape.Emit(ref errMsg);
            }

        private bool IsPolygon
            {
            get
                {
                return ShapeType == eShapeType.Heptagon || ShapeType == eShapeType.Hexagon ||
                    ShapeType == eShapeType.Octagon || ShapeType == eShapeType.Pentagon;
                }
            }


        /// <summary>
        /// Is the measurement type valid for this shape.
        /// </summary>
        /// <param name="mname">e.g. radius</param>
        /// <returns>True if it's valid.</returns>
        public bool IsValidMeasurement(string mname)
            {
            // Build the allowed types if required.
            if (allowedTypes == null) buildAllowedTypes();

            if (!measurementNames.Contains(mname)) return false;
            return allowedTypes.Contains(mname);
            }

        /// <summary>
        /// Build the list of allowed measurement types for the current shape type.
        /// </summary>
        private void buildAllowedTypes()
            {
            allowedTypes = new List<string>();
            switch (ShapeType)
                {
                case eShapeType.Circle:
                    allowedTypes.Add("radius");
                    break;
                case eShapeType.Heptagon:
                case eShapeType.Hexagon:
                case eShapeType.Octagon:
                case eShapeType.Pentagon:
                    allowedTypes.Add("radius");
                    allowedTypes.Add("side");
                    break;
                case eShapeType.Oval:
                    break;
                case eShapeType.Parallelogram:
                    allowedTypes.Add("height");
                    allowedTypes.Add("width");
                    allowedTypes.Add("offset");
                    break;
                case eShapeType.Rectangle:
                    allowedTypes.Add("height");
                    allowedTypes.Add("width");
                    break;
                case eShapeType.Square:
                    allowedTypes.Add("height");
                    allowedTypes.Add("width");
                    allowedTypes.Add("side");
                    break;
                case eShapeType.Triangle:
                    allowedTypes.Add("height");
                    allowedTypes.Add("width");
                    allowedTypes.Add("side");
                    break;
                case eShapeType.Ellipse:
                    allowedTypes.Add("originx");
                    allowedTypes.Add("originy");
                    allowedTypes.Add("radiusx");
                    allowedTypes.Add("radiusy");
                    allowedTypes.Add("rotation");
                    break;
                case eShapeType.Cube:
                    allowedTypes.Add("height");
                    allowedTypes.Add("width");
                    allowedTypes.Add("side");
                    allowedTypes.Add("depth");
                    break;
                case eShapeType.Sphere:
                    allowedTypes.Add("radius");
                    break;
                }
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

                int height, width, radius, sides = 0, offset;
                int rx, ry, ox, oy;

                // Create the shape with the given measurements.
                // We should try and be a bit flexible with the specific measurement types.
                switch (ShapeType)
                    {
                    case eShapeType.Square:
                        height = getMeasurementValue(eParamType.Height);
                        if (height <= 0) height = getMeasurementValue(eParamType.Width);
                        if (height <= 0) height = getMeasurementValue(eParamType.Side);
                        if (height >= 0)
                            {
                            _shape = new Square(height);
                            }
                        else
                            {
                            errMsg = string.Format("Invalid or missing size ({0})", height);
                            return null;
                            }
                        break;
                    case eShapeType.Pentagon:
                    case eShapeType.Hexagon:
                    case eShapeType.Heptagon:
                    case eShapeType.Octagon:
                        if (ShapeType == eShapeType.Pentagon)
                            sides = 5;
                        else if (ShapeType == eShapeType.Hexagon)
                            sides = 6;
                        else if (ShapeType == eShapeType.Heptagon)
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
                            if (slen <= 0)
                                {
                                errMsg = string.Format("You must specify a radius or side length.");
                                return null;
                                }
                            radius = (int)Math.Round(calcRadius(slen, sides));
                            }
                        _shape = new Polygon(radius, sides);
                        break;
                    case eShapeType.Triangle:
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
                        if (height > 0 && width <= 0)
                            width = height;
                        else if (width > 0 && height <= 0)
                            height = width;
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
                    case eShapeType.Rectangle:
                        height = getMeasurementValue(eParamType.Height);
                        width = getMeasurementValue(eParamType.Width);
                        if (height <= 0 || width <= 0)
                            {
                            errMsg = "Both the height and width must be greater than zero.";
                            return null;
                            }
                        _shape = new Rectangle(height, width);
                        break;
                    case eShapeType.Circle:
                        radius = getMeasurementValue(eParamType.Radius);
                        if (radius <= 0)
                            {
                            errMsg = "The radius cannot be negative or zero.";
                            return null;
                            }
                        _shape = new Circle(radius);
                        break;
                    case eShapeType.Parallelogram:
                        height = getMeasurementValue(eParamType.Height);
                        width = getMeasurementValue(eParamType.Width);
                        offset = getMeasurementValue(eParamType.Offset);
                        if (height <= 0)
                            {
                            errMsg = "Height is either missing, negative or zero.";
                            return null;
                            }
                        if (width <= 0)
                            {
                            errMsg = "Width is either missing, negative or zero.";
                            return null;
                            }
                        if (offset <= 0)
                            {
                            errMsg = "Offset is either missing, negative or zero.";
                            return null;
                            }

                        _shape = new Parallelogram(height, width, offset);
                        break;
                    case eShapeType.Ellipse:
                        rx = getMeasurementValue(eParamType.RadiusX);
                        ry = getMeasurementValue(eParamType.RadiusY);
                        ox = getMeasurementValue(eParamType.OriginX);
                        oy = getMeasurementValue(eParamType.OriginY);
                        var rot = getMeasurementValue(eParamType.Rotation);
                        if (rot < 0) rot = 0;
                        _shape = new Ellipse(ox, oy, rx, ry, rot);
                        break;
                    case eShapeType.Cube:
                        height = getMeasurementValue(eParamType.Height);
                        if (height <= 0) height = getMeasurementValue(eParamType.Width);
                        if (height <= 0) height = getMeasurementValue(eParamType.Depth);
                        if (height <= 0) height = getMeasurementValue(eParamType.Side);
                        if (height <= 0)
                            {
                            errMsg = "You must specify a cube size.";
                            return null;
                            }
                        _shape = new Cube(height);
                        break;
                    case eShapeType.Sphere:
                        radius = getMeasurementValue(eParamType.Radius);
                        if (radius <= 0)
                            {
                            errMsg = "the radius cannot be negative or zero";
                            return null;
                            }
                        _shape = new Sphere(radius);
                        break;
                    default:
                        errMsg = string.Format("Sorry - I can't draw a {0} yet.", ShapeType.ToString());
                        return null;
                    }

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
        private static List<string> _paramNames = null;

        static Measurement()
            {
            _paramNames = new List<string>();
            var names = Enum.GetNames(typeof(eParamType));
            foreach (var name in names) _paramNames.Add(name.ToLower());
            }

        /// <summary>
        /// Parse a measurement clause into a measurement object.
        /// </summary>
        /// <param name="clause">A clause of the form
        /// with|and a(n) 'measurement' of 'value'
        /// </param>
        /// <param name="errMsg"></param>
        /// <returns>A decoded measurement object</returns>
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
                errMsg = string.Format("Sorry - invalid conjunction ({0})", words[0]);
                return null;
                }
            if (words[1] != "a" && words[1] != "an")
                {
                errMsg = "Sorry - invalid measurement clause";
                return null;
                }

            m = new Measurement();
            if (!_paramNames.Contains(words[2]))
                {
                errMsg = string.Format("Unknown measurement type {0}", words[2]);
                return null;
                }
            m.parameterType = (eParamType)_paramNames.IndexOf(words[2]);

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
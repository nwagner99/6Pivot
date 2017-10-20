using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _6PivotPolygon.Controllers;

namespace ShapeTester
    {
    [TestClass]
    public class UnitTest1
        {
        private ShapesProxy _control = new ShapesProxy();

        public UnitTest1()
            {

            }

        [TestMethod]
        public void TestSquare()
            {

            string s = "Draw a square with a height of 150";
            var retval = _control.GetShape(s);
            if (retval == null) Assert.Fail(string.Format("Failed {0}", s));

            Assert.AreEqual("square", retval.type);
            Assert.AreEqual(true, retval.status);
            Assert.AreEqual(4, retval.points.Length);
            }

        [TestMethod]
        public void TestCircle()
            {
            string s = "draw a circle with a radius of 100";
            var retval = _control.GetShape(s);
            if (retval == null) Assert.Fail(string.Format("Failed {0}", s));

            Assert.AreEqual("circle", retval.type);
            Assert.AreEqual(true, retval.status);
            Assert.AreEqual(null, retval.points);
            Assert.AreEqual(100, retval.radius);
            }

        [TestMethod]
        public void TestAllPolygons()
            {
            for (int i = 5; i <= 8; i++) TestPolygon(i);
            }

        public void TestPolygon(int sides)
            {
            string[] _shapes = { "pentagon", "hexagon", "heptagon", "octagon" };
            Assert.IsTrue(sides >= 5 && sides <= 8);

            string s = "draw a " + _shapes[sides - 5] + " with a radius of 150";
            var retval = _control.GetShape(s);
            Assert.IsNotNull(retval);
            Assert.AreEqual(string.Format("{0} sided polygon", sides), retval.type);
            Assert.AreEqual(sides, retval.sides);
            Assert.AreEqual(retval.points.Length, sides);
            Assert.AreEqual(150, retval.radius);
            }

        [TestMethod]
        public void TestEllipse()
            {
            string s = "Draw an ellipse with an originx of 200 and an originy of 200 and a radiusx of 100 and a radiusy of 150 and a rotation of 45";
            var retval = _control.GetShape(s);

            Assert.IsNotNull(retval);
            Assert.AreEqual("ellipse", retval.type);
            Assert.AreEqual(true, retval.status);
            Assert.IsNull(retval.points);
            Assert.AreEqual(200, retval.originX);
            Assert.AreEqual(200, retval.originY);
            Assert.AreEqual(100, retval.radiusX);
            Assert.AreEqual(150, retval.radiusY);
            Assert.AreEqual(Math.PI / 4.0, retval.rotation);
            }
        }
    }

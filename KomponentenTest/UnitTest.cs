using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;

namespace TrassierungInterface
{
    [TestClass]
    public class UnitTest
    {
        public TestContext TestContext { get; set; }
        public UnitTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(config =>
            {
                config.AddConsole();
            });
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            TrassierungLog.AssignLogger(loggerFactory);
        }

        #region Geometry Interpolation tests
        [TestMethod]
        [TestCategory("GeometryInterpolation")]
        [DataRow(100, 100, 0, DisplayName = "InterpolationGerade")]
        [DataRow(double.NaN, double.NaN, double.NaN, DisplayName = "InterpolationGerade NaN")]
        public void InterpolationGerade(double input, double expectedX, double expectedY)
        {
            double delta = 1.0;
            int num = Double.IsNaN(input) ? 1 : (int)(input / delta) + 1;
            double[] Xinterp = new double[num];
            double[] Yinterp = new double[num];
            TrassenGeometrie gerade = new Gerade();
            for (int i = 0; i < Xinterp.Length; i++)
            {
                (Xinterp[i], Yinterp[i], _, _) = gerade.PointAt(i < Xinterp.Length - 1 ? i * delta : input);
            }
            if (Double.IsNaN(Xinterp.Last()) || Double.IsNaN(Xinterp.Last()))
            {
                Assert.IsTrue(Double.IsNaN(input), "Line Interpolation returned NaN-coordinate");
                return;
            }
            Assert.AreEqual(expectedX, Xinterp.Last(), 0.0000001, "Line Interpolation returned wrong X-coordinate");
            Assert.AreEqual(expectedY, Yinterp.Last(), 0.0000001, "Line Interpolation returned wrong Y-coordinate");
        }

        [TestMethod]
        [TestCategory("GeometryInterpolation")]
        [DataRow(100 * Math.PI, 100, 0, 200, DisplayName = "half circle right")]
        [DataRow(100 * Math.PI, -100, 0, -200, DisplayName = "half circle left")]
        [DataRow(100 * Math.PI * 0.5, 100, 100, 100, DisplayName = "quater circle right")]
        [DataRow(100 * Math.PI * 0.5, -100, 100, -100, DisplayName = "quater circle left")]
        [DataRow(Double.NaN, -100, Double.NaN, Double.NaN, DisplayName = "circle NaN")]
        [DataRow(100, 0, 100, 0, DisplayName = "zero radius")]
        public void InterpolationCircle(double input, double radius, double expectedX, double expectedY)
        {
            double delta = 1.0;
            int num = Double.IsNaN(input) ? 1 : (int)(input / delta) + 1;
            double[] Xinterp = new double[num];
            double[] Yinterp = new double[num];
            TrassenGeometrie circle = new Kreis(radius);
            for (int i = 0; i < Xinterp.Length; i++)
            {
                (Xinterp[i], Yinterp[i], _, _) = circle.PointAt(i < Xinterp.Length - 1 ? i * delta : input);
            }
            if (Double.IsNaN(Xinterp.Last()) || Double.IsNaN(Xinterp.Last()))
            {
                Assert.IsTrue(Double.IsNaN(input), "Circle Interpolation returned NaN-coordinate");
                return;
            }
            Assert.AreEqual(expectedX, Xinterp.Last(), 0.0000001, "Circle Interpolation returned wrong X-coordinate");
            Assert.AreEqual(expectedY, Yinterp.Last(), 0.0000001, "Circle Interpolation returned wrong Y-coordinate");
        }

        [TestMethod]
        [TestCategory("GeometryInterpolation")]
        [DataRow(100, 0, 50, 100, DisplayName = "Clothoid curvature increase from 0 right turn")]
        [DataRow(100, 50, 0, 100, DisplayName = "Clothoid curvature decrease to 0 right turn")]
        [DataRow(100, 0, -50, 100, DisplayName = "Clothoid curvature increase from 0 left turn")]
        [DataRow(100, -50, 0, 100, DisplayName = "Clothoid curvature decrease to 0 left turn")]
        [DataRow(100, 500, 50, 100, DisplayName = "Clothoid curvature increase right turn")]
        [DataRow(100, -500, -50, 100, DisplayName = "Clothoid curvature increase left turn")]
        [DataRow(100, 50, 500, 100, DisplayName = "Clothoid curvature decrease right turn")]
        [DataRow(100, -50, -500, 100, DisplayName = "Clothoid curvature decrease left turn")]
        [DataRow(100, 50, -50, 100, DisplayName = "Clothoid curvature decrease right left turn")]
        [DataRow(100, -50, 50, 100, DisplayName = "Clothoid curvature decrease left right turn")]
        [DataRow(64.81533, 3221.20091, 1573.91942, 64.81533, DisplayName = "Clothoid curvature decrease2")]
        [DataRow(80, 0, 500, 80, double.NegativeInfinity, double.NegativeInfinity, 0.0800006569, DisplayName = "Clothoid curvature increase2")] //R.Ullrich EVT_3_Uebergangsboegen
        [DataRow(Double.NaN, -50, 100, 100, double.NaN, double.NaN, DisplayName = "Clothoid s = NaN")]
        [DataRow(100, 0, 0, 100, 100, 0, DisplayName = "Zero Radii")]
        [DataRow(100, 0, 0, 0, 100, 0, 0, DisplayName = "Zero Length")]
        // TODO: Testdata for Clothoids needed including expected X and Y
        public void InterpolationClothoid(double input, double radius1, double radius2, double length, double expectedX = double.NegativeInfinity, double expectedY = double.NegativeInfinity, double expectedTangent = double.NegativeInfinity)
        {
            double delta = 1.0;
            int num = Double.IsNaN(input) ? 1 : (int)(input / delta) + 1;
            double[] Xinterp = new double[num];
            double[] Yinterp = new double[num];
            double[] Tinterp = new double[num];
            TrassenGeometrie klotoid = new Klothoid(radius1, radius2, length);
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(TestContext.TestDir, @"..\CoordinatesOut.txt")))
            {
                for (int i = 0; i <= (int)input; i++)
                {
                    (Xinterp[i], Yinterp[i], Tinterp[i], _) = klotoid.PointAt(i);
                    Debug.WriteLine("Clothoid: " + Xinterp[i] + " " + Yinterp[i] + " " + Tinterp[i]);
                    outputFile.WriteLine(Xinterp[i].ToString(CultureInfo.InvariantCulture) + " " + Yinterp[i].ToString(CultureInfo.InvariantCulture) + " " + Tinterp[i].ToString(CultureInfo.InvariantCulture));
                }
            }
            if (Double.IsNaN(Xinterp.Last()) || Double.IsNaN(Yinterp.Last()))
            {
                Assert.IsTrue(Double.IsNaN(expectedX) && Double.IsNaN(expectedY), "Clothoid Interpolation returned wrong X-coordinate");
                return;
            }
            if (double.IsFinite(expectedX)) { Assert.AreEqual(expectedX, Xinterp.Last(), 0.00001, "Clothoid Interpolation returned wrong X-coordinate"); }
            if (double.IsFinite(expectedY)) { Assert.AreEqual(expectedY, Yinterp.Last(), 0.00001, "Clothoid Interpolation returned wrong Y-coordinate"); }
            if (double.IsFinite(expectedTangent)) { Assert.AreEqual(expectedTangent, Tinterp.Last(), 0.00001, "Clothoid Interpolation returned wrong Tangent"); }
        }
        #endregion

        #region Geometry Projection Tests
        [TestMethod]
        [TestCategory("GeometryProjection")]
        [DataRow(100, 50, double.NaN, 100, DisplayName = "ProjectionGerade")]
        [DataRow(100, 50, -Math.PI / 4, 150, DisplayName = "ProjectionGerade intesecting t = 45°")]
        [DataRow(100, 50, 0, double.NaN, DisplayName = "ProjectionGerade intesecting t = 0°")]
        [DataRow(double.NaN, 50, double.NaN, double.NaN, DisplayName = "ProjectionGerade NaN")]
        public void ProjectionGerade(double X, double Y, double t, double expecteds)
        {
            TrassenGeometrie gerade = new Gerade();
            double s = gerade.sAt(X, Y, t);
            if (Double.IsNaN(s))
            {
                Assert.IsTrue(Double.IsNaN(expecteds), "Line Projection returned wrong s-value");
                return;
            }
            Assert.AreEqual(expecteds, s, 0.0000001, "Line Projection returned wrong s-value");
        }

        [TestMethod]
        [TestCategory("GeometryProjection")]
        [DataRow(100, 0, double.NaN, 100, Math.PI / 4 * 100, DisplayName = "Projection quater Circle")]
        [DataRow(100, 0, Math.PI / 2, 100, Math.PI / 2 * 100, DisplayName = "Projection quater Circle t = 90°")]
        [DataRow(100, 0, Math.PI / 2.0001, 100, double.NaN, DisplayName = "Projection quater Circle t = 89°")]
        [DataRow(5.0, 10.0, double.NaN, 5.0, 11.78097245, DisplayName = "Get s along Circle from Point (135°)")]
        [DataRow(5.0, 0.0, double.NaN, 5.0, 3.926990817, DisplayName = "Get s along Circle from Point (45°)")]
        [DataRow(-10.0, 5.0, double.NaN, 5.0, 23.5619449, DisplayName = "Get s along Circle from Point (270°)")]
        [DataRow(5.0, -10.0, double.NaN, -5.0, 11.78097245, DisplayName = "Get s along Circle from Point (-135°)")]
        [DataRow(double.NaN, -10.0, double.NaN, -5.0, double.NaN, DisplayName = "Get s along Circle from Point NaN")]
        [DataRow(5.0, -10.0, double.NaN, 0.0, 5.0, DisplayName = "Get s along Circle from radius 0")]
        public void ProjectionCircle(double X, double Y, double t, double radius, double expecteds)
        {
            TrassenGeometrie kreis = new Kreis(radius);
            double s = kreis.sAt(X, Y, t);
            if (Double.IsNaN(s))
            {
                Assert.IsTrue(Double.IsNaN(expecteds), "Circle Projection returned wrong s-value");
                return;
            }
            Assert.AreEqual(expecteds, s, 0.0000001, "Circle Projection returned wrong s-value");
        }

        [TestMethod]
        [TestCategory("GeometryProjection")]
        [DataRow(double.NaN, -10.0, double.NaN, 0.0, 5.0, 100, double.NaN, DisplayName = "Get s along Clothoid NaN")]
        [DataRow(5.0, -10.0, double.NaN, 10.0, double.NaN, 100, 36.24000549316406, DisplayName = "Get s along Clothoid r2=NaN")]
        // TODO: Testdata for Clothoids needed
        public void ProjectionClothoid(double X, double Y, double t, double radius1, double radius2, double length, double expecteds)
        {
            TrassenGeometrie klothoid = new Klothoid(radius1, radius2, length);
            double s = klothoid.sAt(X, Y, t);
            if (Double.IsNaN(s))
            {
                Assert.IsTrue(Double.IsNaN(expecteds), "Clothoid Projection returned wrong s-value");
                return;
            }
            Assert.AreEqual(expecteds, s, 0.0000001, "Clothoid Projection returned wrong s-value");
        }
        #endregion

        #region File Import Tests
        [TestMethod]
        [TestCategory("FileImport")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046L.TRA", DisplayName = "6240046L.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046R.TRA", DisplayName = "6240046R.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046S.TRA", DisplayName = "6240046S.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\Achse.TRA", DisplayName = "Achse.TRA")]
        public TRATrasse ImportTestTRA(string Filename)
        {
            TRATrasse trasse = Trassierung.ImportTRA(Filename);
            trasse.Interpolate();
            foreach (TrassenElementExt T in trasse.Elemente)
            {
                T.ToString();
            }
            return trasse;
        }
        [TestMethod]
        [TestCategory("FileImport")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240045L.GRA", DisplayName = "6240045L.GRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240045R.GRA", DisplayName = "6240045R.GRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\Gradiente.GRA", DisplayName = "Gradiente.GRA")]
        public GRATrasse ImportTestGRA(string Filename)
        {
            GRATrasse trasse;
            (trasse, GleisscherenElement[] gleisschere) = Trassierung.ImportGRA(Filename);
            foreach (GradientElementExt T in trasse.GradientenElemente)
            {
                T.print();
            }
            return trasse;
        }
        #endregion

        #region Element Estimation Tests
        [TestMethod]
        [TestCategory("ElementEstimation")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046L.TRA", 5.4221e+06, 5.6488e+06, 26, DisplayName = "6240046L.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046R.TRA", 5.4221e+06, 5.6488e+06, 26, DisplayName = "6240046R.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046S.TRA", 5.4221e+06, 5.6488e+06, 16, DisplayName = "6240046S.TRA")]
        public void GetTRAElementFromPoint(string Filename, double PointY, double PointX, int expectedElementID)
        {
            TRATrasse trasse = Trassierung.ImportTRA(Filename);
            TrassenElementExt element = trasse.GetElementFromPoint(PointX, PointY);
            Assert.AreEqual(expectedElementID, element.ID, "Input Point was not associated with correct Element");
        }

        [TestMethod]
        [TestCategory("ElementEstimation")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240045L.GRA", 54300, 26, DisplayName = "6240045L.GRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240045R.GRA", 48288.1, 11, DisplayName = "6240045R.GRA")]
        public void GetGRAElementFromPoint(string Filename, double s, int expectedElementID)
        {
            GRATrasse trasse;
            (trasse, _) = Trassierung.ImportGRA(Filename);
            GradientElementExt element = trasse.GetGradientElementFromS(s);
            Assert.AreEqual(expectedElementID, element.ID, "Station value was not associated with correct Element");
        }
        #endregion

        #region Transformation Tests
        [TestMethod]
        [TestCategory("CoordinateTransformation")]
        [DataRow(100, 100, 0.2, DisplayName = "Transform 2D")]
        public void Transformation2DTest(double X, double Y, double T)
        {
            Transform2D transform = new Transform2D(100, 100, 0.2);
            Random rnd = new Random();
            double InX = rnd.Next(-1000, 1000);
            double InY = rnd.Next(-1000, 1000);
            double InT = rnd.Next(6);
            double OutX = InX;
            double OutY = InY;
            double OutT = InT;
            transform.Apply(ref InX, ref InY, ref InT);
            transform.ApplyInverse(ref InX, ref InY, ref InT);
            Assert.AreEqual(OutX, InX, 1.0e-10, "Transform gone wrong");
            Assert.AreEqual(OutY, InY, 1.0e-10, "Transform gone wrong");
            Assert.AreEqual(OutT, InT, 1.0e-10, "Transform gone wrong");
        }
        #endregion

        #region OverallExecutionTest
        [TestMethod]
        [TestCategory("OverallExecutionTest")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046L.TRA", "C:\\HTW\\Trassierung\\Infos\\6240046S.TRA", "C:\\HTW\\Trassierung\\Infos\\6240045L.GRA", DisplayName = "3D Interpolation 6240046L.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046R.TRA", "C:\\HTW\\Trassierung\\Infos\\6240046S.TRA", "C:\\HTW\\Trassierung\\Infos\\6240045R.GRA", DisplayName = "3D Interpolation 6240046R.TRA")]
        public void TestImportTrasse(string FilenameTRA_LR, string FilenameTRA_S, string FilenameGRA_LR)
        {
            TRATrasse trasseLR = Trassierung.ImportTRA(FilenameTRA_LR);
            TRATrasse trasseS = Trassierung.ImportTRA(FilenameTRA_S);
            GRATrasse trasseGRA;
            (trasseGRA, _) = Trassierung.ImportGRA(FilenameGRA_LR);
            trasseLR.AssignTrasseS(trasseS);
            trasseLR.AssignGRA(trasseGRA);
            trasseLR.Interpolate3D(null, 5);
            trasseLR.Plot();
            trasseS.Interpolate();
            trasseS.Plot();
        }
        #endregion
    }


}
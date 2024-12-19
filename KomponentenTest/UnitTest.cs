using System.Diagnostics;
using System.Runtime.InteropServices;
using TrassierungInterface;
using ScottPlot.WinForms;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Logging;
using ScottPlot;
using System.Globalization;
using System.Xml.Linq;

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
            TrassierungLog.Init(loggerFactory);
        }

        [TestMethod]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046L.TRA", DisplayName = "6240046L.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046R.TRA", DisplayName = "6240046R.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046S.TRA", DisplayName = "6240046S.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\Achse.TRA", DisplayName = "Achse.TRA")]
        public TRATrasse ImportTestTRA(string Filename)
        {
            TRATrasse trasse = Trassierung.ImportTRA(Filename);
            double[] X = new double[trasse.Elemente.Length];
            double[] Y = new double[trasse.Elemente.Length];
            double[] Xinterp = new double[0];
            double[] Yinterp = new double[0];
            double[] Tinterp = new double[0];
            int[] kz = new int[0];
            int i = 0;
            foreach (TrassenElementExt T in trasse.Elemente)
            {
                T.Interpolate();
                //T.print(); 
                X[i] = T.Xstart;
                Y[i] = T.Ystart;
                int[] filledArray = new int[T.InterpX.Length];
                Array.Fill(filledArray, T.Kz);
                kz = kz.Concat(filledArray).ToArray();
                Xinterp = Xinterp.Concat(T.InterpX).ToArray();
                Yinterp = Yinterp.Concat(T.InterpY).ToArray();
                Tinterp = Tinterp.Concat(T.InterpT).ToArray();
                i++;
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(TestContext.TestDir, @"..\CoordinatesOut.txt")))
            {
                for (int j = 0; j < Xinterp.Length; j++)
                {
                    outputFile.WriteLine(Xinterp[j].ToString(CultureInfo.InvariantCulture) + " " + Yinterp[j].ToString(CultureInfo.InvariantCulture) + " " + Tinterp[j].ToString(CultureInfo.InvariantCulture));
                }
            }
            return trasse;
        }
        [TestMethod]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240045L.GRA", DisplayName = "6240045L.GRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240045R.GRA", DisplayName = "6240045R.GRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\Gradiente.GRA", DisplayName = "Gradiente.GRA")]
        public GRATrasse ImportTestGRA(string Filename)
        {
            GRATrasse trasse;
            (trasse, GleisscherenElement[] TEST2) = Trassierung.ImportGRA(Filename);
            foreach (GradientElementExt T in trasse.GradientenElemente) { T.print(); }
            return trasse;
        }
        [TestMethod]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046L.TRA",5.4221e+06,5.6488e+06,26,DisplayName = "6240046L.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046R.TRA",5.4221e+06,5.6488e+06,26,DisplayName = "6240046R.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046S.TRA", 5.4221e+06, 5.6488e+06,16, DisplayName = "6240046S.TRA")]
        public void TestGetTrassenelementFromPoint(string Filename,double PointY, double PointX, int expectedElementID)
        {
            TRATrasse trasse = ImportTestTRA(Filename);
            TrassenElementExt element = trasse.GetElementFromPoint(PointX, PointY);
            trasse.Plot();
            Assert.AreEqual(element.ID, expectedElementID, "Input Point was not associated with correct Element");
        }

        [TestMethod]
        [DataRow(5.0, 10.0,5.0,11.78097245, DisplayName = "Get s along Circle from Point (135°)")]
        [DataRow(5.0, 0.0, 5.0, 3.926990817, DisplayName = "Get s along Circle from Point (45°)")]
        [DataRow(-10.0, 5.0, 5.0, 23.5619449, DisplayName = "Get s along Circle from Point (270°)")]
        [DataRow(5.0, -10.0, -5.0, 11.78097245, DisplayName = "Get s along Circle from Point (-135°)")]
        public void SalongCircle(double PointX, double PointY, double radius, double expectedS)
        {
            TrassenGeometrie geometry = new Kreis(radius);
            double s = geometry.sAt(PointX, PointY);
            Assert.AreEqual(expectedS,s, 1.0e-8, "S from given Point along Geometry differs from expected value");
        }
        [TestMethod]
        [DataRow(100 * Math.PI, 100, 0, 200, DisplayName = "half circle right")]
        [DataRow(100 * Math.PI, -100, 0, -200, DisplayName = "half circle left")]
        [DataRow(100 * Math.PI * 0.5, 100, 100, 100, DisplayName = "quater circle right")]
        [DataRow(100 * Math.PI * 0.5, -100, 100, -100, DisplayName = "quater circle left")]
        public void InterpolationTestCircle(double input, double radius, double expectedX, double expectedY)
        {
            double delta = 1.0;
            double[] Xinterp = new double[(int)(input/delta) + 1];
            double[] Yinterp = new double[(int)(input/delta) + 1];
            TrassenGeometrie circle = new Kreis(radius);
            for (int i = 0; i < Xinterp.Length; i++)
            {
                (Xinterp[i], Yinterp[i], _,_) = circle.PointAt(i < Xinterp.Length-1?i * delta:input);
            }
            Assert.AreEqual(expectedX, Xinterp.Last(), 0.0000001, "Circle Interpolation returned wrong X-coordinate");
            Assert.AreEqual(expectedY, Yinterp.Last(), 0.0000001, "Circle Interpolation returned wrong Y-coordinate");
        }
        [TestMethod]
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
        public void InterpolationTestClothoid(double input, double radius1, double radius2, double length, double expectedX = double.NegativeInfinity, double expectedY = double.NegativeInfinity, double expectedTangent = double.NegativeInfinity)
        {
            double[] Xinterp = new double[(int)input + 1];
            double[] Yinterp = new double[(int)input + 1];
            double[] Tinterp = new double[(int)input + 1];
            TrassenGeometrie klotoid = new Klothoid(radius1, radius2, length);
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(TestContext.TestDir, @"..\CoordinatesOut.txt")))
            {
                for (int i = 0; i <= (int)input; i++)
                {
                    (Xinterp[i], Yinterp[i], Tinterp[i],_) = klotoid.PointAt(i);
                    Debug.WriteLine("Clothoid: " + Xinterp[i] + " " + Yinterp[i] + " " + Tinterp[i]);
                    outputFile.WriteLine(Xinterp[i].ToString(CultureInfo.InvariantCulture) + " " + Yinterp[i].ToString(CultureInfo.InvariantCulture) + " " + Tinterp[i].ToString(CultureInfo.InvariantCulture));
                }
            }
            if (double.IsFinite(expectedX)) { Assert.AreEqual(expectedX, Xinterp.Last(), 0.00001, "Clothoid Interpolation returned wrong X-coordinate"); }
            if (double.IsFinite(expectedY)) { Assert.AreEqual(expectedY, Yinterp.Last(), 0.00001, "Clothoid Interpolation returned wrong Y-coordinate"); }
            if (double.IsFinite(expectedTangent)) { Assert.AreEqual(expectedTangent, Tinterp.Last(), 0.00001, "Clothoid Interpolation returned wrong Tangent"); }
        }
        [TestMethod]
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
        [TestMethod]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046L.TRA", "C:\\HTW\\Trassierung\\Infos\\6240046S.TRA", "C:\\HTW\\Trassierung\\Infos\\6240045L.GRA", DisplayName = "3D Interpolation 6240046L.TRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240046R.TRA", "C:\\HTW\\Trassierung\\Infos\\6240046S.TRA", "C:\\HTW\\Trassierung\\Infos\\6240045R.GRA", DisplayName = "3D Interpolation 6240046R.TRA")]
        public void TestImportTrasse(string FilenameTRA_LR, string FilenameTRA_S, string FilenameGRA_LR)
        {
            TRATrasse trasseLR = Trassierung.ImportTRA(FilenameTRA_LR);
            TRATrasse trasseS = Trassierung.ImportTRA(FilenameTRA_S);
            GRATrasse trasseGRA;
            (trasseGRA, _) = Trassierung.ImportGRA(FilenameGRA_LR);
            trasseLR.SetTrasseS = trasseS;
            trasseLR.AssignGRA(trasseGRA);
            trasseLR.Interpolate3D(null,10);
            trasseLR.Plot();
            trasseS.Interpolate();
            trasseS.Plot();
        }
    }


    }
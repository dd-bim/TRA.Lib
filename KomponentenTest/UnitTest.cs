using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;

namespace TRA_Lib
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
        [DataRow(100, 50, -Math.PI / 4, 150, DisplayName = "ProjectionGerade intesecting t = 45�")]
        [DataRow(100, 50, 0, double.NaN, DisplayName = "ProjectionGerade intesecting t = 0�")]
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
            Assert.AreEqual(expecteds, s, 0.000001, "Line Projection returned wrong s-value");
        }

        [TestMethod]
        [TestCategory("GeometryProjection")]
        [DataRow(100, 0, double.NaN, 100, Math.PI / 4 * 100, DisplayName = "Projection quater Circle")]
        [DataRow(100, 0, Math.PI / 2, 100, Math.PI / 2 * 100, DisplayName = "Projection quater Circle t = 90�")]
        [DataRow(100, 0, Math.PI / 2.0001, 100, double.NaN, DisplayName = "Projection quater Circle t = 89�")]
        [DataRow(5.0, 10.0, double.NaN, 5.0, 11.78097245, DisplayName = "Get s along Circle from Point (135�)")]
        [DataRow(5.0, 0.0, double.NaN, 5.0, 3.926990817, DisplayName = "Get s along Circle from Point (45�)")]
        [DataRow(-10.0, 5.0, double.NaN, 5.0, 23.5619449, DisplayName = "Get s along Circle from Point (270�)")]
        [DataRow(5.0, -10.0, double.NaN, -5.0, 11.78097245, DisplayName = "Get s along Circle from Point (-135�)")]
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
            trasse = Trassierung.ImportGRA(Filename);
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
        [DataRow("6240046L.TRA", 5.4221e+06, 5.6488e+06, 26, DisplayName = "6240046L.TRA")]
        [DataRow("6240046R.TRA", 5.4221e+06, 5.6488e+06, 26, DisplayName = "6240046R.TRA")]
        [DataRow("6240046S.TRA", 5.4221e+06, 5.6488e+06, 16, DisplayName = "6240046S.TRA")]
        public void GetTRAElementFromPoint(string Filename, double PointY, double PointX, int expectedElementID)
        {
            TRATrasse trasse = Trassierung.ImportTRA(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", Filename));
            TrassenElementExt element = trasse.GetElementFromPoint(PointX, PointY);
            Assert.AreEqual(expectedElementID, element.ID, "Input Point was not associated with correct Element");
        }

        [TestMethod]
        [TestCategory("ElementEstimation")]
        [DataRow("TestData\\6240045L.GRA", 54300, 26, DisplayName = "6240045L.GRA")]
        [DataRow("TestData\\6240045R.GRA", 48288.1, 11, DisplayName = "6240045R.GRA")]
        public void GetGRAElementFromPoint(string Filename, double s, int expectedElementID)
        {
            GRATrasse trasse;
            trasse= Trassierung.ImportGRA(Filename);
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
        [DataRow("TestData\\6240046L.TRA", "TestData\\6240046S.TRA", "TestData\\6240045L.GRA", DisplayName = "3D Interpolation 6240046L.TRA")]
        [DataRow("TestData\\6240046R.TRA", "TestData\\6240046S.TRA", "TestData\\6240045R.GRA", DisplayName = "3D Interpolation 6240046R.TRA")]
        public void TestImportTrasse(string FilenameTRA_LR, string FilenameTRA_S, string FilenameGRA_LR)
        {
            TRATrasse trasseLR = Trassierung.ImportTRA(FilenameTRA_LR);
            TRATrasse trasseS = Trassierung.ImportTRA(FilenameTRA_S);
            GRATrasse trasseGRA;
            trasseGRA = Trassierung.ImportGRA(FilenameGRA_LR);
            trasseLR.AssignTrasseS(trasseS);
            trasseLR.AssignGRA(trasseGRA);
            trasseLR.Interpolate3D(null, 5);
#if USE_SCOTTPLOT
            trasseLR.Plot();
#endif
            trasseS.Interpolate();
#if USE_SCOTTPLOT
            trasseS.Plot();
#endif
        }
#endregion
    }


}

namespace TRA_Tool
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using TRA.Tool;
    using TRA_Lib;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement;
    using static TRA_Lib.Trassierung;

    [TestClass]
    public class RoundtripTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Roundtrip")]
        [DataRow("TestData\\6240046L.TRA", "TestData\\6240046S.TRA", "TestData\\6240045L.GRA",8,0, DisplayName = "Roundtrip for 6240046L.TRA")]
        public void Roundtrip(string FilenameTRA_LR, string FilenameTRA_S, string FilenameGRA_LR, int SourceCRSIdx,int TargetCRSIdx)
        {
            BindingList<string> test = new BindingList<string>(egbt22lib.Convert.Defined_CRS);
            string tmpFolderPath = Path.Combine("TestData", "tmp");
            if (!Directory.Exists(tmpFolderPath))
            {
                Directory.CreateDirectory(tmpFolderPath);
            }
            File.Copy(FilenameTRA_LR,Path.Combine(tmpFolderPath,"TRA.TRA"),true);
            File.Copy(FilenameTRA_S, Path.Combine(tmpFolderPath, "S.TRA"), true);

            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
            //CreateTrassenPanel
            TrassenPanel panel = new TrassenPanel();
            panel.trasseL = Trassierung.ImportTRA(Path.Combine(tmpFolderPath, "TRA.TRA"));
            panel.trasseS = Trassierung.ImportTRA(Path.Combine(tmpFolderPath, "S.TRA"));
            panel.gradientL = Trassierung.ImportGRA(FilenameGRA_LR);
            flowLayoutPanel.Controls.Add(panel);

            //Load Original values
            TRATrasse trasseL = Trassierung.ImportTRA(FilenameTRA_LR);
            TRATrasse trasseS = Trassierung.ImportTRA(FilenameTRA_S);

            //Create TransformPanel & Transform
            TransformPanel transformPanel = new TransformPanel();
            flowLayoutPanel.Controls.Add(transformPanel);
            //Use reflection to set provate properties for testing
            var propertyFrom = typeof(TransformPanel).GetField("comboBox_TransformFrom", BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyFrom != null)
            {
                var comboBoxInstance = propertyFrom.GetValue(transformPanel);

                // Assuming that `comboBoxInstance` is a ComboBox and has a `SelectedItem` property
                var selectedItemProperty = comboBoxInstance.GetType().GetProperty("SelectedIndex");
                if (selectedItemProperty != null)
                {
                    selectedItemProperty.SetValue(comboBoxInstance, SourceCRSIdx); // Set selected item
                }
            }
            var propertyTo = typeof(TransformPanel).GetField("comboBox_TransformTo", BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyTo != null)
            {
                var comboBoxInstance = propertyTo.GetValue(transformPanel);

                // Assuming that `comboBoxInstance` is a ComboBox and has a `SelectedItem` property
                var selectedItemProperty = comboBoxInstance.GetType().GetProperty("SelectedIndex");
                if (selectedItemProperty != null)
                {
                    selectedItemProperty.SetValue(comboBoxInstance, TargetCRSIdx); // Set selected item
                }
            }

            //Update TransformSetup 
            transformPanel.Refresh();
            var methodInfo = typeof(TransformPanelBase).GetMethod("btn_Transform_Click", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = methodInfo.Invoke(transformPanel, new object[] { null, EventArgs.Empty });
            //Save Transformed
            Trassierung.ExportTRA(panel.trasseL, Path.Combine(tmpFolderPath, panel.trasseL.Filename), Trassierung.ESaveScale.multiply);
            Trassierung.ExportTRA(panel.trasseS, Path.Combine(tmpFolderPath, panel.trasseS.Filename), Trassierung.ESaveScale.multiply);

            //Transform inverse--------------------------------
            propertyFrom = typeof(TransformPanel).GetField("comboBox_TransformFrom", BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyFrom != null)
            {
                var comboBoxInstance = propertyFrom.GetValue(transformPanel) as System.Windows.Forms.ComboBox;

                // Assuming that `comboBoxInstance` is a ComboBox and has a `SelectedItem` property
                var selectedItemProperty = comboBoxInstance.GetType().GetProperty("SelectedIndex");
                if (selectedItemProperty != null)
                {
                    selectedItemProperty.SetValue(comboBoxInstance, TargetCRSIdx); // Set selected item
                }
            }
            propertyTo = typeof(TransformPanel).GetField("comboBox_TransformTo", BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyTo != null)
            {
                var comboBoxInstance = propertyTo.GetValue(transformPanel) as System.Windows.Forms.ComboBox;

                // Assuming that `comboBoxInstance` is a ComboBox and has a `SelectedItem` property
                var selectedItemProperty = comboBoxInstance.GetType().GetProperty("SelectedIndex");
                if (selectedItemProperty != null)
                {
                    selectedItemProperty.SetValue(comboBoxInstance, SourceCRSIdx); // Set selected item
                }
            }
            //Update TransformSetup 
            transformPanel.Refresh();
            //Trigger Transform
            result = methodInfo.Invoke(transformPanel, new object[] { null, EventArgs.Empty });

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(TestContext.TestDir, @"..\Roundtrip.csv")))
            {
                outputFile.WriteLine($"Differences from Roundtrip for {FilenameTRA_LR}");
                outputFile.WriteLine("ID;X;Y;T;L;R1;R2");
                //Compare Roundtrip
                for (int i = 0; i < trasseL.Elemente.Count(); i++)
                {
                    TrassenElementExt elemenPre = trasseL.Elemente[i];
                    TrassenElementExt elementPost = panel.trasseL.Elemente[i];
                    Assert.AreEqual(elemenPre.ID, elementPost.ID, "Element ID mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.Xstart, elementPost.Xstart, 0.0001, "Element X mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.Ystart, elementPost.Ystart, 0.0001, "Element Y mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.T, elementPost.T, 0.00001, "Element T mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.L, elementPost.L, 0.00001, "Element L mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.R1, elementPost.R1, 0.00001, "Element R1 mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.R2, elementPost.R2, 0.00001, "Element R2 mismatch after roundtrip");
                    outputFile.WriteLine(string.Join(";",
                        elemenPre.ID,
                        (elemenPre.Xstart - elementPost.Xstart),
                        (elemenPre.Ystart - elementPost.Ystart),
                        (elemenPre.T - elementPost.T),
                        (elemenPre.L - elementPost.L),
                        (elemenPre.R1 - elementPost.R1),
                        (elemenPre.R2 - elementPost.R2)));
                }
                outputFile.WriteLine($"Differences from Roundtrip for {FilenameTRA_LR}");
                outputFile.WriteLine("ID;X;Y;T;L;R1;R2");
                for (int i = 0; i < trasseS.Elemente.Count(); i++)
                {
                    TrassenElementExt elemenPre = trasseS.Elemente[i];
                    TrassenElementExt elementPost = panel.trasseS.Elemente[i];
                    Assert.AreEqual(elemenPre.ID, elementPost.ID, "Element ID mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.Xstart, elementPost.Xstart, 0.0001, "Element X mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.Ystart, elementPost.Ystart, 0.0001, "Element Y mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.T, elementPost.T, 0.00001, "Element T mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.L, elementPost.L, 0.00001, "Element L mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.R1, elementPost.R1, 0.00001, "Element R1 mismatch after roundtrip");
                    Assert.AreEqual(elemenPre.R2, elementPost.R2, 0.00001, "Element R2 mismatch after roundtrip");
                    outputFile.WriteLine(string.Join(";",
                        elemenPre.ID,
    (elemenPre.Xstart - elementPost.Xstart),
    (elemenPre.Ystart - elementPost.Ystart),
    (elemenPre.T - elementPost.T),
    (elemenPre.L - elementPost.L),
    (elemenPre.R1 - elementPost.R1),
    (elemenPre.R2 - elementPost.R2)));
                }
            }
        }
    }
}
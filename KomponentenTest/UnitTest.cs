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

namespace KomponentenTest
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
        public void ImportTestTRA(string Filename)
        {
            TrassenElement[] TEST = Trassierung.ImportTRA(Filename);
            double[] X = new double[TEST.Length];
            double[] Y = new double[TEST.Length];
            double[] Xinterp = new double[0];
            double[] Yinterp = new double[0];
            int[] kz = new int[0];
            int i = 0;
            foreach (TrassenElement T in TEST){
                T.Interpolate();  
                //T.print(); 
                X[i] = T.Xstart; 
                Y[i] = T.Ystart;
                int[] filledArray = new int[T.InterpX.Length];
                Array.Fill(filledArray, T.Kz);
                kz = kz.Concat(filledArray).ToArray();
                Xinterp = Xinterp.Concat(T.InterpX).ToArray();
                Yinterp = Yinterp.Concat(T.InterpY).ToArray();
                i++; }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(TestContext.TestDir, @"..\CoordinatesOut.txt")))
            {
                for (int j = 0; j < Xinterp.Length; j++)
                {
                    outputFile.WriteLine(Xinterp[j].ToString(CultureInfo.InvariantCulture) + " " + Yinterp[j].ToString(CultureInfo.InvariantCulture) + " " + kz[j]);
                }
            }
        }
        [TestMethod]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240045L.GRA",DisplayName = "6240045L.GRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\6240045R.GRA", DisplayName = "6240045R.GRA")]
        [DataRow("C:\\HTW\\Trassierung\\Infos\\Gradiente.GRA", DisplayName = "Gradiente.GRA")]
        public void ImportTestGRA(string Filename)
        {
            (GradientElement[] TEST,GleisscherenElement[] TEST2) = Trassierung.ImportGRA(Filename);
            foreach (GradientElement T in TEST) { T.print(); }
        }
        [TestMethod]
        [DataRow(100*Math.PI,100,0,200,DisplayName = "half circle right")]
        [DataRow(100 * Math.PI, -100, 0, -200, DisplayName = "half circle left")]
        [DataRow(100 * Math.PI *0.5, 100, 100, 100, DisplayName = "quater circle right")]
        [DataRow(100 * Math.PI *0.5, -100, 100, -100, DisplayName = "quater circle left")]
        public void InterpolationTestCircle(double input, double radius, double expectedX, double expectedY)
        {
            double[] Xinterp = new double[(int)input];
            double[] Yinterp = new double[(int)input];
            double Z;
            for (int i = 0; i < (int)input; i++)
            {
                (Xinterp[i], Yinterp[i],Z) = InterpolationClass.PointOnCircle(i, radius);

            }
            (double X, double Y,Z) = InterpolationClass.PointOnCircle(input, radius);
            Assert.AreEqual(expectedX, X, 0.0000001, "Circle Interpolation returned wrong X-coordinate");
            Assert.AreEqual(expectedY, Y, 0.0000001, "Circle Interpolation returned wrong Y-coordinate");
        }
        [TestMethod]
        [DataRow(100, 500,50, 100, DisplayName = "Clothoid curvature increase")]
        [DataRow(100, 50, 500, 100, DisplayName = "Clothoid curvature decrease")]
        [DataRow(64.81533, 3221.20091, 1573.91942, 64.81533, DisplayName = "Clothoid curvature decrease2")]
        public void InterpolationTestClothoid(double input, double radius1, double radius2, double length)
        {
            double[] Xinterp = new double[(int)input];
            double[] Yinterp = new double[(int)input];
            double Z;
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(TestContext.TestDir, @"..\CoordinatesOut.txt")))
            {
                for (int i = 0; i < (int)input; i++)
                {
                    (Xinterp[i], Yinterp[i], Z) = InterpolationClass.PointOnClothoid(i, radius1, radius2, length);
                    Debug.WriteLine("Clothoid: " + Xinterp[i] + " " + Yinterp[i]);
                    outputFile.WriteLine(Xinterp[i].ToString(CultureInfo.InvariantCulture) + " " + Yinterp[i].ToString(CultureInfo.InvariantCulture));
                }
            }
            (double X, double Y, Z) = InterpolationClass.PointOnClothoid(input, radius1, radius2, length);
        }
        [TestMethod]
        [DataRow(10.0, DisplayName = "Fresnel")]
        public void CalculationTestFresnel(double input)
        {
            double stepsize = 0.1;
            double[] Xinterp = new double[(int)(input/stepsize)+1];
            double[] Yinterp = new double[(int)(input/stepsize)+1];
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(TestContext.TestDir, @"..\CoordinatesOut.txt")))
            {
                int i = 0;
                for (double x = 0; x < input; x = x + stepsize)
                {
                    (Xinterp[i], Yinterp[i]) = InterpolationClass.CalculateFresnel(x);
                    Debug.WriteLine("Fresnel: " + Xinterp[i] + " " + Yinterp[i]);
                    outputFile.WriteLine(Xinterp[i].ToString(CultureInfo.InvariantCulture) + " " + Yinterp[i].ToString(CultureInfo.InvariantCulture));
                    i++;
                }
            }       
        }
    }
    


}
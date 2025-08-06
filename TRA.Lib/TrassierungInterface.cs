using Microsoft.Extensions.Logging;
using netDxf.Tables;
using netDxf;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Xml.Linq;
using static netDxf.Entities.HatchBoundaryPath;


namespace TRA_Lib
{
    public static class TrassierungLog
    {
        private static ILoggerFactory _loggerFactory;
        private static readonly ConcurrentQueue<Tuple<LogLevel,string,object>> _logQueue = new ConcurrentQueue<Tuple<LogLevel, string,object>>();
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private static Task _logTask;

        public static void AssignLogger(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            Logger = _loggerFactory.CreateLogger("TrassierungLogger");
            _logTask = Task.Run(() => ProcessLogQueue(), _cts.Token);
        }
        public static ILogger Logger
        {
            get; private set;
        }

        //public static void Log(string message)
        //{
        //    var logEntry = $"{DateTime.Now}: {message}";
        //    _logQueue.Enqueue(logEntry);
        //}

        public static void Log_Async(this ILogger logger,LogLevel logLevel, string? message, params object?[] args)
        {
            _logQueue.Enqueue(new Tuple<LogLevel,string,object>(logLevel,message,args));
        }

        private static void ProcessLogQueue()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (_logQueue.TryDequeue(out var logEntry))
                {
                    Logger.Log(logEntry.Item1,logEntry.Item2,logEntry.Item3);
                }
                else
                {
                    Thread.Sleep(100); // Adjust the sleep time as needed
                }
            }
        }
        public static void Dispose()
        {
            _cts.Cancel();
            _logTask.Wait();
        }
    }
    /// <summary>
    /// Class providing import and export functionality
    /// </summary>
    public class Trassierung
    {
        //config from settings.json
        static Trassierung self;
        //Tolerance for Station-values; the Station + Length is compared to the successor Station
        public static readonly double StationMismatchTolerance;
        //The last point(L)-Position of the interpolation is compared to the successor element coordinates of the TRA-File.A euclidean distance is used for comparison.[m]
        public static readonly double ConnectivityMismatchTolerance;
        //The last point(L)-Heading of the interpolation is compared to the successor element heading of the TRA-File.[rad]
        public static readonly double ContinuityOfHeadingTolerance;
        //The last point(L)-Curvature of the interpolation is compared to the successor element curvature of the TRA-File.[1/m]
        public static readonly double ContinuityOfCurvatureTolerance;

        static Trassierung()
        {
            if (File.Exists("settings.json"))
            {
                Dictionary<string, double> config = JsonSerializer.Deserialize<Dictionary<string, double>>(File.ReadAllText("settings.json"));
                if (config == null)
                {
                    throw new FileNotFoundException("settings.json not found");
                }
                else
                {
                    StationMismatchTolerance = config["StationMismatchTolerance"];
                    ConnectivityMismatchTolerance = config["ConnectivityMismatchTolerance"];
                    ContinuityOfHeadingTolerance = config["ContinuityOfHeadingTolerance"];
                    ContinuityOfCurvatureTolerance = config["ContinuityOfCurvatureTolerance"];
                }
            }
            else
            {
                StationMismatchTolerance = 1e-8;
                ConnectivityMismatchTolerance = 1e-8;
                ContinuityOfHeadingTolerance = 1e-8;
                ContinuityOfCurvatureTolerance = 1e-8;
            }
        }
        public static TRATrasse ImportTRA(string fileName)
        {
            if(self == null) self = new Trassierung();
            if (File.Exists(fileName))
            {
                using (var stream = File.Open(fileName, FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                    {
                        int num;
                        // 0-Element:                       
                        reader.ReadDouble();
                        reader.ReadDouble();
                        reader.ReadDouble();
                        reader.ReadDouble();
                        reader.ReadDouble();
                        reader.ReadDouble();
                        num = reader.ReadInt16();
                        reader.ReadDouble();
                        reader.ReadDouble();
                        reader.ReadDouble();
                        reader.ReadSingle();

                        //Search for existing trasse
                        TRATrasse trasse = (TRATrasse)Trasse.LoadedTrassen.Find(n => n.Filename.Equals(Path.GetFileName(fileName)));// Split('.')[0]));
                        if (trasse != null)
                        {
                            TrassierungLog.Logger?.LogInformation("A existing TRA-Trasse was found, existing TRA Data is overwritten", nameof(trasse));
                        }
                        else
                        {
                            trasse = new TRATrasse();
                            trasse.Filename = Path.GetFileName(fileName);
                        }

                        trasse.Elemente = new TrassenElementExt[num + 1];
                        TrassenElementExt predecessor = null;
                        for (int i = 0; i < num + 1; i++)
                        {
                            trasse.Elemente[i] = new TrassenElementExt(
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadInt16(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadSingle(),
                            i + 1,
                            trasse,
                            predecessor
                            );
                            predecessor = trasse.Elemente[i];
                        }
                        if (reader.BaseStream.Position != reader.BaseStream.Length) { throw new SerializationException("End of Bytestream was not reached"); }
                        return trasse;
                    }
                }
            }
            return new TRATrasse();
        }
        public enum ESaveScale
        {
            //The scale is ignored, the length retains its original value. This leads to coordinate differences between the geometry elements. The condition station value + L == subsequent element station value is satisfied.
            discard = 0,
            //The length is multiplied by scale. The station values of the elements are not adjusted. The condition station value + L == subsequent element.station value is NOT met.
            multiply = 1,
            //The length is multiplied by the scale. To satisfy the condition Stationswert + L == Folgeelement.Stationswert, additional KSprung elements are added.
            asKSprung = 2,
        }
        public static void ExportTRA(TRATrasse trasse, string fileName,ESaveScale saveScale = ESaveScale.discard)
        {
            List<TrassenElementExt> elements = trasse.Elemente.ToList();
            if (saveScale == ESaveScale.asKSprung)
            {
                for (int i = 0; i < elements.Count-1; i++)
                {
                    if (elements[i].Scale != 1.0 && !double.IsNaN(elements[i].Scale))
                    {
                        double deltaL = elements[i].L * (elements[i].Scale -1);
                        //First check if the following Element is already a KSprung
                        if (elements[i].Successor.GetGeometryType() == typeof(KSprung))
                        {   
                            //Adding delta from scale to Length
                            elements[i].Successor.L -= deltaL;
                            //And move Station 
                            elements[i].Successor.S += deltaL;
                        }
                        else
                        {
                            //Create a new K-Sprung element to solve length difference
                            elements.Insert(i + 1, new TrassenElementExt(0, 0,
                                elements[i].Yend,
                                elements[i].Xend,
                                elements[i].Successor != null ? elements[i].Successor.T : 0,
                                elements[i].S + elements[i].L * elements[i].Scale,
                                (int)Trassenkennzeichen.KSprung,
                                -deltaL,
                                0, 0,
                                elements[i].Cf,
                                elements[i].ID,
                                elements[i].owner));
                        }
                        i++; // Adjust index to avoid looping over the inserted element
                    }
                }
            }
            try
            {
                using (FileStream fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new BinaryWriter(fileStream, Encoding.UTF8, false))
                    {
                        //Write 0-Element
                        writer.Write((double)0); 
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((short)(elements.Count - 1));
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((float)0);

                        foreach (TrassenElementExt element in elements)
                        {
                            writer.Write(element.R1);
                            writer.Write(element.R2);
                            writer.Write(element.Ystart);
                            writer.Write(element.Xstart);
                            writer.Write(element.T);
                            writer.Write(element.S);
                            writer.Write((short)element.Kz);
                            if (double.IsNaN(element.Scale))
                            {
                                writer.Write((double)0.0); //set 0 if scale was NaN (should only occur for the closing element (last) as there no scale is defined. We set length to zero.
                            }
                            else
                            { 
                                writer.Write(saveScale == ESaveScale.discard ? element.L : element.L * element.Scale); //if discarding scale we set the original length, otherwise apply scale. 
                            }
                            writer.Write(element.U1);
                            writer.Write(element.U2);
                            writer.Write((float)element.Cf);
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Can not write to File " + fileName);
            }
        }
        public static void ExportTRA_CSV(TRATrasse trasse, string fileName)
        {
            try
            {
                using (FileStream fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    CultureInfo info = CultureInfo.CurrentCulture;
                    string[] titles = { "R1[1/m]", "R2[1/m]", "Y[m]", "X[m]", "T[rad]", "S[m]", "Kz", "L[m]", "U1", "U2", "C", "H[m]", "s[1/m]", "Scale", "Deviation", "Warnings" };
                    writer.WriteLine(string.Join(info.TextInfo.ListSeparator, titles));
                    foreach (TrassenElementExt ele in trasse.Elemente)
                    {
                        writer.WriteLine(ele.ToString());
                        Interpolation interp = ele.InterpolationResult;
                        if (interp.Y != null)
                        {
                            for (int i = 0; i < ele.InterpolationResult.Y.Length; i++)
                            {
                                string[] values = { "", "", interp.Y[i].ToString(info), interp.X[i].ToString(info), interp.T[i].ToString(info), interp.S[i].ToString(info), interp.K[i].ToString(info) };
                                if (interp.H != null)
                                {
                                    values = values.Concat(new string[] { "", "", "", "", interp.H[i].ToString(info), interp.s[i].ToString(info) }).ToArray();
                                }
                                else
                                {
                                    values = values.Concat(new string[] { "", "", "", "", "", "" }).ToArray();
                                }
                                writer.WriteLine(string.Join(info.TextInfo.ListSeparator, values));
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Can not write to File " + fileName);
            }
        }

        public static void ExportTRA_DXF(TRATrasse trasse, string fileName)
        {
            // Create a new DXF document
            DxfDocument dxf = new DxfDocument();

            foreach (TrassenElementExt element in trasse.Elemente)
            {
                if (element.GetGeometryType() == typeof(KSprung))
                {
                    // Skip KSprung elements for DXF export
                    continue;
                }
                netDxf.Entities.EntityObject entitiy;
                Interpolation interp = element.InterpolationResult;
                if (interp.H != null)
                {
                    List<Vector3> points = new List<Vector3>
                    {
                        new Vector3(element.Ystart,element.Xstart,interp.H[0])
                    };
                    for (int i = 0; i < interp.X.Length; i++)
                    {
                        if (!double.IsNaN(interp.X[i]) && !double.IsNaN(interp.Y[i]) && !double.IsNaN(interp.H[i])) points.Add(new Vector3(interp.Y[i], interp.X[i], interp.H[i]));
                    }
                    entitiy = new netDxf.Entities.Polyline3D(points);
                }
                else
                {
                    List<Vector2> points = new List<Vector2>
                    {
                        new Vector2(element.Ystart,element.Xstart)
                    };
                    for (int i = 0; i < interp.X.Length; i++)
                    {
                        if (!double.IsNaN(interp.X[i]) && !double.IsNaN(interp.Y[i])) points.Add(new Vector2(interp.Y[i], interp.X[i]));
                    }
                    entitiy = new netDxf.Entities.Polyline2D(points);
                }
                entitiy.Layer = new Layer(trasse.Filename);
                entitiy.Linetype = Linetype.Continuous;

                // Add polyline to the DXF document
                dxf.Entities.Add(entitiy);
            }
            //In case of a VA CRS we add an insert point 
            if(trasse.CRS_Name.StartsWith("VA"))
            {
                dxf.Entities.Add(new netDxf.Entities.Polyline2D(new[]
                    {
                        new Vector2(5000, 10000),
                        new Vector2(5000+5, 10000),
                    }));
                dxf.Entities.Add(new netDxf.Entities.Polyline2D(new[]
                    {
                        new Vector2(5000, 10000),
                        new Vector2(5000, 10000+5),
                    }));
                dxf.Entities.Add(new netDxf.Entities.Polyline2D(new[]
                    {
                        new Vector2(5000, 10000),
                        new Vector2(5000-5, 10000),
                    }));
                dxf.Entities.Add(new netDxf.Entities.Polyline2D(new[]
                    {
                        new Vector2(5000, 10000),
                        new Vector2(5000, 10000-5),
                    }));
            }
            // Save the DXF file
            dxf.Save(fileName);
        }

        public static GRATrasse ImportGRA(string fileName)
        {
            if (self == null) self = new Trassierung();
            if (File.Exists(fileName))
            {
                using (var stream = File.Open(fileName, FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                    {
                        int num_NW = 0;
                        int num_GS = 0;
                        // 0-Element:                       
                        num_NW = (int)reader.ReadDouble();
                        reader.ReadDouble();
                        reader.ReadDouble();
                        reader.ReadDouble();
                        num_GS = reader.ReadInt32();

                        //Search for existing trasse
                        GRATrasse trasse = (GRATrasse)Trasse.LoadedTrassen.Find(n => n.Filename.Equals(Path.GetFileName(fileName).Split('.')[0]));
                        if (trasse != null)
                        {
                            TrassierungLog.Logger?.LogInformation("A existing Trasse was found: " + (trasse is TRATrasse ? "GRA Data is added to TRATrasse, " : "") + "existing GRA Data is overwritten", nameof(trasse));
                        }
                        else
                        {
                            trasse = new GRATrasse();
                            trasse.Filename = Path.GetFileName(fileName);
                        }

                        trasse.GradientenElemente = new GradientElementExt[num_NW];
                        GradientElementExt predecessor = null;
                        for (int i = 0; i < num_NW; i++)
                        {
                            trasse.GradientenElemente[i] = new GradientElementExt(
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadInt32(),
                            i + 1,
                            trasse,
                            predecessor
                            );
                            predecessor = trasse.GradientenElemente[i];
                        }
                        trasse.GleisscherenElemente = new GleisscherenElement[num_GS];
                        for (int i = 0; i < num_GS; i++)
                        {
                            trasse.GleisscherenElemente[i] = new GleisscherenElement(
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadInt32()
                            );
                        }
                        if (reader.BaseStream.Position != reader.BaseStream.Length) { throw new SerializationException("End of Bytestream was not reached"); }
                        return trasse;
                    }
                }
            }
            return null;
        }
        public static void ExportGRA(GRATrasse trasse, string fileName)
        {
            try
            {
                using (FileStream fileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new BinaryWriter(fileStream, Encoding.UTF8, false))
                    {
                        //Write 0-Element
                        writer.Write((double)trasse.GradientenElemente.Length);
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((int)trasse.GleisscherenElemente.Length);

                        foreach (GradientElementExt element in trasse.GradientenElemente)
                        {
                            writer.Write(element.S);
                            writer.Write(element.H);
                            writer.Write(element.R);
                            writer.Write(element.T);
                            writer.Write((int)element.Pkt);
                        }
                        foreach (GleisscherenElement element in trasse.GleisscherenElemente)
                        {
                            writer.Write(element.RE1);
                            writer.Write(element.RA);
                            writer.Write(element.RE2);
                            if(element.Kz == Trassenkennzeichen.UB_S_Form ||
                                element.Kz == Trassenkennzeichen.Bloss)
                            {
                                writer.Write(element.Ueberhoeung1 + (int)element.Kz * 1000);
                            }
                            else
                            {
                                writer.Write(element.Ueberhoeung1);
                            }
                            writer.Write((int)(element.Ueberhoeung2 /10));
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Can not write to File " + fileName);
            }
        }
    }

}
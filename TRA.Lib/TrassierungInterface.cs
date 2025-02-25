using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace TRA_Lib
{
    public static class TrassierungLog
    {
        private static ILoggerFactory _loggerFactory;
        public static void AssignLogger(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            Logger = _loggerFactory.CreateLogger("TrassierungLogger");
        }
        public static ILogger Logger
        {
            get; private set;
        }
    }

    public class Trassierung
    {
        public static TRATrasse ImportTRA(string fileName)
        {
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
                        TRATrasse trasse = (TRATrasse)Trasse.LoadedTrassen.Find(n => n.Filename.Contains(Path.GetFileName(fileName)));// Split('.')[0]));
                        if (trasse != null)
                        {
                            TrassierungLog.Logger?.LogInformation("A existing TRA-Trasse was found, esisting TRA Data is overwritten", nameof(trasse));
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
        public static void ExportTRA(TRATrasse trasse, string fileName)
        {
            try
            {
                using (FileStream fileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
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
                        writer.Write((short)trasse.Elemente.Length);
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((double)0);
                        writer.Write((float)0);

                        foreach (TrassenElementExt element in trasse.Elemente)
                        {
                            writer.Write(element.R1);
                            writer.Write(element.R2);
                            writer.Write(element.Ystart);
                            writer.Write(element.Xstart);
                            writer.Write(element.T);
                            writer.Write(element.S);
                            writer.Write((short)element.Kz);
                            writer.Write(element.L);
                            writer.Write(element.U1);
                            writer.Write(element.U2);
                            writer.Write(element.Cf);
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
                using (FileStream fileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    CultureInfo info = CultureInfo.CurrentCulture;
                    string[] titles = { "R1", "R2", "Y", "X", "T", "S", "Kz", "L", "U1", "U2", "C", "H", "s", "Deviation", "Warnings" };
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

        public static (GRATrasse, GleisscherenElement[]) ImportGRA(string fileName)
        {
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
                        GRATrasse trasse = (GRATrasse)Trasse.LoadedTrassen.Find(n => n.Filename.Contains(Path.GetFileName(fileName).Split('.')[0]));
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
                        GleisscherenElement[] gleisscheren = new GleisscherenElement[num_GS];
                        for (int i = 0; i < num_GS; i++)
                        {
                            gleisscheren[i] = new GleisscherenElement(
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadInt32()
                            );
                        }
                        if (reader.BaseStream.Position != reader.BaseStream.Length) { throw new SerializationException("End of Bytestream was not reached"); }
                        return (trasse, gleisscheren);
                    }
                }
            }
            return (null, new GleisscherenElement[0]);
        }
        public static void ExportGRA(GradientElement[] gradient, GleisscherenElement[] gleisschere, string fileName)
        {
        }
    }

}
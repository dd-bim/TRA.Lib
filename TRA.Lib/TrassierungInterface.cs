using Microsoft.Extensions.Logging;
using System.Runtime.Serialization;
using System.Text;

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
        public static void ExportTRA(TrassenElement[] trasse, string fileName)
        {

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
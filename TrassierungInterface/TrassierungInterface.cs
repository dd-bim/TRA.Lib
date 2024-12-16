using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TrassierungInterface
{
    public static class TrassierungLog
    {
        private static ILoggerFactory _loggerFactory; 
        public static void Init(ILoggerFactory loggerFactory)
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
        public static Trasse ImportTRA(string fileName)
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

                        Trasse trasse = new Trasse();
                        trasse.Filename = Path.GetFileName(fileName);
                        trasse.Elemente = new TrassenElement[num + 1];
                        TrassenElement predecessor = null;
                        for (int i = 0; i < num + 1; i++)
                        {
                            trasse.Elemente[i] = new TrassenElement(
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
                            i+1,
                            predecessor
                            );
                            predecessor = trasse.Elemente[i];
                        }
                        if(reader.BaseStream.Position != reader.BaseStream.Length) { throw new SerializationException("End of Bytestream was not reached"); }
                        return trasse;
                    }
                }
            }
            return new Trasse();
        }
        public static void ExportTRA(TrassenElement[] trasse, string fileName)
        {
            
        }

        public static (GradientElement[], GleisscherenElement[]) ImportGRA(string fileName)
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

                        GradientElement[] gradienten = new GradientElement[num_NW];
                        for (int i = 0; i < num_NW; i++)
                        {
                            gradienten[i] = new GradientElement(
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadDouble(),
                            reader.ReadInt32()
                            );
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
                        return (gradienten, gleisscheren);
                    }
                }
            }
            return (new GradientElement[0],new GleisscherenElement[0]);
        }
        public static void ExportGRA(GradientElement[] gradient, GleisscherenElement[] gleisschere, string fileName)
        {
        }
    }

}
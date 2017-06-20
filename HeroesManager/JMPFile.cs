using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HeroesManager
{
    // Yes, this is all copied from LoL-Fantome ツ
    class JMPFile
    {
        public byte[] Bytes { get; private set; }
        public String Version { get; private set; }
        public Int32 FileCount { get; private set; }
        public List<JMPEntry> Files { get; private set; } = new List<JMPEntry>();
        public String Output { get; set; }
        
        public JMPFile(string Location, string Output)
        {
            this.Output = Output;
            using (BinaryReader br = new BinaryReader(File.OpenRead(Location)))
            {
                string Magic = Encoding.ASCII.GetString(br.ReadBytes(4));
                if(Magic != "DATA")
                {
                    // Should check for HEX but nah ツ
                    Console.WriteLine("No valid DATA.JMP file");
                }
                Version = Encoding.UTF8.GetString(br.ReadBytes(4), 0, 4);
                br.BaseStream.Seek(0x32, SeekOrigin.Begin);
                FileCount = br.ReadInt32();
                br.BaseStream.Seek(0x36, SeekOrigin.Begin);
                for (int i = 0; i < FileCount; i++)
                {
                    var entry = new JMPEntry()
                    {
                        Name = Encoding.UTF8.GetString(br.ReadBytes(0x104), 0, 0x104).Replace("\0", string.Empty),
                        Offset = br.ReadInt32(),
                        Zsize = br.ReadInt32(),
                        Size = br.ReadInt32(),
                        Null = Encoding.UTF8.GetString(br.ReadBytes(0x20), 0, 0x20)
                    };
                    Files.Add(entry);
                }
                Extract(br);
            }
        }

        private void Write(BinaryWriter bw)
        {
            // Not working atm
            bw.Write(0x41544144);
            bw.Write(Encoding.ASCII.GetBytes(Version));
            bw.BaseStream.Seek(0x32, SeekOrigin.Begin);
            bw.Write(FileCount);
            bw.BaseStream.Seek(0x36, SeekOrigin.Begin);
            foreach (JMPEntry f in Files)
            {
                f.Write(bw);
            }
            Console.WriteLine("FINISHED");
        }

        public void Extract(BinaryReader br)
        {
            // Get the byte and decompress it with zlib
            foreach (JMPEntry f in Files)
            {
                br.BaseStream.Seek(f.Offset, SeekOrigin.Begin);
                var decompressedFile = Ionic.Zlib.ZlibStream.UncompressBuffer(br.ReadBytes(f.Zsize));
                string fileName = f.Name.Substring(2);
                Helper.ByteArrayToFile(Output + "\\" + fileName, fileName, decompressedFile);
            }
            Console.WriteLine("Finished extracing...");
        }

    }
}

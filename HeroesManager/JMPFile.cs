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
        public String Location { get; set; }
        public String Output { get; set; }
        
        public JMPFile(string Location, string Output)
        {
            this.Location = Location;
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

        public JMPFile(string BuildDirectory)
        {
            // We are building a complete new Data.jmp from scratch
            foreach (string file in Helper.GetFiles(BuildDirectory))
            {
                var path = file.Replace(BuildDirectory, "..");
                var fileAsByte = File.ReadAllBytes(file);
                JMPEntry newEntry = new JMPEntry()
                {
                    Name = path,
                    Path = file,
                    Offset = 0,
                    Size = fileAsByte.Count(),
                    compressedFile = Ionic.Zlib.ZlibStream.CompressBuffer(fileAsByte)
                };
                fileAsByte = null;
                Files.Add(newEntry);
                Console.WriteLine("Registered File: " + path);
            }
            using (BinaryWriter bw = new BinaryWriter(File.Create(BuildDirectory + "\\Data_compiled.jmp")))
            {
                bw.Write(0x41544144);
                bw.Write(Encoding.ASCII.GetBytes("1.0")); // never was anything other than 1.0
                bw.BaseStream.Seek(0x32, SeekOrigin.Begin);
                bw.Write(Files.Count);
                bw.BaseStream.Seek(0x36, SeekOrigin.Begin);
                foreach (JMPEntry f in Files)
                {
                    Console.WriteLine("Writing Meta Info of : " + f.Name);
                    f.Write(bw);
                }
                foreach (JMPEntry f in Files)
                {
                    Console.WriteLine("Writing Binary Info of : " + f.Name);
                    Int32 currentPosition = Convert.ToInt32(bw.BaseStream.Position);
                    // Go back and write Offset
                    bw.BaseStream.Seek(f.BuildOffset, SeekOrigin.Begin);
                    bw.Write(currentPosition);
                    // Go now write compressed bytes
                    bw.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
                    bw.Write(f.compressedFile);
                    // Empty that shit
                    f.compressedFile = null;
                }
                Console.WriteLine("Finished repacking...");
            }
        }

        private void Write(BinaryWriter bw)
        {
            bw.Write(0x41544144);
            bw.BaseStream.Seek(0x32, SeekOrigin.Begin);
            bw.Write(FileCount);
            bw.BaseStream.Seek(0x36, SeekOrigin.Begin);
            foreach (JMPEntry f in Files)
            {
                f.Write(bw);
            }
            foreach (JMPEntry f in Files)
            {
                bw.BaseStream.Seek(f.Offset, SeekOrigin.Begin);
                bw.Write(f.compressedFile);
            }
            Console.WriteLine("Finished repacking...");
        }

        public void Extract(BinaryReader br)
        {
            // Get the byte and decompress it with zlib
            foreach (JMPEntry f in Files)
            {
                br.BaseStream.Seek(f.Offset, SeekOrigin.Begin);
                f.compressedFile = br.ReadBytes(f.Zsize);
                var decompressedFile = Ionic.Zlib.ZlibStream.UncompressBuffer(f.compressedFile);
                string fileName = f.Name.Substring(2);
                Helper.ByteArrayToFile(Output + "\\" + fileName, fileName, decompressedFile);
            }

            Console.WriteLine("Finished extracing...");
        }

    }
}

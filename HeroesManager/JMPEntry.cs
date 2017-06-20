using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroesManager
{
    class JMPEntry
    {
        public String Name { get; set; }
        public Int32 Offset { get; set; }
        public Int64 BuildOffset { get; set; }
        public Int32 Zsize { get; set; }
        public Int32 Size { get; set; }
        public String Null { get; set; }
        public String Path { get; set; }
        public byte[] compressedFile { get; set; }

        public void Write(BinaryWriter bw)
        {
            byte[] name = Encoding.ASCII.GetBytes(Name);
            int i1 = 0x104;
            foreach (byte b in name)
            {
                bw.Write(b);
                i1--;
            }
            for (int i = 0; i < i1; i++)
            {
                bw.Write((byte)0);
            }
            BuildOffset = bw.BaseStream.Position;
            bw.Write(Offset);
            bw.Write(compressedFile.Count());
            bw.Write(Size);
            // we don't need uncompressedFile anymore, so empty it
            for (int i = 0; i < 0x20; i++)
            {
                bw.Write((byte)0);
            }
        }
    }
}

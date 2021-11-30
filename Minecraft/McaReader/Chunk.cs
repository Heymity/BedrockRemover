/*
 * Original code made by mblaine in his project Topographer (https://github.com/mblaine/Topographer) (MIT License), with some modifications made by me (Heymity)
 */

using System;
using System.Text;
using Cyotek.Data.Nbt;

namespace RemoveBedrock.Minecraft.McaReader
{
    public class Chunk
    {
        public NbtDocument NbtData;
        public Coord Coords;
        public int Timestamp;
        public bool Dirty = false;
        public byte CompressionType;
        public byte[] RawData = null;

        public TagCompound Root => NbtData?.DocumentRoot;
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            var time = new DateTime(1970, 1, 1).AddSeconds(Timestamp);

            sb.AppendFormat("Chunk [{0}, {1}] {2:M/d/yyyy h:mm:ss tt}{3}{{{3}", Coords.X, Coords.Z, time, Environment.NewLine);
            if (Root != null)
                sb.Append(Root);
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
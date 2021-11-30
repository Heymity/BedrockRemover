using System;
using System.Text;
using Cyotek.Data.Nbt;


namespace RemoveYZeroBedrock.Minecraft.McaReader
{
    public class Chunk
    {
        //public TAG_Compound Root;
        public TagCompound Root;
        public Coord Coords;
        public Int32 Timestamp;
        public bool Dirty = false;
        public byte CompressionType;
        public byte[] RawData = null;

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            DateTime time = new DateTime(1970, 1, 1).AddSeconds(Timestamp);

            sb.AppendFormat("Chunk [{0}, {1}] {2:M/d/yyyy h:mm:ss tt}{3}{{{3}", Coords.X, Coords.Z, time, Environment.NewLine);
            if (Root != null)
                sb.Append(Root);
            sb.AppendLine("}");
            return sb.ToString();
        }

        public void ChangeBedrockToDeepslate()
        {
            
        }
    }
}
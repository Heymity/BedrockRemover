using System;

namespace RemoveBedrock.Minecraft.McaReader
{
    public struct Coord
    {
        public int X;
        public int Z;

        public Coord(int x, int z)
        {
            X = x;
            Z = z;
        }

        public Coord(Coord c)
        {
            X = c.X;
            Z = c.Z;
        }

        public void Add(int dx, int dz)
        {
            X += dx;
            Z += dz;
        }

        public void ChunkToAbsolute()
        {
            X *= 16;
            Z *= 16;
        }

        public void AbsoluteToChunk()
        {
            X = (int)Math.Floor(X / 16.0);
            Z = (int)Math.Floor(Z / 16.0);
        }

        public void RegionToAbsolute()
        {
            X = X * 16 * 32;
            Z = Z * 16 * 32;
        }

        public void AbsoluteToRegion()
        {
            X = (int)Math.Floor(X / 32.0 / 16.0);
            Z = (int)Math.Floor(Z / 32.0 / 16.0);
        }

        public void RegionToChunk()
        {
            X *= 32;
            Z *= 32;
        }

        public void ChunkToRegion()
        {
            X = (int)Math.Floor(X / 32.0);
            Z = (int)Math.Floor(Z / 32.0);
        }

        //chunk coordinates within region, between (0, 0) and (31, 31)
        public void ChunkToRegionRelative()
        {
            X %= 32;
            if (X < 0)
                X += 32;
            Z %= 32;
            if (Z < 0)
                Z += 32;
        }

        public override string ToString()
        {
            return $"x:{X};z:{Z}";
        }
    }
}
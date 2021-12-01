using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RemoveBedrock.Minecraft.McaReader;

namespace RemoveBedrock.BedrockRemoval
{
    public static class RegionHandler
    {
        private const string Bedrock = "minecraft:bedrock";
        private const string Deepslate = "minecraft:deepslate";

        private static void RemoveBedrockAndSave(this RegionFile region, Coord min, Coord max)
        {
            Console.WriteLine($"Removing Bedrock in region {region.Coords}");
            //region.ToList().ForEach(chunk => chunk.ChangeChunkPaletteBlock(Bedrock, Deepslate));
            Parallel.ForEach(region, chunk =>
            {
                var c = chunk.Coords;
                if (c.X < min.X || c.Z < min.Z || c.X > max.X || c.Z > max.Z) return;

                chunk.ChangeChunkPaletteBlock(Bedrock, Deepslate);
            });
            region.SetDirty();
            
            region.Write();
            
            GC.Collect();
        }

        private static IEnumerable<RegionFile> LoadRegionsInDirectory(string dir, Coord min, Coord max)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"The directory {dir} was not found by the program.");

            var filePaths = Directory.GetFiles(dir);

            return from filePath in filePaths
                where Path.GetExtension(filePath) == ".mca" && InRegionOnRange(filePath, min, max)
                select new RegionFile(filePath);
        }

        private static bool InRegionOnRange(string filePath, Coord min, Coord max)
        {
            var identifiers = Path.GetFileName(filePath).Split('.');

            var x = int.Parse(identifiers[1]);
            var z = int.Parse(identifiers[2]);
            
            min.ChunkToRegion();
            max.ChunkToRegion();

            return x >= min.X && z >= min.Z && x <= max.X && z <= max.Z;
        }

        public static void RemoveBedrockForDirectory(string dir, Coord min, Coord max)
        {
            //LoadRegionsInDirectory(dir).ToList().ForEach(reg => reg.RemoveBedrockAndSave());
            Parallel.ForEach(LoadRegionsInDirectory(dir, min, max), reg => reg.RemoveBedrockAndSave(min, max));
        }
    }
}
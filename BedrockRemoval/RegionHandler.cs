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

        private static void RemoveBedrockAndSave(this RegionFile region)
        {
            Console.WriteLine($"Removing Bedrock in region {region.Coords}");
            //region.ToList().ForEach(chunk => chunk.ChangeChunkPaletteBlock(Bedrock, Deepslate));
            Parallel.ForEach(region, chunk => chunk.ChangeChunkPaletteBlock(Bedrock, Deepslate));
            region.SetDirty();
            
            region.Write();
            
            GC.Collect();
        }

        private static IEnumerable<RegionFile> LoadRegionsInDirectory(string dir)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"The directory {dir} was not found by the program.");

            var filePaths = Directory.GetFiles(dir);

            return from filePath in filePaths where Path.GetExtension(filePath) == ".mca" select new RegionFile(filePath);
        }

        public static void RemoveBedrockForDirectory(string dir, Coord? min = null, Coord? max = null)
        {
            //LoadRegionsInDirectory(dir).ToList().ForEach(reg => reg.RemoveBedrockAndSave());
            Parallel.ForEach(LoadRegionsInDirectory(dir), reg => reg.RemoveBedrockAndSave());
        }
    }
}
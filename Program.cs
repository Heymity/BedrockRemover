using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Cyotek.Data.Nbt;

namespace RemoveYZeroBedrock
{
    using Minecraft.McaReader;
    
    class Program
    {
        private const string ProgramPath =
            @"C:\Users\GABRIEL\Desktop\LangFiles\C#\NBTManipulation\RemoveYZeroBedrock\RemoveYZeroBedrock";
        
        static void Main(string[] args)
        {
            var reg = LoadRegionFile(ProgramPath + @"\test\input\r.0.-1.mca");
            Console.WriteLine(reg);
            reg.ToList().ForEach(HandleChunk);
            // Can be done in parallel.
            //Parallel.ForEach(reg, HandleChunk);
        }
        
        private static void HandleChunk(Chunk chunk)
        {
            // Should probably be moved to another class to allow more versatility. Specially in the block to be replaced.
            
            var root = chunk.Root;
            if (root is null)
            {
                Console.WriteLine($"Chunk {chunk.Coords} is empty");
                return;
            }
            
            Console.WriteLine($"Querying palette of chunk {chunk.Coords}");
            //var query = root.Query<TagCompound>("Level").Query<TagList>("Sections");
            var level = (TagCompound)root["Level"];
            var sections = (TagList)level["Sections"];

            var layerZero = sections.Value.Cast<TagCompound>().First(tag => ((TagByte)tag["Y"]).Value == 0);

            var palette = layerZero.Query<TagList>("Palette");

            Console.WriteLine($"Changing palette");

            var bedrockTag = palette.Value.Cast<TagCompound>().Select(tag => (TagString)tag["Name"])
                .FirstOrDefault(tag => tag.Value == "minecraft:bedrock");

            if (bedrockTag is null)
            {
                Console.WriteLine($"No bedrock found for chunk {chunk.Coords}");
                return;
            }
            
            Console.WriteLine($"Substituting bedrock");

            bedrockTag.Value = "minecraft:deepslate";
            
            Console.WriteLine($"Chunk {chunk.Coords} done!");

            // I think this line is needed for it to be saved, need to check the region.Write() function.
            // chunk.Dirty = true;
        }

        private static RegionFile LoadRegionFile(string path) => new RegionFile(path);
    }
}
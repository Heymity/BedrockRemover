using System;
using System.Linq;
using Cyotek.Data.Nbt;
using RemoveBedrock.Minecraft.McaReader;

namespace RemoveBedrock.BedrockRemoval
{
    public static class ChunkHandler
    {
        public static bool LogStages { get; set; } = true;
        
        public static void ChangeChunkPaletteBlock(this Chunk chunk, string oldBlockType, string newBlockType, byte yLayer = 0)
        {
            var root = chunk.Root;
            if (root is null)
            {
                Log($"Chunk {chunk.Coords} is empty");
                return;
            }
            
            Log($"Querying palette of chunk {chunk.Coords}");
            var level = (TagCompound)root["Level"];
            var sections = (TagList)level["Sections"];

            var layerZero = sections.Value.Cast<TagCompound>().FirstOrDefault(tag => ((TagByte)tag["Y"]).Value == yLayer);

            if (layerZero is null)
            {
                Log($"Chunk {chunk.Coords} has no layer zero (there is no bedrock to be changed)");
                return;
            }
            
            var palette = layerZero.Query<TagList>("Palette");

            Log($"Changing palette of chunk {chunk.Coords}");

            var bedrockTag = palette.Value.Cast<TagCompound>().Select(tag => (TagString)tag["Name"])
                .FirstOrDefault(tag => tag.Value == oldBlockType);

            if (bedrockTag is null)
            {
                Log($"No bedrock found for chunk {chunk.Coords}");
                return;
            }
            
            Log($"Substituting bedrock of chunk {chunk.Coords}");

            bedrockTag.Value = newBlockType;
            chunk.Dirty = true;
            
            Log($"Chunk {chunk.Coords} done!");
        }

        private static void Log(string logValue)
        {
            if (!LogStages) return;
            Console.WriteLine(logValue);
        }
    }
}
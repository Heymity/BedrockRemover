using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Cyotek.Data.Nbt;
using RemoveBedrock.BedrockRemoval;
using RemoveBedrock.Minecraft.McaReader;

namespace RemoveBedrock
{
    internal static class Program
    {
        //private const string ProgramPath =
        //    @"C:\Users\GABRIEL\Desktop\LangFiles\C#\NBTManipulation\RemoveYZeroBedrock\RemoveYZeroBedrock";

        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH",
            MessageId = "type: System.Int16[]; size: 109MB")]
        private static void Main(string[] args)
        {
            var minCoord = new Coord(int.MinValue, int.MinValue);
            var maxCoord = new Coord(int.MaxValue, int.MaxValue);
            
            if (args.Length > 0)
            {
                if (args.Length % 2 != 0)
                {
                    Console.WriteLine(
                        "The provided args cannot be converted into coordinates, they weren't provided in pairs");
                    return;
                }

                var coord1X = int.Parse(args[0]);
                var coord1Z = int.Parse(args[1]);
                var coord2X = int.MaxValue;
                var coord2Z = int.MaxValue;

                if (args.Length > 2)
                {
                    coord2X = int.Parse(args[2]);
                    coord2Z = int.Parse(args[3]);
                }

                minCoord.X = Math.Min(coord1X, coord2X);
                maxCoord.X = Math.Max(coord1X, coord2X);
                minCoord.Z = Math.Min(coord1Z, coord2Z);
                maxCoord.Z = Math.Max(coord1Z, coord2Z);
                
                minCoord.AbsoluteToChunk();
                maxCoord.AbsoluteToChunk();
                
                Console.WriteLine($"Substituting bedrock for area ranging from chunk {minCoord} to chunk {maxCoord}");
            }

            Console.Write("Please specify the world save to be altered (Path to save): ");
            var saveDir = Console.ReadLine() + "\\region";

            /*var saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\saves";

            var i = 0;
            foreach (var save in Directory.GetDirectories(saveFolder))
            {
                Console.WriteLine($"{i} {Path.GetFileName(save)}");
                i++;
            }*/
            
            RegionHandler.RemoveBedrockForDirectory(saveDir, minCoord, maxCoord);
        }
    }
}
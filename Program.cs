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
        private const string ProgramPath =
            @"C:\Users\GABRIEL\Desktop\LangFiles\C#\NBTManipulation\RemoveYZeroBedrock\RemoveYZeroBedrock";

        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH",
            MessageId = "type: System.Int16[]; size: 109MB")]
        private static void Main(string[] args)
        {
            RegionHandler.RemoveBedrockForDirectory(ProgramPath + @"\test\input");
        }
    }
}
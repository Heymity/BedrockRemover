using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cyotek.Data.Nbt;
using Ionic.Zlib;

namespace RemoveBedrock.Minecraft.McaReader
{
   public class RegionFile : IEnumerable<Chunk>
    {
        private Chunk[,] _chunks;
        private Coord _coords;
        private readonly string _path;
        private bool _dirty;

        public RegionFile()
        {
            _chunks = new Chunk[32, 32];
        }

        public RegionFile(string path)
        {
            _path = path;
            Read(_path);
        }

        public Coord Coords => _coords;

        //http://www.minecraftwiki.net/wiki/Region_file_format
        private void Read(string path)
        {
            _chunks = new Chunk[32, 32];
            var m = Regex.Match(path, @"r\.(-?\d+)\.(-?\d+)\.mc[ar]");
            _coords.X = int.Parse(m.Groups[1].Value);
            _coords.Z = int.Parse(m.Groups[2].Value);

            var header = new byte[8192];

            using var file = new BinaryReader(File.Open(path, FileMode.Open));
            file.Read(header, 0, 8192);

            for (var chunkZ = 0; chunkZ < 32; chunkZ++)
            {
                for (var chunkX = 0; chunkX < 32; chunkX++)
                {
                    var chunk = new Chunk();
                    chunk.Coords.X = Coords.X;
                    chunk.Coords.Z = Coords.Z;
                    chunk.Coords.RegionToChunk();
                    chunk.Coords.Add(chunkX, chunkZ);
                    
                    var i = 4 * (chunkX + chunkZ * 32);

                    var temp = new byte[4];
                    temp[0] = 0;
                    Array.Copy(header, i, temp, 1, 3);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(temp);
                    var offset = ((long)BitConverter.ToInt32(temp, 0)) * 4096;
                    var length = header[i + 3] * 4096;

                    temp = new byte[4];
                    Array.Copy(header, i + 4096, temp, 0, 4);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(temp);
                    chunk.Timestamp = BitConverter.ToInt32(temp, 0);

                    if (offset == 0 && length == 0)
                    {
                        _chunks[chunkX, chunkZ] = chunk;
                        continue;
                    }

                    file.BaseStream.Seek(offset, SeekOrigin.Begin);

                    temp = new byte[4];
                    file.Read(temp, 0, 4);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(temp);
                    var exactLength = BitConverter.ToInt32(temp, 0);

                    chunk.CompressionType = file.ReadByte();
                    switch (chunk.CompressionType)
                    {
                        //GZip
                        case 1:
                        {
                            chunk.RawData = new byte[exactLength - 1];
                            file.Read(chunk.RawData, 0, exactLength - 1);

                            using var decompress = new GZipStream(new MemoryStream(chunk.RawData), CompressionMode.Decompress);
                            using var mem = new MemoryStream();
                            decompress.CopyTo(mem);
                            mem.Seek(0, SeekOrigin.Begin);

                            var document = NbtDocument.LoadDocument(mem);
                            chunk.NbtData = document;
                            break;
                        }
                        //Zlib
                        case 2:
                        {
                            chunk.RawData = new byte[exactLength - 1];
                            file.Read(chunk.RawData, 0, exactLength - 1);

                            using var decompress = new ZlibStream(new MemoryStream(chunk.RawData), CompressionMode.Decompress);
                            using var mem = new MemoryStream();
                            decompress.CopyTo(mem);
                            mem.Seek(0, SeekOrigin.Begin);

                            var document = NbtDocument.LoadDocument(mem);
                            chunk.NbtData = document;
                            break;
                        }
                        default:
                            throw new Exception("Unrecognized compression type");
                    }
                    _chunks[chunkX, chunkZ] = chunk;
                }
            }

            file.Close();
        }

        public void Write()
        {
            Write(_path);
        }

        public void Write(string path)
        {
            if (!_dirty)
                return;
            
            var header = new byte[8192];
            Array.Clear(header, 0, 8192);

            var sectorOffset = 2;
            using var file = new BinaryWriter(File.Exists(path) ? File.Open(path, FileMode.Truncate) : File.Open(path, FileMode.Create));
            file.Write(header, 0, 8192);

            for (var chunkX = 0; chunkX < 32; chunkX++)
            {
                for (var chunkZ = 0; chunkZ < 32; chunkZ++)
                {
                    var c = _chunks[chunkX, chunkZ];
                    if (c == null)
                        continue;

                    var i = 4 * (chunkX + chunkZ * 32);

                    var temp = BitConverter.GetBytes(c.Timestamp);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(temp);
                    Array.Copy(temp, 0, header, i + 4096, 4);

                    if (c.Root == null)
                    {
                        Array.Clear(temp, 0, 4);
                        Array.Copy(temp, 0, header, i, 4);
                        continue;
                    }

                    temp = BitConverter.GetBytes(sectorOffset);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(temp);
                    Array.Copy(temp, 1, header, i, 3);

                    if (c.RawData == null || c.Dirty)
                    {
                        //this is the performance bottleneck when doing 1024 chunks in a row;
                        //trying to only do when necessary
                        using var mem = new MemoryStream();
                        using var zlib = new ZlibStream(mem, CompressionMode.Compress);
                        c.NbtData.Save(zlib);
                        zlib.Close();
                        c.RawData = mem.ToArray();
                        c.CompressionType = 2;
                    }

                    temp = BitConverter.GetBytes(c.RawData.Length + 1);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(temp);

                    file.Write(temp, 0, 4);
                    file.Write(c.CompressionType);
                    file.Write(c.RawData, 0, c.RawData.Length);

                    var padding = new byte[(4096 - ((c.RawData.Length + 5) % 4096))];
                    Array.Clear(padding, 0, padding.Length);
                    file.Write(padding);

                    header[i + 3] = (byte)((c.RawData.Length + 5) / 4096 + 1);
                    sectorOffset += (c.RawData.Length + 5) / 4096 + 1;
                    c.Dirty = false;
                }
            }

            file.Seek(0, SeekOrigin.Begin);
            file.Write(header, 0, 8192);
            file.Flush();
            file.Close();
            _dirty = false;
        }

        public void SetDirty() => _dirty = true;
        
        public IEnumerator<Chunk> GetEnumerator() => _chunks.Cast<Chunk>().ToList().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _chunks.GetEnumerator();
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Region [{0}, {1}]{2}{{{2}", Coords.X, Coords.Z, Environment.NewLine);
            foreach (var c in _chunks)
                sb.Append(c);
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string ToString(string path)
        {
            var m = Regex.Match(path, @"r\.(-?\d+)\.(-?\d+)\.mc[ar]");
            var c = new Coord(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
            var c2 = new Coord(int.Parse(m.Groups[1].Value) + 1, int.Parse(m.Groups[2].Value) + 1);
            c.RegionToAbsolute();
            c2.RegionToAbsolute();
            return $"Region {m.Groups[1].Value}, {m.Groups[2].Value} :: ({c.X}, {c.Z}) to ({c2.X - 1}, {c2.Z - 1})";
        }
    }
}
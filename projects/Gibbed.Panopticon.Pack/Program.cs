/* Copyright (c) 2025 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;
using Gibbed.Panopticon.FileFormats;
using Gibbed.Panopticon.FileFormats.Archives;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using NDesk.Options;
using Endian = Gibbed.Memory.Endian;
using FileEndian = Gibbed.IO.Endian;

namespace Gibbed.Panopticon.Pack
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int? compressionLevel = null;
            bool verbose = false;
            bool showHelp = false;

            OptionSet options = new()
            {
                { "c|compression-level=", "set compression level (0-9), default 9", v => compressionLevel = int.Parse(v) },
                { "v|verbose", "be verbose", v => verbose = v != null },
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", ProjectHelpers.GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", ProjectHelpers.GetExecutableName());
                return;
            }

            if (extras.Count < 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ output_eaf input_directory+", ProjectHelpers.GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            List<string> inputPaths = new();
            string outputPath;

            if (extras.Count == 1)
            {
                inputPaths.Add(extras[0]);
                outputPath = Path.ChangeExtension(extras[0], ".eaf");
            }
            else
            {
                outputPath = extras[0];
                inputPaths.AddRange(extras.Skip(1));
            }

            SortedDictionary<string, string> pendingEntries = new(new ArchivePathComparer());

            if (verbose == true)
            {
                Console.WriteLine("Finding files...");
            }

            var directorySeparatorString = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            var altDirectorySeparatorString = Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

            foreach (var relativePath in inputPaths)
            {
                string inputPath = Path.GetFullPath(relativePath);

                if (inputPath.EndsWith(directorySeparatorString) == true ||
                    inputPath.EndsWith(altDirectorySeparatorString) == true)
                {
                    inputPath = inputPath.Substring(0, inputPath.Length - 1);
                }

                foreach (string path in Directory.EnumerateFiles(inputPath, "*", SearchOption.AllDirectories))
                {
                    string fullPath = Path.GetFullPath(path);
                    string partPath = fullPath.Substring(inputPath.Length + 1);

                    var pieces = partPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    var name = string.Join("/", pieces);

                    if (pendingEntries.TryGetValue(name, out var previousFullPath) == true)
                    {
                        Console.WriteLine($"Ignoring duplicate of {name}: {fullPath}");
                        if (verbose == true)
                        {
                            Console.WriteLine($"  Previously added from: {previousFullPath}");
                        }
                        continue;
                    }

                    pendingEntries[name] = fullPath;
                }
            }

            using (var output = File.Create(outputPath))
            {
                var headerSize = ArchiveFile.EstimateHeaderSize(pendingEntries.Count);

                output.Position = headerSize;

                ArchiveFile archive = new()
                {
                    Endian = Endian.Little,
                };

                var fileEndian = ToFileEndian(archive.Endian);

                long current = 0;
                long total = pendingEntries.Count;
                var padding = total.ToString(CultureInfo.InvariantCulture).Length;

                foreach (var kv in pendingEntries)
                {
                    var name = kv.Key;
                    var fullPath = kv.Value;

                    current++;

                    if (verbose == true)
                    {
                        Console.WriteLine(
                            "[{0}/{1}] {2}",
                            current.ToString(CultureInfo.InvariantCulture).PadLeft(padding),
                            total,
                            name);
                    }

                    output.Position = IO.NumberHelpers.Align(output.Position, 0x100L);

                    Entry entry;
                    using (var input = File.OpenRead(fullPath))
                    {
                        entry = Pack(
                            name,
                            input, input.Length,
                            output,
                            compressionLevel ?? Deflater.BEST_COMPRESSION,
                            fileEndian);
                    }
                    archive.Entries.Add(entry);
                }

                archive.TotalSize = output.Length;

                output.Position = 0;
                archive.Serialize(output);
            }
        }

        private static FileEndian ToFileEndian(Endian endian) => endian switch
        {
            Endian.Little => FileEndian.Little,
            Endian.Big => FileEndian.Big,
            _ => throw new NotSupportedException(),
        };

        private static Entry Pack(
            string name,
            Stream input, long length,
            Stream output, int compressionLevel, FileEndian endian)
        {
            var compressedBytes = CompressZlib(input, length, compressionLevel, out var hash);
            long dataOffset = output.Position;
            uint dataSizeCompressed = (uint)compressedBytes.Length;

            output.WriteValueU32(0x5A4D4523u, endian); // '#EMZ'
            output.WriteValueU32(hash, endian);
            output.WriteValueU32((uint)length, endian);
            output.WriteValueU32(16, endian);
            output.Write(compressedBytes, 0, compressedBytes.Length);

            Entry entry;
            entry.Path = name;
            entry.DataOffset = dataOffset;
            entry.DataSize = 16 + dataSizeCompressed;
            return entry;
        }

        private static byte[] CompressZlib(Stream input, long length, int level, out uint hash)
        {
            using MemoryStream data = new();
            using DeflaterOutputStream zlib = new(data, new(level, true));
            zlib.IsStreamOwner = false;
            input.CopyTo(length, 0u, zlib, out hash);
            zlib.Finish();
            zlib.Flush();
            data.Flush();
            return data.ToArray();
        }
    }
}

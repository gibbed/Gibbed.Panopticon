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
using System.Text.RegularExpressions;
using Gibbed.IO;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;
using Gibbed.Panopticon.FileFormats;
using NDesk.Options;
using Endian = Gibbed.Memory.Endian;
using Entry = Gibbed.Panopticon.FileFormats.Archives.Entry;
using InflaterInputStream = ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream;

namespace Gibbed.Panopticon.Unpack
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string filterPattern = null;
            bool overwriteFiles = false;
            bool test = false;
            bool verbose = false;
            bool showHelp = false;

            OptionSet options = new()
            {
                { "f|filter=", "only extract files using pattern", v => filterPattern = v },
                { "o|overwrite", "overwrite existing files", v => overwriteFiles = v != null },
                { "t|test", "test mode", v => test = v != null },
                { "v|verbose", "be verbose (list files)", v => verbose = v != null },
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

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input [output_directory]", ProjectHelpers.GetExecutableName());
                Console.WriteLine("Unpack specified package.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPath = extras[0];
            string outputBasePath = extras.Count > 1
                ? extras[1]
                : Path.ChangeExtension(inputPath, null) + "_unpack";

            Regex filter = null;
            if (string.IsNullOrEmpty(filterPattern) == false)
            {
                filter = new(filterPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            using (var input = File.OpenRead(inputPath))
            {
                ArchiveFile archive = new();
                archive.Deserialize(input);

                var endian = archive.Endian;

                long current = 0;
                long total = archive.Entries.Count;
                var padding = total.ToString(CultureInfo.InvariantCulture).Length;

                foreach (var entry in archive.Entries.OrderBy(rh => rh.DataOffset))
                {
                    current++;

                    if (filter != null && filter.IsMatch(entry.Path) == false)
                    {
                        continue;
                    }

                    var outputPath = Path.Combine(outputBasePath, entry.Path.Replace('/', Path.DirectorySeparatorChar));

                    if (overwriteFiles == false && File.Exists(outputPath) == true)
                    {
                        continue;
                    }

                    if (verbose == true)
                    {
                        Console.WriteLine(
                            "[{0}/{1}] {2}",
                            current.ToString(CultureInfo.InvariantCulture).PadLeft(padding),
                            total,
                            entry.Path);
                    }

                    if (test == false)
                    {
                        var outputParentPath = Path.GetDirectoryName(outputPath);
                        if (string.IsNullOrEmpty(outputParentPath) == false)
                        {
                            Directory.CreateDirectory(outputParentPath);
                        }

                        using var output = File.Create(outputPath);
                        Unpack(input, entry, endian, output);
                    }
                }
            }
        }

        private static void Unpack(Stream input, Entry entry, Endian endian, Stream output)
        {
            input.Position = entry.DataOffset;

            var headSize = (int)Math.Min(entry.DataSize, 16);
            var headBytes = input.ReadBytes(headSize);
            ReadOnlySpan<byte> headSpan = new(headBytes);

            if (headSize >= 4)
            {
                int index = 0;
                var magic = headSpan.ReadValueU32(ref index, endian);
                if (magic == 0x5A4D4523u) // '#EMZ'
                {
                    UnpackZlib(input, entry, headSpan, endian, output);
                    return;
                }
            }

            if (headSize >= 3 && headSpan[0] == 0x1F && headSpan[1] == 0x8B && headSpan[2] == 8)
            {
                UnpackGzip(input, entry, headSpan, endian, output);
                return;
            }

            throw new InvalidOperationException("unsupported compression scheme");
        }

        private static void UnpackZlib(Stream input, Entry entry, ReadOnlySpan<byte> headSpan, Endian endian, Stream output)
        {
            int index = 4;
            _ = headSpan.ReadValueU32(ref index, endian); // hash
            var uncompressedSize = headSpan.ReadValueU32(ref index, endian);
            var compressedOffset = headSpan.ReadValueU32(ref index, endian);

            input.Position = entry.DataOffset + compressedOffset;

            using InflaterInputStream zlib = new(input, new(true));
            zlib.IsStreamOwner = false;
            zlib.CopyTo(uncompressedSize, output);
        }

        private static void UnpackGzip(Stream input, Entry entry, ReadOnlySpan<byte> headSpan, Endian endian, Stream output)
        {
            // gzip header, followed by compressed data, followed by tail header

            input.Position = entry.DataOffset + entry.DataSize - 8;
            var tailBytes = input.ReadBytes(8);
            ReadOnlySpan<byte> tailSpan = new(tailBytes);
            int index = 0;
            var hash = tailSpan.ReadValueU32(ref index, endian);
            var uncompressedSize = tailSpan.ReadValueU32(ref index, endian);

            throw new NotImplementedException();
        }
    }
}

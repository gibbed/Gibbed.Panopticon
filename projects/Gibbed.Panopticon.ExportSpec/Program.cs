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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;
using Gibbed.Panopticon.FileFormats;
using Hexarc.Serialization.Union;
using NDesk.Options;

namespace Gibbed.Panopticon.ExportItemSpec
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            bool showHelp = false;

            OptionSet options = new()
            {
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input [output]", ProjectHelpers.GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            const string outputExtension = ".json";

            var specTypeHandlers = new Dictionary<uint, (string extension, LoadSpecFileDelegate load)>()
            {
                { ItemSpecFile.Signature, (SpecFileExtensions.Item, ItemSpecFile.Load) },
            };
            var specTypeExtensions = specTypeHandlers.Select(kv => kv.Value.extension).ToList();

            var inputBasePath = Path.GetFullPath(extras[0]);

            List<(string inputPath, string outputPath)> targets = new();
            if (Directory.Exists(inputBasePath) == false)
            {
                var inputPath = inputBasePath;
                string outputPath = extras.Count > 1
                    ? Path.GetFullPath(extras[1])
                    : Path.ChangeExtension(inputPath, outputExtension);
                targets.Add((inputPath, outputPath));
            }
            else
            {
                foreach (var inputPath in Directory.EnumerateFiles(inputBasePath, "*", SearchOption.AllDirectories))
                {
                    if (specTypeExtensions.Contains(Path.GetExtension(inputPath).ToLowerInvariant()) == false)
                    {
                        continue;
                    }
                    var outputPath = Path.ChangeExtension(inputPath, outputExtension);
                    targets.Add((inputPath, outputPath));
                }
            }

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new UnionConverterFactory(),
                },
            };

            foreach (var (inputPath, outputPath) in targets.OrderBy(t => t.inputPath))
            {
                var inputBytes = File.ReadAllBytes(inputPath);
                ReadOnlySpan<byte> inputSpan = new(inputBytes);
                if (GetMagic(inputSpan, out var magic) == false)
                {
                    Console.WriteLine($"Failed to get signature for '{inputPath}'!");
                    continue;
                }

                if (specTypeHandlers.TryGetValue(magic, out var handler) == false)
                {
                    Console.WriteLine($"Unsupported spec file type '{inputPath}'!");
                    continue;
                }

                var specFile = handler.load(inputSpan);
                var specJsonBytes = JsonSerializer.SerializeToUtf8Bytes(specFile, jsonSerializerOptions);
                File.WriteAllBytes(outputPath, specJsonBytes);
            }
        }

        private delegate BaseSpecFile LoadSpecFileDelegate(ReadOnlySpan<byte> span);

        private static bool GetMagic(ReadOnlySpan<byte> span, out uint magic)
        {
            if (span.Length < 4)
            {
                magic = default;
                return false;
            }
            int index = 0;
            magic = span.ReadValueU32(ref index, Endian.Little);
            return true;
        }
    }
}

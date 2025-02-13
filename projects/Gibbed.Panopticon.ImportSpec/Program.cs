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
using Gibbed.Buffers;
using Gibbed.Panopticon.Common;
using Gibbed.Panopticon.FileFormats;
using Hexarc.Serialization.Union;
using NDesk.Options;

namespace Gibbed.Panopticon.ImportItemSpec
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

            const string inputExtension = ".json";

            var inputBasePath = Path.GetFullPath(extras[0]);

            List<(string inputPath, string outputPathBase, bool wantExtension)> targets = new();
            if (Directory.Exists(inputBasePath) == false)
            {
                var inputPath = inputBasePath;
                bool wantExtension;
                string outputPathBase;
                if (extras.Count > 1)
                {
                    outputPathBase = Path.GetFullPath(extras[1]);
                    wantExtension = false;
                }
                else
                {
                    outputPathBase = Path.ChangeExtension(inputPath, null);
                    wantExtension = true;
                }
                targets.Add((inputPath, outputPathBase, wantExtension));
            }
            else
            {
                foreach (var inputPath in Directory.EnumerateFiles(inputBasePath, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(inputPath) != inputExtension)
                    {
                        continue;
                    }
                    var outputPathBase = Path.ChangeExtension(inputPath, null);
                    targets.Add((inputPath, outputPathBase, true));
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

            foreach (var (inputPath, outputPathBase, wantExtension) in targets.OrderBy(t => t.inputPath))
            {
                if (ParseFile(inputPath, jsonSerializerOptions, out var errors, out var specFile) == false)
                {
                    Console.WriteLine($"Failed to import '{inputPath}'!");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  {error}");
                    }
                    continue;
                }

                var outputPath = wantExtension == true
                    ? outputPathBase + specFile.FileExtension
                    : outputPathBase;

                PooledArrayBufferWriter<byte> writer = new();
                specFile.Save(writer);
                var writtenSpan = writer.WrittenSpan;
                File.WriteAllBytes(outputPath, writtenSpan.ToArray());
                writer.Clear();
            }
        }

        private static bool ParseFile(
            string path,
            JsonSerializerOptions jsonSerializerOptions,
            out List<string> errors,
            out BaseSpecFile specFile)
        {
            using (var input = File.OpenRead(path))
            {
                try
                {
                    errors = default;
                    specFile = JsonSerializer.Deserialize<BaseSpecFile>(input, jsonSerializerOptions);
                    return true;
                }
                catch (JsonException ex)
                {
                    errors = new()
                    {
                        $"({1+ex.LineNumber},{1+ex.BytePositionInLine}): {ex.Message}",
                    };
                    specFile = default;
                    return false;
                }
            }
        }
    }
}

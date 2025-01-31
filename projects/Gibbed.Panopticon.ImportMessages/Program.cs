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
using Gibbed.Buffers;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;
using Gibbed.Panopticon.FileFormats;
using NDesk.Options;

namespace Gibbed.Panopticon.ImportMessages
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

            const string inputExtension = ".toml";
            const string outputExtension = ".lmsg";

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
                    if (Path.GetExtension(inputPath) != inputExtension)
                    {
                        continue;
                    }
                    var outputPath = Path.ChangeExtension(inputPath, outputExtension);
                    targets.Add((inputPath, outputPath));
                }
            }

            foreach (var (inputPath, outputPath) in targets.OrderBy(t => t.inputPath))
            {
                if (ParseFile(inputPath, out var errors, out var rootTable) == false ||
                    Import(rootTable, out errors, out var messageFile) == false)
                {
                    Console.WriteLine($"Failed to import '{inputPath}'!");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  {error}");
                    }
                    continue;
                }

                PooledArrayBufferWriter<byte> writer = new();
                messageFile.Write(writer);
                var writtenSpan = writer.WrittenSpan;
                File.WriteAllBytes(outputPath, writtenSpan.ToArray());
                writer.Clear();
            }
        }

        private static bool ParseFile(string path, out List<string> errors, out Tommy.TomlTable table)
        {
            var bytes = File.ReadAllBytes(path);
            using (var input = new MemoryStream(bytes, false))
            using (var reader = new StreamReader(input, true))
            {
                try
                {
                    errors = default;
                    table = Tommy.TOML.Parse(reader);
                    return true;
                }
                catch (Tommy.TomlParseException ex)
                {
                    errors = new();
                    foreach (Tommy.TomlSyntaxException syntaxEx in ex.SyntaxErrors)
                    {
                        errors.Add($"({syntaxEx.Line},{syntaxEx.Column}): {syntaxEx.Message}");
                    }
                    table = default;
                    return false;
                }
            }
        }

        private static bool Import(Tommy.TomlTable table, out List<string> errors, out LanguageMessageFile messageFile)
        {
            Tommy.TomlArray messageArray = new()
            {
                IsTableArray = true,
            };

            errors = new();
            messageFile = new();

            if (ImportEnum(table, "endian", Endian.Little, out var endian) == false)
            {
                errors.Add($"Invalid endian '{endian}' specified.");
                return false;
            }

            messageFile.Endian = endian;

            foreach (Tommy.TomlTable messageTable in table["message"])
            {
                var id2 = ImportUInt32(messageTable["id"]) ?? throw new ArgumentNullException();
                var id = ImportUInt32(messageTable["id2"]) ?? id2;
                var key = messageTable["key"].AsString?.Value;
                var value = messageTable["value"].AsString?.Value;

                value = value.Replace("\r", "");

                messageFile.Messages.Add(new()
                {
                    Id = id,
                    Id2 = id2,
                    Key = key,
                    Value = value,
                });
            }

            return true;
        }

        private static uint? ImportUInt32(Tommy.TomlNode node)
        {
            var asInteger = node.AsInteger;
            return asInteger != null ? Convert.ToUInt32(asInteger.Value) : null;
        }

        private static bool ImportEnum<TEnum>(Tommy.TomlTable table, string key, out TEnum value)
             where TEnum : struct
        {
            if (table[key] is not Tommy.TomlString str)
            {
                value = default;
                return false;
            }
            if (Enum.TryParse(str, out value) == false)
            {
                value = default;
                return false;
            }
            return true;
        }

        private static bool ImportEnum<TEnum>(Tommy.TomlTable table, string key, TEnum defaultValue, out TEnum value)
             where TEnum : struct
        {
            if (table[key] is not Tommy.TomlString str)
            {
                value = defaultValue;
                return true;
            }
            if (Enum.TryParse(str, out value) == false)
            {
                value = default;
                return false;
            }
            return true;
        }
    }
}

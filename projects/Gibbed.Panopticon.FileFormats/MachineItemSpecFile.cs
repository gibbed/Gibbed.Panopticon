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
using System.Text.Json.Serialization;
using Gibbed.Buffers;
using Gibbed.Memory;
using Gibbed.Panopticon.FileFormats.MachineItemSpecs;

namespace Gibbed.Panopticon.FileFormats
{
    using IMachineItemLabeler = ILabeler<StringPool>;

    public class MachineItemSpecFile : BaseSpecFile
    {
        public const uint Signature = FileHeader.Signature;

        public override SpecFileType Type => SpecFileType.MachineItem;

        [JsonIgnore]
        public override string FileExtension => SpecFileExtensions.MachineItem;

        [JsonPropertyName("endian")]
        public Endian Endian { get; set; }

        [JsonPropertyName("version")]
        public GameVersion Version { get; set; }

        [JsonPropertyName("unknown08")]
        public MachineItemLotSpec[] Unknown08s { get; set; }

        [JsonPropertyName("unknown10")]
        public MachineItemLotSpec[] Unknown10s { get; set; }

        private static bool DetermineGameVersion(
            TableInfo<MachineItemLotSpec> tableInfo,
            ReadOnlySpan<byte> span, Endian endian,
            out GameVersion version)
        {
            if (tableInfo.Header.Count < 2)
            {
                version = default;
                return false;
            }
            int index = 38;
            ushort count = span.ReadValueU16(ref index, endian);
            int offset1 = span.ReadValueS32(ref index, endian);
            index = 56;
            int offset2 = span.ReadValueS32(ref index, endian);

            int size = offset2 - offset1;
            if (size == count * 112)
            {
                version = GameVersion.Remaster;
                return true;
            }
            else if (size == count * 80)
            {
                version = GameVersion.Vita;
                return true;
            }
            version = default;
            return false;
        }

        public static MachineItemSpecFile Load(ReadOnlySpan<byte> span)
        {
            int index = 0;
            var header = FileHeader.Read(span, ref index);
            var endian = header.Endian;

            if (DetermineGameVersion(header.Unknown08s, span, endian, out var version) == false ||
                DetermineGameVersion(header.Unknown10s, span, endian, out version) == false)
            {
                throw new FormatException("failed to determine file version");
            }

            MachineItemSpecFile instance = new();
            instance.Endian = endian;
            instance.Version = version;
            instance.Unknown08s = header.Unknown08s.LoadTable(span, version, endian);
            instance.Unknown10s = header.Unknown10s.LoadTable(span, version, endian);
            return instance;
        }

        private void Save(IArrayBufferWriter<byte> writer, IMachineItemLabeler labeler)
        {
            FileHeader header = new();
            var endian = header.Endian = this.Endian;
            var version = this.Version;
            header.Write(writer, labeler);

            header.Unknown08s.SaveTable(this.Unknown08s, writer, labeler, version, endian);
            header.Unknown10s.SaveTable(this.Unknown10s, writer, labeler, version,endian);
        }

        private static void AddStrings(Labeler labeler, MachineItemLotSpec[] machineItemLots)
        {
            if (machineItemLots == null)
            {
                return;
            }
            foreach (var machineItemLot in machineItemLots)
            {
                labeler.AddString(machineItemLot.MachineId);
                if (machineItemLot.Lots == null)
                {
                    continue;
                }
                foreach (var lot in machineItemLot.Lots)
                {
                    labeler.AddString(lot.PartId);
                    for (int i = 0; i < PartDropItemLotSpec.ItemCount; i++)
                    {
                        labeler.AddString(lot.Unknown10[i].ItemId);
                        labeler.AddString(lot.Unknown30[i].ItemId);
                        labeler.AddString(lot.Unknown50[i].ItemId);
                    }
                }
            }
        }

        private void AddStrings(Labeler labeler)
        {
            // This is to ensure the string table gets written out in the same
            // way the original file is set up without having to radically
            // alter the saving code to accomodate this.
            AddStrings(labeler, this.Unknown08s);
            AddStrings(labeler, this.Unknown10s);
        }

        public void Save(IArrayBufferWriter<byte> writer)
        {
            var endian = this.Endian;
            PooledArrayBufferWriter<byte> buffer = new();
            Labeler labeler = new();
            AddStrings(labeler);
            Save(buffer, labeler);
            var bytes = buffer.WrittenSpan.ToArray();
            buffer.Clear();
            labeler.Fixup(bytes, out var stringBytes, endian);
            writer.WriteBytes(bytes);
            writer.WriteBytes(stringBytes);
        }
    }
}

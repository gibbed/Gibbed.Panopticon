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
using Gibbed.Panopticon.Common;

namespace Gibbed.Panopticon.FileFormats.MachineItemSpecs
{
    using ILabeler = ILabeler<StringPool>;
    using ISpec = ISpec<StringPool, ILabeler<StringPool>>;

    public class PartDropItemLotSpec : ISpec
    {
        internal int Size(GameVersion version) => version switch
        {
            GameVersion.Vita => 80,
            GameVersion.Remaster => 112,
            _ => throw new ArgumentOutOfRangeException(nameof(version), "unsupported game version"),
        };

        public const int ItemCount = 4;

        private int _PartIdOffset;
        private readonly int[] _Unknown10ItemIdOffsets;
        private readonly int[] _Unknown30ItemIdOffsets;
        private readonly int[] _Unknown50ItemIdOffsets;

        private readonly PartDropItem[] _Unknown10;
        private readonly PartDropItem[] _Unknown30;
        private readonly PartDropItem[] _Unknown50;

        public PartDropItemLotSpec()
        {
            this._Unknown10ItemIdOffsets = new int[ItemCount];
            this._Unknown30ItemIdOffsets = new int[ItemCount];
            this._Unknown50ItemIdOffsets = new int[ItemCount];
            this._Unknown10 = new PartDropItem[ItemCount];
            this._Unknown30 = new PartDropItem[ItemCount];
            this._Unknown50 = new PartDropItem[ItemCount];
        }

        [JsonConstructor]
        public PartDropItemLotSpec(PartDropItem[] unknown10, PartDropItem[] unknown30, PartDropItem[] unknown50)
            : this()
        {
            if (unknown10 == null)
            {
                throw new ArgumentNullException(nameof(unknown10));
            }
            if (unknown10.Length != ItemCount)
            {
                throw new ArgumentOutOfRangeException(nameof(unknown10));
            }
            Array.Copy(unknown10, this._Unknown10, ItemCount);

            if (unknown30 == null)
            {
                throw new ArgumentNullException(nameof(unknown30));
            }
            if (unknown30.Length != ItemCount)
            {
                throw new ArgumentOutOfRangeException(nameof(unknown30));
            }
            Array.Copy(unknown30, this._Unknown30, ItemCount);

            if (unknown50 == null)
            {
                throw new ArgumentNullException(nameof(unknown50));
            }
            if (unknown50.Length != ItemCount)
            {
                throw new ArgumentOutOfRangeException(nameof(unknown50));
            }
            Array.Copy(unknown50, this._Unknown50, ItemCount);
        }

        [JsonPropertyName("part_id")]
        public string PartId { get; set; }

        [JsonPropertyName("unknown04")]
        public ushort Unknown04 { get; set; }

        [JsonPropertyName("unknown10")]
        public PartDropItem[] Unknown10 => this._Unknown10;

        [JsonPropertyName("unknown30")]
        public PartDropItem[] Unknown30 => this._Unknown30;

        [JsonPropertyName("unknown50")]
        public PartDropItem[] Unknown50 => this._Unknown50;

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size(version))
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._PartIdOffset = span.ReadValueS32(ref index, endian);
            this.Unknown04 = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, 10);
            LoadEntry(this._Unknown10, this._Unknown10ItemIdOffsets, span, ref index, endian);
            LoadEntry(this._Unknown30, this._Unknown30ItemIdOffsets, span, ref index, endian);
            if (version >= GameVersion.Remaster)
            {
                LoadEntry(this._Unknown50, this._Unknown50ItemIdOffsets, span, ref index, endian);
            }
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            this.PartId = Helpers.ReadString(span, this._PartIdOffset);
            PostLoadEntry(this._Unknown10, this._Unknown10ItemIdOffsets, span, endian);
            PostLoadEntry(this._Unknown30, this._Unknown30ItemIdOffsets, span, endian);
            if (version >= GameVersion.Remaster)
            {
                PostLoadEntry(this._Unknown50, this._Unknown50ItemIdOffsets, span, endian);
            }
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteStringRef(this.PartId, labeler);
            writer.WriteValueU16(this.Unknown04, endian);
            writer.SkipPadding(10);
            SaveEntry(this._Unknown10, writer, labeler, endian);
            SaveEntry(this._Unknown30, writer, labeler, endian);
            if (version >= GameVersion.Remaster)
            {
                SaveEntry(this._Unknown50, writer, labeler, endian);
            }
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            PostSaveEntry(this._Unknown10, writer, labeler, endian);
            PostSaveEntry(this._Unknown30, writer, labeler, endian);
            if (version >= GameVersion.Remaster)
            {
                PostSaveEntry(this._Unknown50, writer, labeler, endian);
            }
        }

        private static void LoadEntry(
            PartDropItem[] entries, int[] unknown10Offsets,
            ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            int weightIndex = index;
            int quantityIndex = index + ItemCount * 2;
            int itemIdIndex = quantityIndex + ItemCount * 2;
            for (int i = 0; i < ItemCount; i++)
            {
                PartDropItem entry;
                entry.Weight = span.ReadValueU16(ref weightIndex, endian);
                entry.Quantity = span.ReadValueU16(ref quantityIndex, endian);
                entry.ItemId = default;
                unknown10Offsets[i] = span.ReadValueS32(ref itemIdIndex, endian);
                entries[i] = entry;
            }
            index = itemIdIndex;
        }

        private static void PostLoadEntry(
            PartDropItem[] entries, int[] unknown10Offsets,
            ReadOnlySpan<byte> span, Endian endian)
        {
            for (int i = 0; i < ItemCount; i++)
            {
                PartDropItem entry = entries[i];
                entry.ItemId = Helpers.ReadString(span, unknown10Offsets[i]);
                entries[i] = entry;
            }
        }

        private void SaveEntry(
            PartDropItem[] entries,
            IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
            for (int i = 0; i < ItemCount; i++)
            {
                writer.WriteValueU16(entries[i].Weight, endian);
            }
            for (int i = 0; i < ItemCount; i++)
            {
                writer.WriteValueU16(entries[i].Quantity, endian);
            }
            for (int i = 0; i < ItemCount; i++)
            {
                writer.WriteStringRef(entries[i].ItemId, labeler);
            }
        }

        private static void PostSaveEntry(
            PartDropItem[] entries,
            IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
        }
    }
}

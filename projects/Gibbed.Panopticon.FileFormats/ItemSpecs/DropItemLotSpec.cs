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

namespace Gibbed.Panopticon.FileFormats.ItemSpecs
{
    using ILabeler = ILabeler<StringPool>;
    using ISpec = ISpec<StringPool, ILabeler<StringPool>>;

    public class DropItemLotSpec : ISpec
    {
        internal const int Size = 32;
        internal const int PaddingSize = 4;

        public const int ItemCount = 3;

        private readonly int[] _ItemIdOffsets;
        private readonly DropItem[] _Items;

        public DropItemLotSpec()
        {
            this._ItemIdOffsets = new int[ItemCount];
            this._Items = new DropItem[ItemCount];
        }

        [JsonConstructor]
        private DropItemLotSpec(DropItem[] items)
            : this()
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (items.Length != ItemCount)
            {
                throw new ArgumentOutOfRangeException(nameof(items));
            }
            Array.Copy(items, this._Items, ItemCount);
        }

        [JsonPropertyName("unknown00")]
        public uint Unknown00 { get; set; }

        [JsonPropertyName("items")]
        public DropItem[] Items => this._Items;

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this.Unknown00 = span.ReadValueU32(ref index, endian);
            int itemIdIndex = index;
            int itemQuantityIndex = itemIdIndex + ItemCount * 4;
            int itemWeightIndex = itemQuantityIndex + ItemCount * 2;
            for (int i = 0; i < ItemCount; i++)
            {
                this._ItemIdOffsets[i] = span.ReadValueS32(ref itemIdIndex, endian);
                DropItem item;
                item.ItemId = default;
                item.Quantity = span.ReadValueU16(ref itemQuantityIndex, endian);
                item.Weight = span.ReadValueU16(ref itemWeightIndex, endian);
                this._Items[i] = item;
            }
            index = itemWeightIndex;
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            for (int i = 0; i < DropItemLotSpec.ItemCount; i++)
            {
                var item = this.Items[i];
                item.ItemId = Helpers.ReadString(span, this._ItemIdOffsets[i]);
                this.Items[i] = item;
            }
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteValueU32(this.Unknown00, endian);
            for (int i = 0; i < ItemCount; i++)
            {
                writer.WriteStringRef(this._Items[i].ItemId, labeler);
            }
            for (int i = 0; i < ItemCount; i++)
            {
                writer.WriteValueU16(this._Items[i].Quantity, endian);
            }
            for (int i = 0; i < ItemCount; i++)
            {
                writer.WriteValueU16(this._Items[i].Weight, endian);
            }
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
        }
    }
}

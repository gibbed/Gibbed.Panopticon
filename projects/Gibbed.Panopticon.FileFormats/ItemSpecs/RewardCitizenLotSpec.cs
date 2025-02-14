﻿/* Copyright (c) 2025 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.Panopticon.FileFormats.ItemSpecs
{
    using ILabeler = ILabeler<StringPool>;
    using ISpec = ISpec<StringPool, ILabeler<StringPool>>;

    public class RewardCitizenLotSpec : ISpec
    {
        internal const int Size = 32;

        public const int ItemCount = 5;

        private readonly int[] _ItemIdOffsets;
        private readonly RewardCitizenLotItem[] _Items;

        public RewardCitizenLotSpec()
        {
            this._ItemIdOffsets = new int[ItemCount];
            this._Items = new RewardCitizenLotItem[ItemCount];
        }

        [JsonConstructor]
        private RewardCitizenLotSpec(RewardCitizenLotItem[] items)
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

        [JsonPropertyName("id")]
        public ushort Id { get; set; }

        [JsonPropertyName("items")]
        public RewardCitizenLotItem[] Items => this._Items;

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            var itemIdIndex = index;
            var itemWeightIndex = index + 4 * ItemCount + 2;
            for (int i = 0; i < ItemCount; i++)
            {
                this._ItemIdOffsets[i] = span.ReadValueS32(ref itemIdIndex, endian);
                RewardCitizenLotItem item;
                item.ItemId = default;
                item.Weight = span.ReadValueU16(ref itemWeightIndex, endian);
                this._Items[i] = item;
            }
            this.Id = span.ReadValueU16(ref itemIdIndex, endian);
            index = itemWeightIndex;
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            for (int i = 0; i < ItemCount; i++)
            {
                var item = this._Items[i];
                item.ItemId = Helpers.ReadString(span, this._ItemIdOffsets[i]);
                this._Items[i] = item;
            }
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            for (int i = 0; i < ItemCount; i++)
            {
                writer.WriteStringRef(this._Items[i].ItemId, labeler);
            }
            writer.WriteValueU16(this.Id, endian);
            for (int i = 0; i < ItemCount; i++)
            {
                writer.WriteValueU16(this._Items[i].Weight, endian);
            }
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
        }
    }
}

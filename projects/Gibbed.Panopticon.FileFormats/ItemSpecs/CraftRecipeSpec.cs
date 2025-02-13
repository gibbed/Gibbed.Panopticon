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

    public class CraftRecipeSpec : ISpec
    {
        internal const int Size = 48;
        internal const int PaddingSize = 8;

        public const int MaterialCount = 5;

        private int _ProductIdOffset;
        private readonly int[] _MaterialItemIdOffsets;
        private readonly Material[] _Materials;

        public CraftRecipeSpec()
        {
            this._MaterialItemIdOffsets = new int[MaterialCount];
            this._Materials = new Material[MaterialCount];
        }

        [JsonConstructor]
        private CraftRecipeSpec(Material[] materials)
            : this()
        {
            if (materials == null)
            {
                throw new ArgumentNullException(nameof(materials));
            }
            if (materials.Length != MaterialCount)
            {
                throw new ArgumentOutOfRangeException(nameof(materials));
            }
            Array.Copy(materials, this._Materials, MaterialCount);
        }

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("product_type")]
        public ItemType ProductType { get; set; }

        [JsonPropertyName("materials")]
        public Material[] Materials => this._Materials;

        [JsonPropertyName("unknown24")]
        public ushort Unknown24 { get; set; }

        [JsonPropertyName("unknown26")]
        public ushort Unknown26 { get; set; }

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._ProductIdOffset = span.ReadValueS32(ref index, endian);
            int materialItemIdIndex = index;
            int materialQuantityIndex = materialItemIdIndex + MaterialCount * 4 + 2;
            for (int i = 0; i < MaterialCount; i++)
            {
                this._MaterialItemIdOffsets[i] = span.ReadValueS32(ref materialItemIdIndex, endian);
                this._Materials[i].Quantity = span.ReadValueU16(ref materialQuantityIndex, endian);
            }
            index = materialItemIdIndex;
            this.ProductType = (ItemType)span.ReadValueU16(ref index, endian);
            index = materialQuantityIndex;
            this.Unknown24 = span.ReadValueU16(ref index, endian);
            this.Unknown26 = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            this.ProductId = Helpers.ReadString(span, this._ProductIdOffset);
            for (int i = 0; i < MaterialCount; i++)
            {
                this._Materials[i].ItemId = Helpers.ReadString(span, this._MaterialItemIdOffsets[i]);
            }
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteStringRef(this.ProductId, labeler);
            for (int i = 0; i < MaterialCount; i++)
            {
                writer.WriteStringRef(this._Materials[i].ItemId, labeler);
            }
            writer.WriteValueU16((ushort)this.ProductType, endian);
            for (int i = 0; i < MaterialCount; i++)
            {
                writer.WriteValueU16(this._Materials[i].Quantity, endian);
            }
            writer.WriteValueU16(this.Unknown24, endian);
            writer.WriteValueU16(this.Unknown26, endian);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
        }
    }
}

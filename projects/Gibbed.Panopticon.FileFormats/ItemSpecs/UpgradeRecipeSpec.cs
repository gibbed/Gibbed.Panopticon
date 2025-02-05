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
using Gibbed.Buffers;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;
using Newtonsoft.Json;

namespace Gibbed.Panopticon.FileFormats.ItemSpecs
{
    using ISpec = ISpec<StringPool, ILabeler<StringPool>>;
    using ILabeler = ILabeler<StringPool>;

    public class UpgradeRecipeSpec : ISpec
    {
        internal const int Size = 48;
        internal const int PaddingSize = 2;

        public const int MaterialCount = 5;

        private int _OutputItemIdOffset;
        private int _InputItemIdOffset;
        private readonly int[] _MaterialItemIdOffsets;

        private Material[] _Materials;

        public UpgradeRecipeSpec()
        {
            this._MaterialItemIdOffsets = new int[MaterialCount];
            this._Materials = new Material[MaterialCount];
        }

        [JsonConstructor]
        private UpgradeRecipeSpec(Material[] materials)
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

        [JsonProperty("output_item_id")]
        public string OutputItemId { get; set; }

        [JsonProperty("input_item_id")]
        public string InputItemId { get; set; }

        [JsonProperty("materials", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public Material[] Materials => this._Materials;

        [JsonProperty("unknown26")]
        public ushort Unknown26 { get; set; }

        [JsonProperty("unknown28")]
        public ushort Unknown28 { get; set; }

        [JsonProperty("unknown2A")]
        public ushort Unknown2A { get; set; }

        [JsonProperty("unknown2C")]
        public ushort Unknown2C { get; set; }

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._OutputItemIdOffset = span.ReadValueS32(ref index, endian);
            this._InputItemIdOffset = span.ReadValueS32(ref index, endian);
            int materialItemIdIndex = index;
            int materialQuantityIndex = materialItemIdIndex + MaterialCount * 4;
            for (int i = 0; i < MaterialCount; i++)
            {
                this._MaterialItemIdOffsets[i] = span.ReadValueS32(ref materialItemIdIndex, endian);
                var material = this.Materials[i];
                material.Quantity = span.ReadValueU16(ref materialQuantityIndex, endian);
                this.Materials[i] = material;
            }
            index = materialQuantityIndex;
            this.Unknown26 = span.ReadValueU16(ref index, endian);
            this.Unknown28 = span.ReadValueU16(ref index, endian);
            this.Unknown2A = span.ReadValueU16(ref index, endian);
            this.Unknown2C = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            this.OutputItemId = Helpers.ReadString(span, this._OutputItemIdOffset);
            this.InputItemId = Helpers.ReadString(span, this._InputItemIdOffset);
            for (int i = 0; i < MaterialCount; i++)
            {
                var material = this.Materials[i];
                material.ItemId = Helpers.ReadString(span, this._MaterialItemIdOffsets[i]);
                this.Materials[i] = material;
            }
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteStringRef(this.OutputItemId, labeler);
            writer.WriteStringRef(this.InputItemId, labeler);
            for (int i = 0; i < MaterialCount; i++)
            {
                writer.WriteStringRef(this._Materials[i].ItemId, labeler);
            }
            for (int i = 0; i < MaterialCount; i++)
            {
                writer.WriteValueU16(this._Materials[i].Quantity, endian);
            }
            writer.WriteValueU16(this.Unknown26, endian);
            writer.WriteValueU16(this.Unknown28, endian);
            writer.WriteValueU16(this.Unknown2A, endian);
            writer.WriteValueU16(this.Unknown2C, endian);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
        }
    }
}

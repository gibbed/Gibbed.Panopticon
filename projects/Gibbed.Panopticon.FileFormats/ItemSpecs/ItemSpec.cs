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

    public class ItemSpec : ISpec
    {
        internal const int Size = 64;
        internal const int PaddingSize = 4;

        private int _IdOffset;
        private int _NameIdOffset;
        private int _HintIdOffset;
        private int _DescriptionIdOffset;

        [JsonPropertyName("no")]
        public uint IdNo { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public ItemType Type { get; set; }

        [JsonPropertyName("unknown0A")]
        public ushort Unknown0A { get; set; }

        [JsonPropertyName("unknown0C")]
        public uint Unknown0C { get; set; }

        [JsonPropertyName("unknown10")]
        public short Unknown10 { get; set; }

        [JsonPropertyName("unknown12")]
        public ushort Unknown12 { get; set; }

        [JsonPropertyName("unknown14")]
        public ushort Unknown14 { get; set; }

        [JsonPropertyName("unknown16")]
        public ushort Unknown16 { get; set; }

        [JsonPropertyName("unknown18")]
        public ushort Unknown18 { get; set; }

        [JsonPropertyName("unknown1A")]
        public ushort Unknown1A { get; set; }

        [JsonPropertyName("name_id")]
        public string NameId { get; set; }

        [JsonPropertyName("hint_id")]
        public string HintId { get; set; }

        [JsonPropertyName("desc_id")]
        public string DescriptionId { get; set; }

        [JsonPropertyName("unknown28")]
        public uint Unknown28 { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("gpp")]
        public int GrossPanopticonProduct { get; set; }

        [JsonPropertyName("max_quantity")]
        public ushort MaxQuantity { get; set; }

        [JsonPropertyName("unknown36")]
        public ushort Unknown36 { get; set; }

        [JsonPropertyName("unknown38")]
        public ushort Unknown38 { get; set; }
        
        [JsonPropertyName("unknown3A")]
        public ushort Unknown3A { get; set; }

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this.IdNo = span.ReadValueU32(ref index, endian);
            this._IdOffset = span.ReadValueS32(ref index, endian);
            this.Type = (ItemType)span.ReadValueU16(ref index, endian);
            this.Unknown0A = span.ReadValueU16(ref index, endian);
            this.Unknown0C = span.ReadValueU32(ref index, endian);
            this.Unknown10 = span.ReadValueS16(ref index, endian);
            this.Unknown12 = span.ReadValueU16(ref index, endian);
            this.Unknown14 = span.ReadValueU16(ref index, endian);
            this.Unknown16 = span.ReadValueU16(ref index, endian);
            this.Unknown18 = span.ReadValueU16(ref index, endian);
            this.Unknown1A = span.ReadValueU16(ref index, endian);
            this._NameIdOffset = span.ReadValueS32(ref index, endian);
            this._HintIdOffset = span.ReadValueS32(ref index, endian);
            this._DescriptionIdOffset = span.ReadValueS32(ref index, endian);
            this.Unknown28 = span.ReadValueU32(ref index, endian);
            this.Price = span.ReadValueS32(ref index, endian);
            this.GrossPanopticonProduct = span.ReadValueS32(ref index, endian);
            this.MaxQuantity = span.ReadValueU16(ref index, endian);
            this.Unknown36 = span.ReadValueU16(ref index, endian);
            this.Unknown38 = span.ReadValueU16(ref index, endian);
            this.Unknown3A = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            this.Id = Helpers.ReadString(span, this._IdOffset);
            this.NameId = Helpers.ReadString(span, this._NameIdOffset);
            this.HintId = Helpers.ReadString(span, this._HintIdOffset);
            this.DescriptionId = Helpers.ReadString(span, this._DescriptionIdOffset);
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteValueU32(this.IdNo, endian);
            writer.WriteStringRef(this.Id, labeler);
            writer.WriteValueU16((ushort)this.Type, endian);
            writer.WriteValueU16(this.Unknown0A, endian);
            writer.WriteValueU32(this.Unknown0C, endian);
            writer.WriteValueS16(this.Unknown10, endian);
            writer.WriteValueU16(this.Unknown12, endian);
            writer.WriteValueU16(this.Unknown14, endian);
            writer.WriteValueU16(this.Unknown16, endian);
            writer.WriteValueU16(this.Unknown18, endian);
            writer.WriteValueU16(this.Unknown1A, endian);
            writer.WriteStringRef(this.NameId, labeler);
            writer.WriteStringRef(this.HintId, labeler);
            writer.WriteStringRef(this.DescriptionId, labeler);
            writer.WriteValueU32(this.Unknown28, endian);
            writer.WriteValueS32(this.Price, endian);
            writer.WriteValueS32(this.GrossPanopticonProduct, endian);
            writer.WriteValueU16(this.MaxQuantity, endian);
            writer.WriteValueU16(this.Unknown36, endian);
            writer.WriteValueU16(this.Unknown38, endian);
            writer.WriteValueU16(this.Unknown3A, endian);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
        }

        public override string ToString()
        {
            return $"id={this.IdNo}, key={this.Id}, type={this.Type}, u0A={this.Unknown0A}, u0C={this.Unknown0C}, u10={this.Unknown10}, u12={this.Unknown12}, u14={this.Unknown14}, u16={this.Unknown16}, u18={this.Unknown18}, u1A={this.Unknown1A}, name id={this.NameId}, hint id={this.HintId}, desc id={this.DescriptionId}, u28={this.Unknown28}, price={this.Price}, gpp={this.GrossPanopticonProduct}, max quantity={this.MaxQuantity}, u36={this.Unknown36}, u38={this.Unknown38}, u3A={this.Unknown3A}";
        }
    }
}

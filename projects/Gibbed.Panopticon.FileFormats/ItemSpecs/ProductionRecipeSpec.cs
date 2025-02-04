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
    using IItemSpec = ISpec<StringPool, ILabeler<StringPool>>;
    using IItemLabeler = ILabeler<StringPool>;

    public class ProductionRecipeSpec : IItemSpec
    {
        internal const int Size = 48;
        internal const int PaddingSize = 10;

        private int _ProductIdOffset;

        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("unknown04")]
        public ushort Unknown04 { get; set; }

        [JsonProperty("unknown06")]
        public ushort Unknown06 { get; set; }

        [JsonProperty("unknown08")]
        public ushort Unknown08 { get; set; }

        [JsonProperty("unknown0A")]
        public ushort Unknown0A { get; set; }

        [JsonProperty("unknown0C")]
        public ushort Unknown0C { get; set; }

        [JsonProperty("unknown0E")]
        public ushort Unknown0E { get; set; }

        [JsonProperty("unknown10")]
        public ushort Unknown10 { get; set; }

        [JsonProperty("unknown12")]
        public ushort Unknown12 { get; set; }

        [JsonProperty("unknown14")]
        public ushort Unknown14 { get; set; }

        [JsonProperty("unknown16")]
        public ushort Unknown16 { get; set; }

        [JsonProperty("unknown18")]
        public ushort Unknown18 { get; set; }

        [JsonProperty("unknown1A")]
        public ushort Unknown1A { get; set; }

        [JsonProperty("unknown1C")]
        public ushort Unknown1C { get; set; }

        [JsonProperty("unknown1E")]
        public ushort Unknown1E { get; set; }

        [JsonProperty("unknown20")]
        public ushort Unknown20 { get; set; }

        [JsonProperty("unknown22")]
        public ushort Unknown22 { get; set; }

        [JsonProperty("unknown24")]
        public ushort Unknown24 { get; set; }

        void IItemSpec.Load(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._ProductIdOffset = span.ReadValueS32(ref index, endian);
            this.Unknown04 = span.ReadValueU16(ref index, endian);
            this.Unknown06 = span.ReadValueU16(ref index, endian);
            this.Unknown08 = span.ReadValueU16(ref index, endian);
            this.Unknown0A = span.ReadValueU16(ref index, endian);
            this.Unknown0C = span.ReadValueU16(ref index, endian);
            this.Unknown0E = span.ReadValueU16(ref index, endian);
            this.Unknown10 = span.ReadValueU16(ref index, endian);
            this.Unknown12 = span.ReadValueU16(ref index, endian);
            this.Unknown14 = span.ReadValueU16(ref index, endian);
            this.Unknown16 = span.ReadValueU16(ref index, endian);
            this.Unknown18 = span.ReadValueU16(ref index, endian);
            this.Unknown1A = span.ReadValueU16(ref index, endian);
            this.Unknown1C = span.ReadValueU16(ref index, endian);
            this.Unknown1E = span.ReadValueU16(ref index, endian);
            this.Unknown20 = span.ReadValueU16(ref index, endian);
            this.Unknown22 = span.ReadValueU16(ref index, endian);
            this.Unknown24 = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void IItemSpec.PostLoad(ReadOnlySpan<byte> span, Endian endian)
        {
            this.ProductId = Helpers.ReadString(span, this._ProductIdOffset);
        }

        void IItemSpec.Save(IArrayBufferWriter<byte> writer, IItemLabeler labeler, Endian endian)
        {
            writer.WriteStringRef(this.ProductId, labeler);
            writer.WriteValueU16(this.Unknown04, endian);
            writer.WriteValueU16(this.Unknown06, endian);
            writer.WriteValueU16(this.Unknown08, endian);
            writer.WriteValueU16(this.Unknown0A, endian);
            writer.WriteValueU16(this.Unknown0C, endian);
            writer.WriteValueU16(this.Unknown0E, endian);
            writer.WriteValueU16(this.Unknown10, endian);
            writer.WriteValueU16(this.Unknown12, endian);
            writer.WriteValueU16(this.Unknown14, endian);
            writer.WriteValueU16(this.Unknown16, endian);
            writer.WriteValueU16(this.Unknown18, endian);
            writer.WriteValueU16(this.Unknown1A, endian);
            writer.WriteValueU16(this.Unknown1C, endian);
            writer.WriteValueU16(this.Unknown1E, endian);
            writer.WriteValueU16(this.Unknown20, endian);
            writer.WriteValueU16(this.Unknown22, endian);
            writer.WriteValueU16(this.Unknown24, endian);
            writer.SkipPadding(PaddingSize);
        }

        void IItemSpec.PostSave(IArrayBufferWriter<byte> writer, IItemLabeler labeler, Endian endian)
        {
        }

        public override string ToString()
        {
            return $"product id={this.ProductId}, u04={this.Unknown04}, u06={this.Unknown06}, u08={this.Unknown08}, u0A={this.Unknown0A}, u0C={this.Unknown0C}, u0E={this.Unknown0E}, u10={this.Unknown10}, u12={this.Unknown12}, u14={this.Unknown14}, u16={this.Unknown16}, u18={this.Unknown18}, u1A={this.Unknown1A}, u1C={this.Unknown1C}, u1E={this.Unknown1E}, u20={this.Unknown20}, u22={this.Unknown22}, u24={this.Unknown24}";
        }
    }
}

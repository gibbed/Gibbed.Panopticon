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

    public class ProductionRecipeSpec : ISpec
    {
        internal const int Size = 48;
        internal const int PaddingSize = 10;

        public const int ResultCount = 8;

        private int _ProductIdOffset;
        private readonly ProductionRecipeResult[] _Results;

        public ProductionRecipeSpec()
        {
            this._Results = new ProductionRecipeResult[ResultCount];
        }

        [JsonConstructor]
        private ProductionRecipeSpec(ProductionRecipeResult[] results)
            : this()
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            if (results.Length != ResultCount)
            {
                throw new ArgumentOutOfRangeException(nameof(results));
            }
            Array.Copy(results, this._Results, ResultCount);
        }

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("unknown04")]
        public ushort Unknown04 { get; set; }

        [JsonPropertyName("results")]
        public ProductionRecipeResult[] Results => this._Results;

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._ProductIdOffset = span.ReadValueS32(ref index, endian);
            this.Unknown04 = span.ReadValueU16(ref index, endian);
            var weightIndex = index;
            var quantityIndex = weightIndex + 2 * ResultCount;
            for (int i = 0; i < ResultCount; i++)
            {
                var result = this._Results[i];
                result.Weight = span.ReadValueU16(ref weightIndex, endian);
                result.Quantity = span.ReadValueU16(ref quantityIndex, endian);
                this._Results[i] = result;
            }
            index = quantityIndex;
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            this.ProductId = Helpers.ReadString(span, this._ProductIdOffset);
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteStringRef(this.ProductId, labeler);
            writer.WriteValueU16(this.Unknown04, endian);
            for (int i = 0; i < ResultCount; i++)
            {
                writer.WriteValueU16(this._Results[i].Weight, endian);
            }
            for (int i = 0; i < ResultCount; i++)
            {
                writer.WriteValueU16(this._Results[i].Quantity, endian);
            }
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
        }
    }
}

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

    public class Unknown80Spec : ISpec
    {
        internal const int Size = 32;
        internal const int PaddingSize = 14;

        public const int WeightCount = 8;

        private readonly ushort[] _Weights;

        public Unknown80Spec()
        {
            this._Weights = new ushort[WeightCount];
        }

        [JsonConstructor]
        private Unknown80Spec(ushort[] weights)
            : this()
        {
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }
            if (weights.Length != WeightCount)
            {
                throw new ArgumentOutOfRangeException(nameof(weights));
            }
            Array.Copy(weights, this._Weights, WeightCount);
        }

        [JsonPropertyName("weights")]
        public ushort[] Weights => this._Weights;

        [JsonPropertyName("unknown10")]
        public ushort Unknown10 { get; set; }

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            for (int i = 0; i < WeightCount; i++)
            {
                this._Weights[i] = span.ReadValueU16(ref index, endian);
            }
            this.Unknown10 = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            for (int i = 0; i < WeightCount; i++)
            {
                writer.WriteValueU16(this._Weights[i], endian);
            }
            writer.WriteValueU16(this.Unknown10, endian);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
        }
    }
}

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

    public class Unknown88Spec : ISpec
    {
        internal const int Size = 32;
        internal const int PaddingSize = 12;

        private int _WeaponIdOffset;
        private int _Unknown10Offset;

        [JsonProperty("weapon_id")]
        public string WeaponId { get; set; }

        [JsonProperty("unknown04")]
        public uint Unknown04 { get; set; }

        [JsonProperty("unknown08")]
        public ushort Unknown08 { get; set; }

        [JsonProperty("unknown0A")]
        public ushort Unknown0A { get; set; }

        [JsonProperty("unknown0C")]
        public ushort Unknown0C { get; set; }

        [JsonProperty("unknown10")]
        public string Unknown10 { get; set; }

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._WeaponIdOffset = span.ReadValueS32(ref index, endian);
            this.Unknown04 = span.ReadValueU32(ref index, endian);
            this.Unknown08 = span.ReadValueU16(ref index, endian);
            this.Unknown0A = span.ReadValueU16(ref index, endian);
            this.Unknown0C = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, 2);
            this._Unknown10Offset = span.ReadValueS32(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, Endian endian)
        {
            this.WeaponId = Helpers.ReadString(span, this._WeaponIdOffset);
            this.Unknown10 = Helpers.ReadString(span, this._Unknown10Offset);
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
            writer.WriteStringRef(this.WeaponId, labeler);
            writer.WriteValueU32(this.Unknown04, endian);
            writer.WriteValueU16(this.Unknown08, endian);
            writer.WriteValueU16(this.Unknown0A, endian);
            writer.WriteValueU16(this.Unknown0C, endian);
            writer.SkipPadding(2);
            writer.WriteStringRef(this.Unknown10, labeler);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
        }
    }
}

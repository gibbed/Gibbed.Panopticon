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
using Newtonsoft.Json.Converters;

namespace Gibbed.Panopticon.FileFormats.ItemSpecs
{
    using ISpec = ISpec<StringPool, ILabeler<StringPool>>;
    using ILabeler = ILabeler<StringPool>;

    public class UnknownC8Spec : ISpec
    {
        internal const int Size = 32;
        internal const int PaddingSize = 6;

        private int _Unknown00Offset;

        [JsonProperty("unknown00")]
        public string Unknown00 { get; set; }

        [JsonProperty("unknown04")]
        public uint Unknown04 { get; set; }

        [JsonProperty("unknown08")]
        public uint Unknown08 { get; set; }

        [JsonProperty("unknown0C")]
        public ushort Unknown0C { get; set; }

        [JsonProperty("unknown0E")]
        public ushort Unknown0E { get; set; }

        [JsonProperty("unknown10")]
        public ushort Unknown10 { get; set; }

        [JsonProperty("unknown12")]
        public ushort Unknown12 { get; set; }

        [JsonProperty("gender")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CitizenGender Gender { get; set; }

        [JsonProperty("unknown16")]
        public ushort Unknown16 { get; set; } // bool?

        [JsonProperty("unknown18")]
        public ushort Unknown18 { get; set; } // bool?

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._Unknown00Offset = span.ReadValueS32(ref index, endian);
            this.Unknown04 = span.ReadValueU32(ref index, endian);
            this.Unknown08 = span.ReadValueU32(ref index, endian);
            this.Unknown0C = span.ReadValueU16(ref index, endian);
            this.Unknown0E = span.ReadValueU16(ref index, endian);
            this.Unknown10 = span.ReadValueU16(ref index, endian);
            this.Unknown12 = span.ReadValueU16(ref index, endian);
            this.Gender = (CitizenGender)span.ReadValueU16(ref index, endian);
            this.Unknown16 = span.ReadValueU16(ref index, endian);
            this.Unknown18 = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            this.Unknown00 = Helpers.ReadString(span, this._Unknown00Offset);
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteStringRef(this.Unknown00, labeler);
            writer.WriteValueU32(this.Unknown04, endian);
            writer.WriteValueU32(this.Unknown08, endian);
            writer.WriteValueU16(this.Unknown0C, endian);
            writer.WriteValueU16(this.Unknown0E, endian);
            writer.WriteValueU16(this.Unknown10, endian);
            writer.WriteValueU16(this.Unknown12, endian);
            writer.WriteValueU16((ushort)this.Gender, endian);
            writer.WriteValueU16(this.Unknown16, endian);
            writer.WriteValueU16(this.Unknown18, endian);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
        }
    }
}

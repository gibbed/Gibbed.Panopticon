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

    public class CitizenFirstNameSpec : ISpec
    {
        internal const int Size = 16;
        internal const int PaddingSize = 8;

        private int _FirstNameOffset;

        [JsonProperty("name")]
        public string FirstName { get; set; }

        [JsonProperty("gender")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CitizenGender Gender { get; set; }

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._FirstNameOffset = span.ReadValueS32(ref index, endian);
            this.Gender = (CitizenGender)span.ReadValueU32(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, Endian endian)
        {
            this.FirstName = Helpers.ReadString(span, this._FirstNameOffset);
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
            writer.WriteStringRef(this.FirstName, labeler, StringPool.FirstName);
            writer.WriteValueU32((uint)this.Gender, endian);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
        }

        public override string ToString()
        {
            return $"{this.FirstName} ({this.Gender})";
        }
    }
}

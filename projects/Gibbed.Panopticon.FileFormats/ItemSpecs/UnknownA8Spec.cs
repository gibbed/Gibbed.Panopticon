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
using Gibbed.Panopticon.Common;

namespace Gibbed.Panopticon.FileFormats.ItemSpecs
{
    using ILabeler = ILabeler<StringPool>;
    using ISpec = ISpec<StringPool, ILabeler<StringPool>>;

    public class UnknownA8Spec : ISpec
    {
        internal const int Size = 16;
        internal const int PaddingSize = 4;

        private int _Unknown00Offset;

        [JsonPropertyName("unknown00")]
        public string Unknown00 { get; set; }

        [JsonPropertyName("unknown04")]
        public ushort Unknown04 { get; set; }

        [JsonPropertyName("unknown06")]
        public ushort Unknown06 { get; set; }

        [JsonPropertyName("unknown08")]
        public ushort Unknown08 { get; set; }

        [JsonPropertyName("unknown0A")]
        public ushort Unknown0A { get; set; }

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._Unknown00Offset = span.ReadValueS32(ref index, endian);
            this.Unknown04 = span.ReadValueU16(ref index, endian);
            this.Unknown06 = span.ReadValueU16(ref index, endian);
            this.Unknown08 = span.ReadValueU16(ref index, endian);
            this.Unknown0A = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            this.Unknown00 = Helpers.ReadString(span, this._Unknown00Offset);
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteStringRef(this.Unknown00, labeler);
            writer.WriteValueU16(this.Unknown04, endian);
            writer.WriteValueU16(this.Unknown06, endian);
            writer.WriteValueU16(this.Unknown08, endian);
            writer.WriteValueU16(this.Unknown0A, endian);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
        }
    }
}

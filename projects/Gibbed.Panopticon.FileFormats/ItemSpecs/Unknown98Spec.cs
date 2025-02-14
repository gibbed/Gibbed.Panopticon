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
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Gibbed.Buffers;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;

namespace Gibbed.Panopticon.FileFormats.ItemSpecs
{
    using ILabeler = ILabeler<StringPool>;
    using ISpec = ISpec<StringPool, ILabeler<StringPool>>;

    public class Unknown98Spec : ISpec
    {
        internal const int Size = 16;
        internal const int PaddingSize = 4;

        private readonly TableInfo<Unknown98EntrySpec> _Unknown04;

        public Unknown98Spec()
        {
            this._Unknown04 = new();
        }

        [JsonPropertyName("unknown00")]
        public uint Unknown00 { get; set; }

        [JsonPropertyName("unknown04")]
        public List<Unknown98EntrySpec> Unknown04 { get; set; }

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this.Unknown00 = span.ReadValueU32(ref index, endian);
            this._Unknown04.Read(span, ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            this.Unknown04 = this._Unknown04.LoadTable(span, version, endian);
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteValueU32(this.Unknown00, endian);
            this._Unknown04.Write(writer, labeler, endian);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            this._Unknown04.SaveTable(this.Unknown04, writer, labeler, version, endian);
        }
    }
}

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
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Gibbed.Buffers;
using Gibbed.Memory;

namespace Gibbed.Panopticon.FileFormats.ItemSpecs
{
    using ILabeler = ILabeler<StringPool>;
    using ISpec = ISpec<StringPool, ILabeler<StringPool>>;

    public class FieldItemLotTableSpec : ISpec
    {
        internal const int Size = 16;

        private int _FieldIdOffset;
        private readonly TableInfo<DropItemLotSpec> _Lots;

        public FieldItemLotTableSpec()
        {
            this._Lots = new();
        }

        [JsonPropertyName("field_id")]
        public string FieldId { get; set; }

        [JsonPropertyName("unknown04")]
        public uint Unknown04 { get; set; }

        [JsonPropertyName("lots")]
        public List<DropItemLotSpec> Lots { get; set; }

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, GameVersion version, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this._FieldIdOffset = span.ReadValueS32(ref index, endian);
            this.Unknown04 = span.ReadValueU32(ref index, endian);
            this._Lots.Read(span, ref index, endian);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            this.FieldId = Helpers.ReadString(span, this._FieldIdOffset);
            this.Lots = this._Lots.LoadTable(span, version, endian);
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            writer.WriteStringRef(this.FieldId, labeler);
            writer.WriteValueU32(this.Unknown04, endian);
            this._Lots.Write(writer, labeler, endian);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            this._Lots.SaveTable(this.Lots, writer, labeler, version, endian);
        }

        public override string ToString()
        {
            return $"field id={this.FieldId}, u04={this.Unknown04}";
        }
    }
}

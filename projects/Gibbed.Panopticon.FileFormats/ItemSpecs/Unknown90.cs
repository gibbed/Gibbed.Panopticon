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

    public class Unknown90 : IItemSpec
    {
        internal const int Size = 32;
        internal const int PaddingSize = 6;

        public const int UnknownCount = 3;

        private readonly int[] _Unknown00Offsets;
        private readonly Unknown90Entry[] _Entries;

        public Unknown90()
        {
            this._Unknown00Offsets = new int[UnknownCount];
            this._Entries = new Unknown90Entry[UnknownCount];
        }

        [JsonConstructor]
        private Unknown90(Unknown90Entry[] entries)
            : this()
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }
            if (entries.Length != UnknownCount)
            {
                throw new ArgumentOutOfRangeException(nameof(entries));
            }
            Array.Copy(entries, this._Entries, UnknownCount);
        }

        [JsonProperty("id")]
        public ushort Id { get; set; }

        [JsonProperty("entries")]
        public Unknown90Entry[] Entries => this._Entries;

        void IItemSpec.Load(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            int unknown00Index = index;
            int weightIndex = unknown00Index + 4 * UnknownCount + 2;
            int unknown14Index = weightIndex + 2 * UnknownCount;
            for (int i = 0; i < UnknownCount; i++)
            {
                this._Unknown00Offsets[i] = span.ReadValueS32(ref unknown00Index, endian);
                Unknown90Entry entry;
                entry.Unknown00 = default;
                entry.Weight = span.ReadValueU16(ref weightIndex, endian);
                entry.Unknown14 = span.ReadValueU16(ref unknown14Index, endian);
                this._Entries[i] = entry;
            }
            this.Id = span.ReadValueU16(ref unknown00Index, endian);
            index = unknown14Index;
            span.SkipPadding(ref index, PaddingSize);
        }

        void IItemSpec.PostLoad(ReadOnlySpan<byte> span, Endian endian)
        {
            for (int i = 0; i < UnknownCount; i++)
            {
                var entry = this._Entries[i];
                entry.Unknown00 = Helpers.ReadString(span, this._Unknown00Offsets[i]);
                this._Entries[i] = entry;
            }
        }

        void IItemSpec.Save(IArrayBufferWriter<byte> writer, IItemLabeler labeler, Endian endian)
        {
            for (int i = 0; i < UnknownCount; i++)
            {
                writer.WriteStringRef(this._Entries[i].Unknown00, labeler);
            }
            writer.WriteValueU16(this.Id, endian);
            for (int i = 0; i < UnknownCount; i++)
            {
                writer.WriteValueU16(this._Entries[i].Weight, endian);
            }
            for (int i = 0; i < UnknownCount; i++)
            {
                writer.WriteValueU16(this._Entries[i].Unknown14, endian);
            }
            writer.SkipPadding(PaddingSize);
        }

        void IItemSpec.PostSave(IArrayBufferWriter<byte> writer, IItemLabeler labeler, Endian endian)
        {
        }
    }
}

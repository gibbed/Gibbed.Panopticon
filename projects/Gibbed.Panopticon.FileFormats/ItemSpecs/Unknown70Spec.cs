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

    public class Unknown70Spec : ISpec
    {
        internal const int Size = 64;
        internal const int PaddingSize = 6;

        public const int UnknownCount = 6;

        private readonly int[] _Unknown04Offsets;
        private readonly Unknown70Entry[] _Unknown04;
        private int _Unknown28Offset;
        private int _Unknown2COffset;

        public Unknown70Spec()
        {
            this._Unknown04Offsets = new int[UnknownCount];
            this._Unknown04 = new Unknown70Entry[UnknownCount];
        }

        [JsonConstructor]
        private Unknown70Spec(Unknown70Entry[] unknown04)
            : this()
        {
            if (unknown04 == null)
            {
                throw new ArgumentNullException(nameof(unknown04));
            }
            if (unknown04.Length != UnknownCount)
            {
                throw new ArgumentOutOfRangeException(nameof(unknown04));
            }
            Array.Copy(unknown04, this._Unknown04, UnknownCount);
        }

        [JsonProperty("id")]
        public uint Id { get; set; }

        [JsonProperty("unknown04")]
        public Unknown70Entry[] Unknown04 => this._Unknown04;

        [JsonProperty("unknown28")]
        public string Unknown28 { get; set; } // bonus item id?

        [JsonProperty("unknown2C")]
        public string Unknown2C { get; set; } // another item id?

        [JsonProperty("unknown30")]
        public ushort Unknown30 { get; set; } // bonus item quantity?

        [JsonProperty("reward_citizen_lot_id")]
        public ushort RewardCitizenLotId { get; set; }

        [JsonProperty("unknown34")]
        public ushort Unknown34 { get; set; } // bonus type? 0-4

        // 0 = ID_BTU_RSL_P900 = none
        // 1 = ID_BTU_RSL_P901 = No Sustainability lost
        // 2 = ID_BTU_RSL_P902 = All large Abductors destroyed
        // 3 = ID_BTU_RSL_P903 = All resources harvested
        // 4 = ID_BTU_RSL_P904 = Minimal Accessory downtime

        [JsonProperty("unknown36")]
        public ushort Unknown36 { get; set; }

        [JsonProperty("unknown38")]
        public ushort Unknown38 { get; set; } // id to Unknown90

        void ISpec.Load(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this.Id = span.ReadValueU32(ref index, endian);
            var unknown04Index = index;
            var unknown0CIndex = unknown04Index + 4 * UnknownCount;
            for (int i = 0; i < UnknownCount; i++)
            {
                this._Unknown04Offsets[i] = span.ReadValueS32(ref unknown04Index, endian);
                Unknown70Entry entry;
                entry.Unknown04 = default;
                entry.Unknown0C = span.ReadValueU16(ref unknown0CIndex, endian);
                this._Unknown04[i] = entry;
            }
            index = unknown0CIndex;
            this._Unknown28Offset = span.ReadValueS32(ref index, endian);
            this._Unknown2COffset = span.ReadValueS32(ref index, endian);
            this.Unknown30 = span.ReadValueU16(ref index, endian);
            this.RewardCitizenLotId = span.ReadValueU16(ref index, endian);
            this.Unknown34 = span.ReadValueU16(ref index, endian);
            this.Unknown36 = span.ReadValueU16(ref index, endian);
            this.Unknown38 = span.ReadValueU16(ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
        }

        void ISpec.PostLoad(ReadOnlySpan<byte> span, Endian endian)
        {
            for (int i = 0; i < UnknownCount; i++)
            {
                var entry = this._Unknown04[i];
                entry.Unknown04 = Helpers.ReadString(span, this._Unknown04Offsets[i]);
                this._Unknown04[i] = entry;
            }
            this.Unknown28 = Helpers.ReadString(span, this._Unknown28Offset);
            this.Unknown2C = Helpers.ReadString(span, this._Unknown2COffset);
        }

        void ISpec.Save(IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
            writer.WriteValueU32(this.Id, endian);
            for (int i = 0; i < UnknownCount; i++)
            {
                writer.WriteStringRef(this._Unknown04[i].Unknown04, labeler);
            }
            for (int i = 0; i < UnknownCount; i++)
            {
                writer.WriteValueU16(this._Unknown04[i].Unknown0C, endian);
            }
            writer.WriteStringRef(this.Unknown28, labeler);
            writer.WriteStringRef(this.Unknown2C, labeler);
            writer.WriteValueU16(this.Unknown30, endian);
            writer.WriteValueU16(this.RewardCitizenLotId, endian);
            writer.WriteValueU16(this.Unknown34, endian);
            writer.WriteValueU16(this.Unknown36, endian);
            writer.WriteValueU16(this.Unknown38, endian);
            writer.SkipPadding(PaddingSize);
        }

        void ISpec.PostSave(IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
        }
    }
}

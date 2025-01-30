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
using System.Buffers;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;

namespace Gibbed.Panopticon.FileFormats.LanguageMessages
{
    internal struct FileHeader
    {
        public const int Size = 16;

        public const uint Signature = 0x474E4C23u; // '#LNG'

        public Endian Endian;
        public int MessageCount;
        public int MessageTableOffset;

        internal static FileHeader Read(ReadOnlySpan<byte> span, ref int index)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            var magic = span.ReadValueU32(ref index, Endian.Little);
            if (magic != Signature)
            {
                throw new FormatException("unexpected signature");
            }

            var bom = span.ReadValueU16(ref index, Endian.Little);
            if (bom != 0xFFFE && bom != 0xFEFF)
            {
                throw new FormatException("unexpected bom");
            }
            var endian = bom == 0xFFFE ? Endian.Little : Endian.Big;

            span.SkipPadding(ref index, 2);

            var messageCount = span.ReadValueS32(ref index, endian);
            var messageTableOffset = span.ReadValueS32(ref index, endian);

            FileHeader instance;
            instance.Endian = endian;
            instance.MessageCount = messageCount;
            instance.MessageTableOffset = messageTableOffset;
            return instance;
        }

        internal static void Write(FileHeader instance, IBufferWriter<byte> writer)
        {
            var endian = instance.Endian;
            writer.WriteValueU32(Signature, endian);
            writer.WriteValueU16((ushort)0xFFFEu, endian);
            writer.WriteValueU16(0, endian);
            writer.WriteValueS32(instance.MessageCount, endian);
            writer.WriteValueS32(instance.MessageTableOffset, endian);
        }

        internal void Write(IBufferWriter<byte> writer)
        {
            Write(this, writer);
        }
    }
}

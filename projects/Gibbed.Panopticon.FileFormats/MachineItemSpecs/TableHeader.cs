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

namespace Gibbed.Panopticon.FileFormats.MachineItemSpecs
{
    using ILabeler = ILabeler<StringPool>;

    internal class TableHeader
    {
        internal const int Size = 8;

        public int Count;
        public int Offset;

        public ILabel<int> CountLabel;
        public ILabel<int> OffsetLabel;

        internal void Read(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            this.Count = span.ReadValueS32(ref index, endian);
            this.Offset = span.ReadValueS32(ref index, endian);
        }

        internal void Write(IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
            this.CountLabel = writer.WritePointer(labeler);
            this.OffsetLabel = writer.WritePointer(labeler);
        }

        internal void Set(int count, int offset)
        {
            this.CountLabel.Set(count);
            this.OffsetLabel.Set(offset);
        }
    }
}

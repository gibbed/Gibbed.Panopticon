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
using Gibbed.Panopticon.Common.Math;

namespace Gibbed.Panopticon.FileFormats.Models
{
    internal struct MeshHeader
    {
        internal const int Size = 60;

        public AABB AABB;
        public int NameOffset;
        public ushort Unknown; // 34
        public ushort SubmeshCount;
        public int SubmeshOffsetTableOffset;

        internal static MeshHeader Read(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            MeshHeader instance;
            instance.AABB = AABB.Read(span, ref index, endian);
            instance.NameOffset = span.ReadValueS32(ref index, endian);
            instance.Unknown = span.ReadValueU16(ref index, endian);
            instance.SubmeshCount = span.ReadValueU16(ref index, endian);
            instance.SubmeshOffsetTableOffset = span.ReadValueS32(ref index, endian);
            return instance;
        }

        internal static void Write(MeshHeader instance, IArrayBufferWriter<byte> writer, Endian endian)
        {
            throw new NotImplementedException();
        }

        internal readonly void Write(IArrayBufferWriter<byte> writer, Endian endian)
        {
            Write(this, writer, endian);
        }
    }
}

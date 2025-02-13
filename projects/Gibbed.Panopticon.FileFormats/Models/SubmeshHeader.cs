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
    internal struct SubmeshHeader
    {
        internal static int Size(FileVersion version) => version.IsNew == true ? 80 : 76;

        public AABB AABB;
        public uint VertexType;
        public int VertexSize;
        public int VertexCount;
        public int VertexBufferOffset;
        public int NameOffset;
        public PrimitiveType PrimitiveType;
        public byte MaterialCount;
        public ushort IndexBufferCount;
        public int MaterialDataOffset;
        public int IndexBufferOffsetTableOffset;

        internal static SubmeshHeader Read(ReadOnlySpan<byte> span, ref int index, FileVersion version, Endian endian)
        {
            if (span.Length < Size(version))
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            SubmeshHeader instance;
            instance.AABB = AABB.Read(span, ref index, endian);
            instance.VertexType = span.ReadValueU32(ref index, endian);
            instance.VertexSize = version.IsNew == true
                ? span.ReadValueS32(ref index, endian)
                : span.ReadValueU16(ref index, endian);
            instance.VertexCount = version.IsNew == true
                ? span.ReadValueS32(ref index, endian)
                : span.ReadValueU16(ref index, endian);
            instance.VertexBufferOffset = span.ReadValueS32(ref index, endian);
            instance.NameOffset = span.ReadValueS32(ref index, endian);
            instance.PrimitiveType = (PrimitiveType)span.ReadValueU8(ref index);
            instance.MaterialCount = span.ReadValueU8(ref index);
            instance.IndexBufferCount = span.ReadValueU16(ref index, endian);
            instance.MaterialDataOffset = span.ReadValueS32(ref index, endian);
            instance.IndexBufferOffsetTableOffset = span.ReadValueS32(ref index, endian);
            return instance;
        }

        internal static void Write(SubmeshHeader instance, IArrayBufferWriter<byte> writer, FileVersion version, Endian endian)
        {
            throw new NotImplementedException();
        }

        internal readonly void Write(IArrayBufferWriter<byte> writer, FileVersion version, Endian endian)
        {
            Write(this, writer, version, endian);
        }
    }
}

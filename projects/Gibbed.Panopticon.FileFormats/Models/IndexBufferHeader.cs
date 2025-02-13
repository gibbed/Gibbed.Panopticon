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

namespace Gibbed.Panopticon.FileFormats.Models
{
    internal struct IndexBufferHeader
    {
        internal static int Size(FileVersion version) => version.IsNew == true ? 16 : 12;

        public int IndexCount;
        public int BoneCount;
        public int DataOffset;
        public int BoneNameOffsetTableOffset;

        internal static IndexBufferHeader Read(ReadOnlySpan<byte> span, ref int index, FileVersion version, Endian endian)
        {
            if (span.Length < Size(version))
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            IndexBufferHeader instance;
            instance.IndexCount = version.IsNew == true
                ? span.ReadValueS32(ref index, endian)
                : span.ReadValueU16(ref index, endian);
            instance.BoneCount = version.IsNew == true
                ? span.ReadValueS32(ref index, endian)
                : span.ReadValueU16(ref index, endian);
            instance.DataOffset = span.ReadValueS32(ref index, endian);
            instance.BoneNameOffsetTableOffset = span.ReadValueS32(ref index, endian);
            return instance;
        }

        internal static void Write(IndexBufferHeader instance, IArrayBufferWriter<byte> writer, FileVersion version, Endian endian)
        {
            throw new NotImplementedException();
        }

        internal readonly void Write(IArrayBufferWriter<byte> writer, FileVersion version, Endian endian)
        {
            Write(this, writer, version, endian);
        }
    }
}

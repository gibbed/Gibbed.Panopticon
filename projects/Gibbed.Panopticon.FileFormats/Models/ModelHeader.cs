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
using Gibbed.Buffers;
using Gibbed.Memory;

namespace Gibbed.Panopticon.FileFormats.Models
{
    internal struct ModelHeader
    {
        internal const int Size = 8;

        public ushort Unknown; // 00
        public ushort MeshCount;
        public int MeshOffsetTableOffset;

        internal static ModelHeader Read(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            ModelHeader instance;
            instance.Unknown = span.ReadValueU16(ref index, endian);
            instance.MeshCount = span.ReadValueU16(ref index, endian);
            instance.MeshOffsetTableOffset = span.ReadValueS32(ref index, endian);
            return instance;
        }

        internal static void Write(ModelHeader instance, IArrayBufferWriter<byte> writer, Endian endian)
        {
            throw new NotImplementedException();
        }

        internal readonly void Write(IArrayBufferWriter<byte> writer, Endian endian)
        {
            Write(this, writer, endian);
        }
    }
}

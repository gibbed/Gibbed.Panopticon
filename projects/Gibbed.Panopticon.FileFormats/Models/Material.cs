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
    public struct Material
    {
        internal const int Size = 12;

        public byte Unknown0;
        public byte TextureIndex;
        public byte Unknown2;
        public byte Unknown3;
        public float Unknown4;
        public float Unknown8;

        public byte Unknown2_0
        {
            get => (byte)((this.Unknown2 >> 0) & 0xF);
        }

        public byte Unknown2_4
        {
            get => (byte)((this.Unknown2 >> 4) & 0xF);
        }

        public byte Unknown3_0
        {
            get => (byte)((this.Unknown3 >> 0) & 0xF);
        }

        public byte Unknown3_4
        {
            get => (byte)((this.Unknown3 >> 4) & 0xF);
        }

        internal static Material Read(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            Material instance;
            instance.Unknown0 = span.ReadValueU8(ref index);
            instance.TextureIndex = span.ReadValueU8(ref index);
            instance.Unknown2 = span.ReadValueU8(ref index);
            instance.Unknown3 = span.ReadValueU8(ref index);
            instance.Unknown4 = span.ReadValueF32(ref index, endian);
            instance.Unknown8 = span.ReadValueF32(ref index, endian);
            return instance;
        }

        internal static void Write(Material instance, IArrayBufferWriter<byte> writer, Endian endian)
        {
            throw new NotImplementedException();
        }

        internal readonly void Write(IArrayBufferWriter<byte> writer, Endian endian)
        {
            Write(this, writer, endian);
        }
    }
}

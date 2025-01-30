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
using System.Text;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;

namespace Gibbed.Panopticon.FileFormats.Archives
{
    public struct Entry
    {
        public const int Size = 288;

        public static readonly Encoding Encoding = Encoding.ASCII;

        public string Path;
        public long DataOffset;
        public uint DataSize;

        internal static Entry Read(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            Entry instance;
            instance.Path = span.ReadString(ref index, 256, Encoding, true);
            instance.DataOffset = span.ReadValueS64(ref index, endian);
            instance.DataSize = span.ReadValueU32(ref index, endian);
            span.SkipPadding(ref index, 20);
            return instance;
        }

        internal static void Write(Entry instance, IBufferWriter<byte> writer, Endian endian)
        {
            writer.WriteString(instance.Path, 256, Encoding);
            writer.WriteValueS64(instance.DataOffset, endian);
            writer.WriteValueU32(instance.DataSize, endian);
            writer.SkipPadding(20);
        }

        internal void Write(IBufferWriter<byte> writer, Endian endian)
        {
            Write(this, writer, endian);
        }
    }
}

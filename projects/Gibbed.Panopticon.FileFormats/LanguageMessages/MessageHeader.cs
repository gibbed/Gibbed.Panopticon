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

namespace Gibbed.Panopticon.FileFormats.LanguageMessages
{
    internal struct MessageHeader
    {
        internal const int Size = 48;

        internal static readonly Encoding KeyEncoding = Encoding.ASCII;

        public uint Id;
        public uint Id2;
        public string Key;
        public int ValueOffset;

        internal static MessageHeader Read(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            MessageHeader instance;
            instance.Id = span.ReadValueU32(ref index, endian);
            instance.Id2 = span.ReadValueU32(ref index, endian);
            span.SkipPadding(ref index, 4);
            instance.Key = span.ReadString(ref index, 32, KeyEncoding, true);
            instance.ValueOffset = span.ReadValueS32(ref index, endian);
            return instance;
        }

        internal static void Write(MessageHeader instance, IBufferWriter<byte> writer, Endian endian)
        {
            writer.WriteValueU32(instance.Id, endian);
            writer.WriteValueU32(instance.Id2, endian);
            writer.SkipPadding(4);
            writer.WriteString(instance.Key, 32, KeyEncoding);
            writer.WriteValueS32(instance.ValueOffset, endian);
        }

        internal void Write(IBufferWriter<byte> writer, Endian endian)
        {
            Write(this, writer, endian);
        }
    }
}

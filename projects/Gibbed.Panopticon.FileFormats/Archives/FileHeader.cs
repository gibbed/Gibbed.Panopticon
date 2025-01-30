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

namespace Gibbed.Panopticon.FileFormats.Archives
{
    internal struct FileHeader
    {
        public const int Size = 128;

        public const uint Signature = 0x46414523u; // '#EAF' probably "Eva Archive File" or "Em Archive File"
        public const ushort ExpectedVersion = (ushort)0x9300u;

        public Endian Endian;
        public long TotalSize;
        public int EntryCount;

        internal static FileHeader Read(ReadOnlySpan<byte> span, ref int index)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            var magic = span.ReadValueU32(ref index, Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException("unexpected signature");
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var version = span.ReadValueU16(ref index, endian);
            if (version != ExpectedVersion && version != 0)
            {
                throw new FormatException($"unsupported version {version:X} (expected {ExpectedVersion:X})");
            }

            var unknown06 = span.ReadValueU16(ref index, endian);
            if (unknown06 != 0u && unknown06 != 1u)
            {
                throw new FormatException("u06 is non-zero");
            }

            var totalSize = span.ReadValueS64(ref index, endian);
            if (totalSize < 0)
            {
                throw new FormatException("total size is negative");
            }

            var entryCount = span.ReadValueS32(ref index, endian);

            var unknown14 = span.ReadValueU32(ref index, endian);
            if (unknown14 != 1u)
            {
                throw new FormatException("u14 is not one");
            }

            span.SkipPadding(ref index, 104);

            FileHeader instance;
            instance.Endian = endian;
            instance.TotalSize = totalSize;
            instance.EntryCount = entryCount;
            return instance;
        }

        internal static void Write(FileHeader instance, IBufferWriter<byte> writer)
        {
            var endian = instance.Endian;
            writer.WriteValueU32(Signature, endian);
            writer.WriteValueU16(ExpectedVersion, endian);
            writer.WriteValueU16(0, endian);
            writer.WriteValueS64(instance.TotalSize, endian);
            writer.WriteValueS32(instance.EntryCount, endian);
            writer.WriteValueU32(1, endian);
            writer.SkipPadding(104);
        }

        internal void Write(IBufferWriter<byte> writer)
        {
            Write(this, writer);
        }
    }
}

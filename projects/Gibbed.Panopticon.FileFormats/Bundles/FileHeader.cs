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

namespace Gibbed.Panopticon.FileFormats.Bundles
{
    internal struct FileHeader
    {
        public const int Size = 32;

        public const uint Signature = 0x424D4523u; // '#EMB'
        public const ushort ExpectedVersion = (ushort)0x9300u;

        public Endian Endian;
        public uint EntryCount;
        public uint EntryTableOffset;
        public uint NameTableOffset;

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

            var unknown06 = span.ReadValueU16(ref index, endian);
            if (unknown06 != 32)
            {
                throw new FormatException("u06 is not 32");
            }

            var version = span.ReadValueU16(ref index, endian);
            if (version != ExpectedVersion)
            {
                throw new FormatException($"unsupported version {version:X} (expected {ExpectedVersion:X})");
            }

            var unknown0A = span.ReadValueU16(ref index, endian);
            if (unknown0A != 0u)
            {
                throw new FormatException("u06 is non-zero");
            }

            var entryCount = span.ReadValueU32(ref index, endian);

            var unknown10 = span.ReadValueU32(ref index, endian);
            if (unknown10 != 0u)
            {
                throw new FormatException("u10 is not zero");
            }

            var unknown14 = span.ReadValueU32(ref index, endian);
            if (unknown14 != 0u)
            {
                throw new FormatException("u14 is not zero");
            }

            var entryTableOffset = span.ReadValueU32(ref index, endian);
            var nameTableOffset = span.ReadValueU32(ref index, endian);

            FileHeader instance;
            instance.Endian = endian;
            instance.EntryCount = entryCount;
            instance.EntryTableOffset = entryTableOffset;
            instance.NameTableOffset = nameTableOffset;
            return instance;
        }

        internal static void Write(FileHeader instance, IBufferWriter<byte> writer)
        {
            var endian = instance.Endian;
            throw new NotImplementedException();
        }

        internal void Write(IBufferWriter<byte> writer)
        {
            Write(this, writer);
        }
    }
}

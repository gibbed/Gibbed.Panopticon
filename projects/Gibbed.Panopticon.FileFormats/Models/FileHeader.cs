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
using Gibbed.Panopticon.Common;

namespace Gibbed.Panopticon.FileFormats.Models
{
    internal class FileHeader
    {
        public const int Size = 28;

        internal const uint SignatureOld = 0x444D4523u; // '#EMD'
        internal const uint Signature = 0x444D4E23u; // '#NMD'

        public Endian Endian;
        public FileVersion Version;
        public ushort ModelCount;
        public int ModelOffsetTableOffset;
        public int ModelNameOffsetTableOffset;

        internal static FileHeader Read(ReadOnlySpan<byte> span, ref int index)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            var magic = span.ReadValueU32(ref index, Endian.Little);
            if (magic != Signature && magic != SignatureOld)
            {
                throw new FormatException("unexpected signature");
            }

            var bom = span.ReadValueU16(ref index, Endian.Little);
            if (bom != 0xFFFE && bom != 0xFEFF)
            {
                throw new FormatException("unexpected bom");
            }
            var endian = bom == 0xFFFE ? Endian.Little : Endian.Big;

            var headerSize = span.ReadValueU16(ref index, endian);
            if (headerSize != Size)
            {
                throw new FormatException("unexpected header size");
            }

            var version = FileVersion.Read(span, ref index, endian);
            if (version != FileVersion.VersionLegacy &&
                version != FileVersion.Version10 &&
                version != FileVersion.Version11)
            {
                throw new FormatException("unexpected version");
            }

            span.SkipPadding(ref index, 6);
            var modelCount = span.ReadValueU16(ref index, endian);
            var modelOffsetTableOffset = span.ReadValueS32(ref index, endian);
            var modelNameOffsetTableOffset = span.ReadValueS32(ref index, endian);

            FileHeader instance = new();
            instance.Endian = endian;
            instance.Version = version;
            instance.ModelCount = modelCount;
            instance.ModelOffsetTableOffset = modelOffsetTableOffset;
            instance.ModelNameOffsetTableOffset = modelNameOffsetTableOffset;
            return instance;
        }

        internal static void Write(FileHeader instance, IArrayBufferWriter<byte> writer)
        {
            var endian = instance.Endian;
            throw new NotImplementedException();
        }

        internal void Write(IArrayBufferWriter<byte> writer)
        {
            Write(this, writer);
        }
    }
}

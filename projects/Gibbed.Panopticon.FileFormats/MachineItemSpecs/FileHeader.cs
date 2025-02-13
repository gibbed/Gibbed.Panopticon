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
using Gibbed.Panopticon.Common;

namespace Gibbed.Panopticon.FileFormats.MachineItemSpecs
{
    using ILabeler = ILabeler<StringPool>;

    internal class FileHeader
    {
        public const int Size = 16;
        public const int PaddingSize = 8;

        public const uint Signature = 0x53494D23u; // '#MIS'

        public Endian Endian;
        public readonly TableInfo<MachineItemLotSpec> Unknown08s = new();
        public readonly TableInfo<MachineItemLotSpec> Unknown10s = new();

        public static FileHeader Read(ReadOnlySpan<byte> span, ref int index)
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

            span.SkipPadding(ref index, 2);

            FileHeader instance = new();
            instance.Endian = endian;
            instance.Unknown08s.Read(span, ref index, endian);
            instance.Unknown10s.Read(span, ref index, endian);
            span.SkipPadding(ref index, PaddingSize);
            return instance;
        }

        internal static void Write(FileHeader instance, IArrayBufferWriter<byte> writer, ILabeler labeler)
        {
            var endian = instance.Endian;
            writer.WriteValueU32(Signature, endian);
            writer.WriteValueU16((ushort)0xFFFEu, endian);
            writer.WriteValueU16(0, endian);
            instance.Unknown08s.Write(writer, labeler, endian);
            instance.Unknown10s.Write(writer, labeler, endian);
            writer.SkipPadding(PaddingSize);
        }

        internal void Write(IArrayBufferWriter<byte> writer, ILabeler labeler)
        {
            Write(this, writer, labeler);
        }
    }
}

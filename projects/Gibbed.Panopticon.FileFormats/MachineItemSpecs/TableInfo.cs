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
using System.Collections.Generic;
using Gibbed.Buffers;
using Gibbed.Memory;

namespace Gibbed.Panopticon.FileFormats.MachineItemSpecs
{
    using ISpec = ISpec<StringPool, ILabeler<StringPool>>;
    using ILabeler = ILabeler<StringPool>;

    internal class TableInfo<T>
        where T : ISpec, new()
    {
        public readonly TableHeader Header;

        public TableInfo()
        {
            this.Header = new();
        }

        internal void Set(int count, int offset)
        {
            this.Header.Set(count, offset);
        }

        internal void Read(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            this.Header.Read(span, ref index, endian);
        }

        internal void Write(IArrayBufferWriter<byte> writer, ILabeler labeler, Endian endian)
        {
            this.Header.Write(writer, labeler, endian);
        }

        public List<T> LoadTable(ReadOnlySpan<byte> span, GameVersion version, Endian endian)
        {
            var count = this.Header.Count;
            List<T> list = new(count);
            int index = this.Header.Offset;
            for (int i = 0; i < count; i++)
            {
                T instance;
                ISpec spec = instance = new();
                spec.Load(span, ref index, version, endian);
                list.Add(instance);
            }
            for (int i = 0; i < count; i++)
            {
                ISpec spec = list[i];
                spec.PostLoad(span, version, endian );
            }
            return list;
        }

        public void SaveTable(IList<T> list, IArrayBufferWriter<byte> writer, ILabeler labeler, GameVersion version, Endian endian)
        {
            var count = list.Count;
            this.Header.Set(count, writer.WrittenCount);
            for (int i = 0; i < count; i++)
            {
                T instance;
                ISpec spec = instance = list[i];
                spec.Save(writer, labeler, version, endian);
            }
            for (int i = 0; i < count; i++)
            {
                ISpec spec = list[i];
                spec.PostSave(writer, labeler, version, endian);
            }
        }
    }
}

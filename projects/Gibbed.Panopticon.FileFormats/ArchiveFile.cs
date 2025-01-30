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

using System.Collections.Generic;
using System.IO;
using Gibbed.Panopticon.Common;
using Gibbed.Panopticon.FileFormats.Archives;
using Endian = Gibbed.Memory.Endian;

namespace Gibbed.Panopticon.FileFormats
{
    public class ArchiveFile
    {
        private readonly List<Entry> _Entries;

        public ArchiveFile()
        {
            this._Entries = new();
        }

        public Endian Endian { get; set; }
        public long TotalSize { get; set; }
        public List<Entry> Entries => this._Entries;

        public static int EstimateHeaderSize(int count)
        {
            var size = FileHeader.Size + Entry.Size * count;
            size += 0x100 - (size % 0x100);
            return size;
        }

        public int EstimateHeaderSize()
        {
            return EstimateHeaderSize(this._Entries.Count);
        }

        public void Serialize(Stream output)
        {
            var endian = this.Endian;

            var headerSize = this.EstimateHeaderSize();
            var headerBytes = new byte[headerSize];

            SimpleBufferWriter<byte> headerWriter = new(headerBytes, 0, headerSize);
            headerWriter.Advance(FileHeader.Size);

            int entriesSize = Entry.Size * this._Entries.Count;
            foreach (var entry in this._Entries)
            {
                entry.Write(headerWriter, endian);
            }

            FileHeader fileHeader;
            fileHeader.Endian = endian;
            fileHeader.TotalSize = this.TotalSize;
            fileHeader.EntryCount = this._Entries.Count;

            headerWriter.Reset();
            fileHeader.Write(headerWriter);

            output.Write(headerBytes, 0, headerSize);
        }

        public void Deserialize(Stream input)
        {
            var header = input.ReadToInstance(FileHeader.Size, FileHeader.Read);
            var endian = header.Endian;
            var entries = input.ReadToInstances((int)header.EntryCount, Entry.Size, endian, Entry.Read);

            this.Endian = endian;
            this.TotalSize = header.TotalSize;
            this.Entries.Clear();
            this.Entries.AddRange(entries);
        }
    }
}

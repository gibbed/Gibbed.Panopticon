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
using System.Collections.Generic;
using System.Text;
using Gibbed.Buffers;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;
using Gibbed.Panopticon.FileFormats.LanguageMessages;

namespace Gibbed.Panopticon.FileFormats
{
    public class LanguageMessageFile
    {
        internal static readonly Encoding ValueEncoding = Encoding.UTF8;

        private readonly List<Message> _Messages;

        public LanguageMessageFile()
        {
            this._Messages = new();
        }

        public Endian Endian { get; set; }

        public List<Message> Messages => this._Messages;

        public static int EstimateHeaderSize(int count)
        {
            return FileHeader.Size + MessageHeader.Size * count;
        }

        public int EstimateHeaderSize()
        {
            return EstimateHeaderSize(this._Messages.Count);
        }

        public static LanguageMessageFile Read(ReadOnlySpan<byte> span)
        {
            int index = 0;

            var header = FileHeader.Read(span, ref index);
            var endian = header.Endian;

            index = header.MessageTableOffset;
            var entryHeaders = new MessageHeader[header.MessageCount];
            for (uint i = 0; i < header.MessageCount; i++)
            {
                entryHeaders[i] = MessageHeader.Read(span, ref index, endian);
            }

            var entries = new Message[header.MessageCount];
            for (uint i = 0; i < header.MessageCount; i++)
            {
                var entryHeader = entryHeaders[i];

                if (entryHeader.Id > entryHeader.Id2)
                {
                    throw new FormatException();
                }

                index = entryHeader.ValueOffset;

                Message entry;
                entry.Id = entryHeader.Id;
                entry.Id2 = entryHeader.Id2;
                entry.Key = entryHeader.Key;
                entry.Value = span.ReadStringZ(ref index, ValueEncoding);
                entries[i] = entry;
            }

            LanguageMessageFile instance = new();
            instance.Endian = endian;
            instance.Messages.AddRange(entries);
            return instance;
        }

        public static void Write(LanguageMessageFile instance, IBufferWriter<byte> writer)
        {
            var endian = instance.Endian;

            var headerSize = instance.EstimateHeaderSize();

            PooledArrayBufferWriter<byte> dataWriter = new();

            List<MessageHeader> messageHeaders = new(instance.Messages.Count);
            foreach (var message in instance.Messages)
            {
                MessageHeader messageHeader;
                messageHeader.Id = message.Id;
                messageHeader.Id2 = message.Id2;
                messageHeader.Key = message.Key;
                messageHeader.ValueOffset = headerSize + dataWriter.WrittenCount;
                messageHeaders.Add(messageHeader);

                dataWriter.WriteStringZ(message.Value, ValueEncoding);
            }

            var headerBytes = new byte[headerSize];

            SimpleBufferWriter<byte> headerWriter = new(headerBytes, 0, headerSize);
            headerWriter.Advance(FileHeader.Size);

            foreach (var messageHeader in messageHeaders)
            {
                messageHeader.Write(headerWriter, endian);
            }

            FileHeader fileHeader;
            fileHeader.Endian = endian;
            fileHeader.MessageCount = messageHeaders.Count;
            fileHeader.MessageTableOffset = 16;

            headerWriter.Reset();
            fileHeader.Write(headerWriter);

            writer.Write(headerBytes);
            writer.Write(dataWriter.WrittenSpan);
        }

        public void Write(IBufferWriter<byte> writer)
        {
            Write(this, writer);
        }
    }
}

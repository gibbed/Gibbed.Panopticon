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
using System.Collections.Generic;
using System.Text;
using Gibbed.Buffers;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;

namespace Gibbed.Panopticon.FileFormats
{
    internal abstract class Labeler<TStringPool> : ILabeler<TStringPool>
        where TStringPool : Enum
    {
        private readonly List<PointerLabel> _PointerLabels;

        public Labeler()
        {
            _PointerLabels = new();
        }

        protected abstract StringLabelPool GetStringPool(TStringPool pool);
        protected abstract IEnumerable<StringLabelPool> GetStringPools();

        public Encoding StringEncoding { get; set; } = Encoding.UTF8;

        public ILabel WritePointer(IArrayBufferWriter<byte> writer)
        {
            var label = new PointerLabel();
            label.Offsets.Add(writer.WrittenCount);
            _PointerLabels.Add(label);
            writer.WriteValueS32(-1, Endian.Little);
            return label;
        }

        public abstract void WriteStringRef(IArrayBufferWriter<byte> writer, string value);

        public void WriteStringRef(IArrayBufferWriter<byte> writer, string value, TStringPool pool)
        {
            if (value == null)
            {
                writer.WriteValueS32(0, Endian.Little);
                return;
            }
            var stringLabelPool = GetStringPool(pool);
            if (stringLabelPool.Lookup.TryGetValue(value, out var label) == false)
            {
                label = new(value);
                stringLabelPool.Labels.Add(label);
                stringLabelPool.Lookup.Add(value, label);
            }
            label.Offsets.Add(writer.WrittenCount);
            writer.WriteValueS32(-1, Endian.Little);
        }

        public void Fixup(byte[] bytes, out byte[] stringBytes, Endian endian)
        {
            var encoding = StringEncoding;
            SimpleBufferWriter<byte> writer = new(bytes);
            var baseStringOffset = writer.WrittenCount = bytes.Length;
            PooledArrayBufferWriter<byte> stringWriter = new();
            foreach (var stringLabelPool in this.GetStringPools())
            {
                Fixup(stringLabelPool, baseStringOffset, writer, stringWriter, encoding, endian);
            }
            stringBytes = stringWriter.WrittenSpan.ToArray();
            stringWriter.Clear();
            foreach (var pointerLabel in _PointerLabels)
            {
                var value = pointerLabel.Value ?? throw new InvalidOperationException("label has no value set");
                foreach (var offset in pointerLabel.Offsets)
                {
                    writer.Seek(offset);
                    writer.WriteValueS32(value, endian);
                }
            }
        }

        private static void Fixup(
            StringLabelPool stringLabelPool,
            int baseStringOffset,
            SimpleBufferWriter<byte> writer,
            IArrayBufferWriter<byte> stringWriter,
            Encoding encoding,
            Endian endian)
        {
            foreach (var stringLabel in stringLabelPool.Labels)
            {
                var stringOffset = baseStringOffset + stringWriter.WrittenCount;
                foreach (var offset in stringLabel.Offsets)
                {
                    writer.Seek(offset);
                    writer.WriteValueS32(stringOffset, endian);
                }
                stringWriter.WriteStringZ(stringLabel.Value, encoding);
            }
        }

        protected class BaseLabel
        {
            private readonly List<int> _Offsets;

            public BaseLabel()
            {
                _Offsets = new();
            }

            public List<int> Offsets => _Offsets;
        }

        protected class PointerLabel : BaseLabel, ILabel
        {
            private int? _Value;

            public PointerLabel()
            {
            }

            public int? Value => _Value;

            public void Set(int value)
            {
                _Value = value;
            }
        }

        protected class StringLabel : BaseLabel
        {
            private readonly string _Value;

            public StringLabel(string value)
            {
                _Value = value;
            }

            public string Value => _Value;
        }

        protected class StringLabelPool
        {
            private readonly List<StringLabel> _Labels;
            private readonly Dictionary<string, StringLabel> _Lookup;

            public StringLabelPool()
            {
                _Labels = new();
                _Lookup = new();
            }

            public List<StringLabel> Labels => _Labels;
            public Dictionary<string, StringLabel> Lookup => _Lookup;
        }
    }
}

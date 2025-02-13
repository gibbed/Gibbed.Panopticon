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

namespace Gibbed.Panopticon.Common.Math
{
    public struct AABB
    {
        internal const int Size = Vector4.Size * 3;

        public Vector4 Center;
        public Vector4 Minimum;
        public Vector4 Maximum;

        public static AABB Read(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            AABB instance;
            instance.Center = Vector4.Read(span, ref index, endian);
            instance.Minimum = Vector4.Read(span, ref index, endian);
            instance.Maximum = Vector4.Read(span, ref index, endian);
            return instance;
        }

        internal static void Write(AABB instance, IArrayBufferWriter<byte> writer, Endian endian)
        {
            instance.Center.Write(writer, endian);
            instance.Minimum.Write(writer, endian);
            instance.Maximum.Write(writer, endian);
        }

        internal readonly void Write(IArrayBufferWriter<byte> writer, Endian endian)
        {
            Write(this, writer, endian);
        }

        public override string ToString()
        {
            return $"({this.Center},{this.Minimum},{this.Maximum})";
        }
    }
}

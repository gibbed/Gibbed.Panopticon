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

namespace Gibbed.Panopticon.FileFormats.Models
{
    public struct FileVersion : IEquatable<FileVersion>
    {
        public static readonly FileVersion VersionLegacy = new(0x9300, 0);
        public static readonly FileVersion Version10 = new(1, 0);
        public static readonly FileVersion Version11 = new(1, 1);

        internal const int Size = 4;

        public ushort Major;
        public ushort Minor;

        public FileVersion(ushort major, ushort minor)
        {
            this.Major = major;
            this.Minor = minor;
        }

        public readonly bool IsNew => this.Major > 1 || this == Version11;

        internal static FileVersion Read(ReadOnlySpan<byte> span, ref int index, Endian endian)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            FileVersion instance;
            instance.Major = span.ReadValueU16(ref index, endian);
            instance.Minor = span.ReadValueU16(ref index, endian);
            return instance;
        }

        internal static void Write(FileVersion instance, IArrayBufferWriter<byte> writer, Endian endian)
        {
            writer.WriteValueU16(instance.Major, endian);
            writer.WriteValueU16(instance.Minor, endian);
        }

        internal readonly void Write(IArrayBufferWriter<byte> writer, Endian endian)
        {
            Write(this, writer, endian);
        }

        public override string ToString()
        {
            return this.Major == 0x9300
                ? $"0x{this.Major:X}.{this.Minor}"
                : $"{this.Major}.{this.Minor}";
        }

        public override readonly bool Equals(object obj)
        {
            return obj is FileVersion version && this.Equals(version) == true;
        }

        public readonly bool Equals(FileVersion other)
        {
            return this.Major == other.Major && this.Minor == other.Minor;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(this.Major, this.Minor);
        }

        public static bool operator ==(FileVersion left, FileVersion right)
        {
            return left.Equals(right) == true;
        }

        public static bool operator !=(FileVersion left, FileVersion right)
        {
            return left.Equals(right) == false;
        }
    }
}

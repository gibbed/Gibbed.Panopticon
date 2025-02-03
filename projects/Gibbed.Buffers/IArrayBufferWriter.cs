using System;
using System.Buffers;

namespace Gibbed.Buffers
{
    public interface IArrayBufferWriter<T> : IBufferWriter<T>
    {
        ReadOnlyMemory<T> WrittenMemory { get; }
        ReadOnlySpan<T> WrittenSpan { get; }
        int WrittenCount { get; }
        int Capacity { get; }
        int FreeCapacity { get; }

        void Clear();
    }
}

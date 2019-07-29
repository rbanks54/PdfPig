namespace UglyToad.PdfPig.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal class ByteArrayInputBytes : IInputBytes
    {
        private readonly ReadOnlyMemory<byte> bytes;

        [DebuggerStepThrough]
        public ByteArrayInputBytes(in IReadOnlyList<byte> bytes)
        {
            this.bytes = bytes.ToArray().AsMemory();
            currentOffset = -1;
        }
        
        private long currentOffset;
        public long CurrentOffset => currentOffset + 1;

        public bool MoveNext()
        {
            if (currentOffset == bytes.Length - 1)
            {
                return false;
            }

            currentOffset++;
            return true;
        }

        public byte CurrentByte { get {
            if (currentOffset < 0) return 0;
            return bytes.Span[(int)currentOffset];
        } }

        public long Length => bytes.Length;

        public byte? Peek()
        {
            if (currentOffset == bytes.Length - 1)
            {
                return null;
            }

            return bytes.Span[(int)currentOffset + 1];
        }

        public bool IsAtEnd()
        {
            return currentOffset == bytes.Length - 1;
        }

        public void Seek(long position)
        {
            if (position > bytes.Length) position = bytes.Length;
            currentOffset = (int)position - 1;
        }

        public void Dispose()
        {
        }

        public ReadOnlySpan<byte> GetSpan(int startOffset, int length) {
            return bytes.Span.Slice(startOffset - 1, length); 
        }
    }
}
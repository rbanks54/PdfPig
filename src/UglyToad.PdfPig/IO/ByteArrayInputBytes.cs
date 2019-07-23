namespace UglyToad.PdfPig.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal class ByteArrayInputBytes : IInputBytes
    {
        private readonly IReadOnlyList<byte> bytes;

        private readonly ReadOnlyMemory<byte> byteMemory;

        [DebuggerStepThrough]
        public ByteArrayInputBytes(in IReadOnlyList<byte> bytes)
        {
            this.bytes = bytes;
            byteMemory = this.bytes.ToArray().AsMemory();
            currentOffset = -1;
        }
        
        private long currentOffset;
        public long CurrentOffset => currentOffset + 1;

        public bool MoveNext()
        {
            if (currentOffset == byteMemory.Length - 1)
            {
                return false;
            }

            currentOffset++;
            return true;
        }

        public byte CurrentByte { 
            get {
                if (currentOffset < 0) return 0;
                return byteMemory.Span[(int)currentOffset];
            }  
        }
        public ReadOnlySpan<byte> GetSpan(in int startOffset, in int length)
        {
            return byteMemory.Slice(startOffset - 1, length).Span;
        }
        
        public long Length => byteMemory.Length;

        public byte? Peek()
        {
            if (currentOffset == byteMemory.Length - 1)
            {
                return null;
            }

            return byteMemory.Span[(int)currentOffset + 1];
        }

        public bool IsAtEnd()
        {
            return currentOffset == byteMemory.Length - 1;
        }

        public void Seek(long position)
        {
            currentOffset = (int)position - 1;
        }

        public void Dispose()
        {
        }
    }
}
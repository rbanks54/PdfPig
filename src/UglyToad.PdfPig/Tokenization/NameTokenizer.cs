namespace UglyToad.PdfPig.Tokenization
{
    using System;
    using System.Buffers;
    using System.Buffers.Text;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using IO;
    using Parser.Parts;
    using Tokens;

    internal class NameTokenizer : ITokenizer
    {
        public bool ReadsNextByte { get; } = true;

        public bool TryTokenize_Original(byte currentByte, IInputBytes inputBytes, out IToken token)
        {
            token = null;

            if (currentByte != '/')
            {
                return false;
            }

            var bytes = new List<byte>();

            bool escapeActive = false;
            int postEscapeRead = 0;
            var escapedChars = new char[2];

            while (inputBytes.MoveNext())
            {
                var b = inputBytes.CurrentByte;

                if (b == '#')
                {
                    escapeActive = true;
                }
                else if (escapeActive)
                {
                    if (ReadHelper.IsHex((char)b))
                    {
                        escapedChars[postEscapeRead] = (char)b;
                        postEscapeRead++;

                        if (postEscapeRead == 2)
                        {
                            var hex = new string(escapedChars);

                            var characterToWrite = (byte)Convert.ToInt32(hex, 16);
                            bytes.Add(characterToWrite);

                            escapeActive = false;
                            postEscapeRead = 0;
                        }
                    }
                    else
                    {
                        bytes.Add((byte)'#');

                        if (postEscapeRead == 1)
                        {
                            bytes.Add((byte)escapedChars[0]);
                        }

                        if (ReadHelper.IsEndOfName(b))
                        {
                            break;
                        }

                        if (b == '#')
                        {
                            // Make it clear what's going on, we read something like #m#AE
                            // ReSharper disable once RedundantAssignment
                            escapeActive = true;
                            postEscapeRead = 0;
                            continue;
                        }

                        bytes.Add(b);
                        escapeActive = false;
                        postEscapeRead = 0;
                    }

                }
                else if (ReadHelper.IsEndOfName(b))
                {
                    break;
                }
                else
                {
                    bytes.Add(b);
                }
            }

            byte[] byteArray = bytes.ToArray();

            var str = ReadHelper.IsValidUtf8(byteArray)
                ? Encoding.UTF8.GetString(byteArray)
                : Encoding.GetEncoding("windows-1252").GetString(byteArray);

            token = NameToken.Create(str);

            return true;
        }

        public bool TryTokenize_v2(byte currentByte, IInputBytes inputBytes, out IToken token)
        {
            //Trying to get fancy with spans, and array pools.  We actually made it slower.
            //Also this implementation breaks some unit tests - I'd investigate, but it's a bad approach so we should delete it
            token = null;

            if (currentByte != '/')
            {
                return false;
            }

            var pool = ArrayPool<byte>.Shared;
            var byteArray = pool.Rent(200);
            var bytes = byteArray.AsSpan();
            var writtenBytes = 0;

            bool escapeActive = false;
            var chunkStartIndex = inputBytes.CurrentOffset + 1;
            ReadOnlySpan<byte> chunk;

            try
            {
                while (inputBytes.MoveNext())
                {
                    var b = inputBytes.CurrentByte;

                    if (b == '#')
                    {
                        escapeActive = true;
                        continue;
                    }
                    else if (escapeActive)
                    {
                        if (ReadHelper.IsHex((char)b))
                        {
                            // Looking for hex encoded as "#FF" format
                            var nextByte = inputBytes.Peek();
                            if (ReadHelper.IsHex((char)nextByte)) {
                                inputBytes.MoveNext(); 
                                //The chunk of the token before the hex marker ends 3 chars earlier
                                chunk = inputBytes.GetSpan((int)chunkStartIndex,(int)(inputBytes.CurrentOffset - 2 - chunkStartIndex));
                                chunk.CopyTo(bytes);
                                writtenBytes += chunk.Length;
                                chunkStartIndex = (int)inputBytes.CurrentOffset + 1;
                                bytes = bytes.Slice(chunk.Length);

                                var hexSpan = inputBytes.GetSpan((int)inputBytes.CurrentOffset - 1,2); //ignore leading hash marker
                                var hexString = Encoding.ASCII.GetString(hexSpan); //yucky - but I can't find a span API that eliminates the need to convert to a string for the int conversion
                                bytes[0] = (byte)Convert.ToInt32(hexString, 16);
                                bytes = bytes.Slice(1);
                                writtenBytes++;
                            }
                        }
                        escapeActive = false;
                    }
                    if (ReadHelper.IsEndOfName(b))
                    {
                        inputBytes.Seek(inputBytes.CurrentOffset - 1); //found an end of name token - we don't want to process it
                        break;
                    }
                }
                //Write the final chunk
                chunk = inputBytes.GetSpan((int)chunkStartIndex,(int)(inputBytes.CurrentOffset - chunkStartIndex + 1));
                chunk.CopyTo(bytes);
                writtenBytes += chunk.Length;
                chunkStartIndex = (int)inputBytes.CurrentOffset;
                bytes = bytes.Slice(chunk.Length);

                var tokenSpan = byteArray.AsSpan().Slice(0,writtenBytes);
                // var tokenArray = tokenSpan.ToArray(); //need to convert the helper methods later
                
                var str = ReadHelper.IsValidUtf8Span(byteArray.AsSpan().Slice(0,writtenBytes))
                    ? Encoding.UTF8.GetString(tokenSpan)
                    : Encoding.GetEncoding("windows-1252").GetString(tokenSpan);

                token = NameToken.Create(str);
            }
            finally
            {
                pool.Return(byteArray);
            }

            return true;
        }


        public bool TryTokenize_v3(byte currentByte, IInputBytes inputBytes, out IToken token)
        {
            token = null;

            if (currentByte != '/')
            {
                return false;
            }

            var bytes = new List<byte>();

            bool escapeActive = false;
            int postEscapeRead = 0;
            var escapedChars = new char[2];

            while (inputBytes.MoveNext())
            {
                var b = inputBytes.CurrentByte;

                if (b == '#')
                {
                    escapeActive = true;
                }
                else if (escapeActive)
                {
                    if (ReadHelper.IsHex((char)b))
                    {
                        escapedChars[postEscapeRead] = (char)b;
                        postEscapeRead++;

                        if (postEscapeRead == 2)
                        {
                            var hex = new string(escapedChars);

                            var characterToWrite = (byte)Convert.ToInt32(hex, 16);
                            bytes.Add(characterToWrite);

                            escapeActive = false;
                            postEscapeRead = 0;
                        }
                    }
                    else
                    {
                        bytes.Add((byte)'#');

                        if (postEscapeRead == 1)
                        {
                            bytes.Add((byte)escapedChars[0]);
                        }

                        if (ReadHelper.IsEndOfName(b))
                        {
                            break;
                        }

                        if (b == '#')
                        {
                            // Make it clear what's going on, we read something like #m#AE
                            // ReSharper disable once RedundantAssignment
                            escapeActive = true;
                            postEscapeRead = 0;
                            continue;
                        }

                        bytes.Add(b);
                        escapeActive = false;
                        postEscapeRead = 0;
                    }

                }
                else if (ReadHelper.IsEndOfName(b))
                {
                    break;
                }
                else
                {
                    bytes.Add(b);
                }
            }

            // byte[] byteArray = bytes.ToArray();
            var tokenSpan = bytes.ToArray().AsSpan();

            var str = ReadHelper.IsValidUtf8Span(tokenSpan)
                ? Encoding.UTF8.GetString(tokenSpan)
                : Encoding.GetEncoding("windows-1252").GetString(tokenSpan);

            token = NameToken.Create(str);

            return true;
        }

        public bool TryTokenize_v4(byte currentByte, IInputBytes inputBytes, out IToken token)
        {
            token = null;

            if (currentByte != '/')
            {
                return false;
            }

            var bytes = new List<byte>();

            bool escapeActive = false;
            int postEscapeRead = 0;
            var escapedChars = new char[2];

            while (inputBytes.MoveNext())
            {
                var b = inputBytes.CurrentByte;

                if (b == '#')
                {
                    escapeActive = true;
                }
                else if (escapeActive)
                {
                    if (ReadHelper.IsHex((char)b))
                    {
                        escapedChars[postEscapeRead] = (char)b;
                        postEscapeRead++;

                        if (postEscapeRead == 2)
                        {
                            var hex = new string(escapedChars);

                            var characterToWrite = (byte)Convert.ToInt32(hex, 16);
                            bytes.Add(characterToWrite);

                            escapeActive = false;
                            postEscapeRead = 0;
                        }
                    }
                    else
                    {
                        bytes.Add((byte)'#');

                        if (postEscapeRead == 1)
                        {
                            bytes.Add((byte)escapedChars[0]);
                        }

                        if (ReadHelper.IsEndOfName(b))
                        {
                            break;
                        }

                        if (b == '#')
                        {
                            // Make it clear what's going on, we read something like #m#AE
                            // ReSharper disable once RedundantAssignment
                            escapeActive = true;
                            postEscapeRead = 0;
                            continue;
                        }

                        bytes.Add(b);
                        escapeActive = false;
                        postEscapeRead = 0;
                    }

                }
                else if (ReadHelper.IsEndOfName(b))
                {
                    break;
                }
                else
                {
                    bytes.Add(b);
                }
            }

            // byte[] byteArray = bytes.ToArray();
            var tokenSpan = bytes.ToArray().AsSpan();

            var str = ReadHelper.IsValidUtf8Span_sharedDecoder(tokenSpan)
                ? Encoding.UTF8.GetString(tokenSpan)
                : Encoding.GetEncoding("windows-1252").GetString(tokenSpan);

            token = NameToken.Create(str);

            return true;
        }

        public bool TryTokenize(byte currentByte, IInputBytes inputBytes, out IToken token)
        {
            token = null;

            if (currentByte != '/')
            {
                return false;
            }

            //We'll try preallocating an array and filling it
            //This way we can avoid the List expansion operations 
            //and the ToArray call at the end

            //It involves two passes of the inputByte array though
            //Once to work out length, and once to fill
            //This could be slower

            var startPosition = inputBytes.CurrentOffset;
            bool escapeActive = false;
            int postEscapeRead = 0;
            var escapedChars = new char[2];

            while (inputBytes.MoveNext())
            {
                var b = inputBytes.CurrentByte;
                if (b == '#')
                {
                    escapeActive = true;
                }
                else if (escapeActive)
                {
                    if (ReadHelper.IsHex((char)b))
                    {
                        escapedChars[postEscapeRead] = (char)b;
                        postEscapeRead++;

                        if (postEscapeRead == 2)
                        {
                            var hex = new string(escapedChars);
                            var characterToWrite = (byte)Convert.ToInt32(hex, 16);
                            escapeActive = false;
                            postEscapeRead = 0;
                        }
                    }
                    else
                    {
                        if (ReadHelper.IsEndOfName_NoHashSet(b))
                        {
                            break;
                        }

                        if (b == '#')
                        {
                            // Make it clear what's going on, we read something like #m#AE
                            // ReSharper disable once RedundantAssignment
                            escapeActive = true;
                            postEscapeRead = 0;
                            continue;
                        }
                        escapeActive = false;
                        postEscapeRead = 0;
                    }
                }
                else if (ReadHelper.IsEndOfName_NoHashSet(b))
                {
                    break;
                }
            }

            var endPosition = inputBytes.CurrentOffset;

            inputBytes.Seek(startPosition);

            var bytes = new byte[endPosition - startPosition + 1];

            escapeActive = false;
            postEscapeRead = 0;
            var index = 0;

            while (inputBytes.MoveNext())
            {
                var b = inputBytes.CurrentByte;

                if (b == '#')
                {
                    escapeActive = true;
                }
                else if (escapeActive)
                {
                    if (ReadHelper.IsHex((char)b))
                    {
                        escapedChars[postEscapeRead] = (char)b;
                        postEscapeRead++;

                        if (postEscapeRead == 2)
                        {
                            var hex = new string(escapedChars);

                            var characterToWrite = (byte)Convert.ToInt32(hex, 16);
                            bytes[index++] = characterToWrite;

                            escapeActive = false;
                            postEscapeRead = 0;
                        }
                    }
                    else
                    {
                        bytes[index++] = (byte)'#';

                        if (postEscapeRead == 1)
                        {
                            bytes[index++] = (byte)escapedChars[0];
                        }

                        if (ReadHelper.IsEndOfName_NoHashSet(b))
                        {
                            break;
                        }

                        if (b == '#')
                        {
                            // Make it clear what's going on, we read something like #m#AE
                            // ReSharper disable once RedundantAssignment
                            escapeActive = true;
                            postEscapeRead = 0;
                            continue;
                        }

                        bytes[index++] = b;
                        escapeActive = false;
                        postEscapeRead = 0;
                    }

                }
                else if (ReadHelper.IsEndOfName_NoHashSet(b))
                {
                    break;
                }
                else
                {
                    bytes[index++] = b;
                }
            }

            // byte[] byteArray = bytes.ToArray();
            var tokenSpan = bytes.AsSpan().Slice(0,index);

            var str = ReadHelper.IsValidUtf8Span_sharedDecoder(tokenSpan)
                ? Encoding.UTF8.GetString(tokenSpan)
                : Encoding.GetEncoding("windows-1252").GetString(tokenSpan);

            token = NameToken.Create(str);

            return true;
        }
    }
}
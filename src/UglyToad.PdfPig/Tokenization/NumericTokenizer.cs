namespace UglyToad.PdfPig.Tokenization
{
    using System;
    using System.Buffers.Text;
    using System.Globalization;
    using System.Text;
    using IO;
    using Tokens;

    internal class NumericTokenizer : ITokenizer
    {
        public bool ReadsNextByte { get; } = true;

        public bool TryTokenize(byte currentByte, IInputBytes inputBytes, out IToken token)
        {
            token = null;

            var c = (char)currentByte;

            if (!char.IsDigit(c) && c != '-' && c != '+' && c != '.')
            {
                return false;
            }

            var startIndex = inputBytes.CurrentOffset;
            var endIndex = startIndex;

            while (inputBytes.MoveNext())
            {
                c = (char)inputBytes.CurrentByte;

                if (!char.IsDigit(c) &&
                    c != '-' &&
                    c != '+' &&
                    c != '.' &&
                    c != 'E' &&
                    c != 'e')
                {
                    break;
                }
                endIndex = inputBytes.CurrentOffset;
            }

            decimal value;

            var tokenSpan = inputBytes.GetSpan((int)startIndex, (int)(endIndex - startIndex) + 1);
            try
            {
                if (tokenSpan.Length == 1 && (tokenSpan[0] == '-' || tokenSpan[0] == '.'))
                {
                    value = 0;
                }
                else
                {
                    var parsed = Utf8Parser.TryParse(tokenSpan, out value, out int bytesUsed);
                    if (!parsed || bytesUsed != tokenSpan.Length) {
                        throw new FormatException("Parsing the token went kaput");
                    }
                }
            }
            catch (FormatException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }

            token = new NumericToken(value);

            return true;
        }
    }
}

namespace UglyToad.PdfPig.Tests.Fonts.TrueType.Embedding
{
    using System;
    using System.IO;
    using PdfPig.Fonts.TrueType;
    using PdfPig.Fonts.TrueType.Embedding;
    using PdfPig.Fonts.TrueType.Parser;
    using PdfPig.IO;
    using Xunit;

    public class TrueTypeSubsetterTests
    {
        private readonly TrueTypeFontParser parser = new TrueTypeFontParser();

        [Fact]
        public void CanCreateValidRobotoSubset()
        {
            var file = GetFontFile("Roboto-Regular.ttf");

            var font = parser.Parse(new TrueTypeDataBytes(new ByteArrayInputBytes(file)));
              
            TrueTypeSubsetter.Subset(font, new int[]{ 'H', 'e', 'l', 'o', 'w', 'r', 'd' });
        }

        private static byte[] GetFontFile(string name)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "TrueType");

            return File.ReadAllBytes(Path.Combine(path, name));
        }
    }
}

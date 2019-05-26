namespace UglyToad.PdfPig.Fonts.TrueType.Embedding
{
    using System.Collections.Generic;
    using System.IO;
    using Tables;

    internal class TrueTypeSubsetter
    {
        private static readonly byte[] VersionHeader =
        {
            0, 1, 0, 0
        };

        public static void Subset(TrueTypeFontProgram font, IReadOnlyList<int> codePoints)
        {
            // Find the corresponding glyph,
            // Keep the normal header
            // Write the glyphs
            // Write the index to location table

            // Required
            var glyf = font.TableRegister.GlyphTable;
            var head = font.TableRegister.HeaderTable;
            var hhea = font.TableRegister.HorizontalHeaderTable;
            var hmtx = font.TableRegister.HorizontalMetricsTable;
            var loca = font.TableRegister.IndexToLocationTable;
            var maxp = font.TableRegister.MaximumProfileTable;


            using (var stream = new MemoryStream())
            {
                // TODO: write header.
                // stream.Write(VersionHeader, 0, VersionHeader.Length);


            }
        }

        private static void WriteHeadTable(HeaderTable header)
        {

        }
    }
}

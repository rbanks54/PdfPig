namespace UglyToad.PdfPig.Content
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Graphics;
    using Graphics.Operations;
    using Tokenization.Scanner;
    using XObjects;

    /// <summary>
    /// Data extracted from the page content stream as well as the parsed underlying content stream.
    /// </summary>
    /// <remarks>
    /// This should contain a replayable stack of drawing instructions for page content
    /// from a content stream in addition to lazily evaluated state such as text on the page or images.
    /// </remarks>
    internal class PageContent
    {
        private readonly IReadOnlyList<int> newlineIndices;
        private readonly IReadOnlyDictionary<XObjectType, List<XObjectContentRecord>> xObjects;
        private readonly IPdfTokenScanner pdfScanner;
        private readonly XObjectFactory xObjectFactory;
        private readonly TextOptions textOptions;
        private readonly bool isLenientParsing;

        internal IReadOnlyList<IGraphicsStateOperation> GraphicsStateOperations { get; }

        public IReadOnlyList<Letter> Letters { get; }

        internal PageContent(IReadOnlyList<IGraphicsStateOperation> graphicsStateOperations, IReadOnlyList<Letter> letters,
            IReadOnlyList<int> newlineIndices,
            IReadOnlyDictionary<XObjectType, List<XObjectContentRecord>> xObjects,
            IPdfTokenScanner pdfScanner,
            XObjectFactory xObjectFactory,
            TextOptions textOptions,
            bool isLenientParsing)
        {
            GraphicsStateOperations = graphicsStateOperations;
            Letters = letters;
            this.newlineIndices = newlineIndices;
            this.xObjects = xObjects;
            this.pdfScanner = pdfScanner;
            this.xObjectFactory = xObjectFactory;
            this.textOptions = textOptions;
            this.isLenientParsing = isLenientParsing;
        }

        public IEnumerable<XObjectImage> GetImages()
        {
            foreach (var contentRecord in xObjects[XObjectType.Image])
            {
                yield return xObjectFactory.CreateImage(contentRecord, pdfScanner, isLenientParsing);
            }
        }

        public string GetText()
        {
            if (Letters == null || Letters.Count == 0)
            {
                return string.Empty;
            }

            if (newlineIndices == null || newlineIndices.Count == 0 || textOptions?.Newline == null)
            {
                // Skip stringbuilder allocation if we don't need to insert newlines.
                return string.Join(string.Empty, Letters.Select(x => x.Value));
            }

            var builder = new StringBuilder();

            for (var i = 0; i < Letters.Count; i++)
            {
                if (newlineIndices.Contains(i))
                {
                    builder.Append(textOptions.Newline);
                }

                builder.Append(Letters[i].Value);
            }

            return builder.ToString();
        }
    }
}

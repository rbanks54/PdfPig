namespace UglyToad.PdfPig
{
    using Content;

    /// <summary>
    /// Configures options used for extracting text from the document content.
    /// </summary>
    public class TextOptions
    {
        private const string DefaultNewline = "\r\n";

        private string newline = DefaultNewline;

        /// <summary>
        /// Should the <see cref="Page.Text"/> include newlines when the content stream moves lines?
        /// </summary>
        public bool IncludeNewlines { get; set; } = true;

        /// <summary>
        /// The newline character(s) to use if <see cref="IncludeNewlines"/> is <see langword="true"/>.
        /// </summary>
        public string Newline
        {
            get => newline ?? DefaultNewline;
            set => newline = value;
        }
    }
}
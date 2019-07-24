namespace Benchmarks {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using BenchmarkDotNet.Attributes;
    using UglyToad.PdfPig.IO;
    using UglyToad.PdfPig.Tokenization;
    public class NameTokens {
        private readonly NameTokenizer tokenizer = new NameTokenizer();
        public class ConvertedStrings 
        { 
            public ConvertedStrings(string[] initialValues) => 
                Values = initialValues 
                        .Select(v => StringConverter.Convert(v)) 
                        .ToArray(); 

            public StringConverter.Result[] Values { get; } 
        } 

        public string[] NameStrings => new[] 
        { 
            "/A−Name_With;Various***Characters?",
        }; 

        public IEnumerable<StringConverter.Result> TestNameStrings() 
        { 
            return new ConvertedStrings(NameStrings).Values.AsEnumerable(); 
        } 

        [Benchmark] 
        [ArgumentsSource(nameof(TestNameStrings))] 
        public void ValidNames(StringConverter.Result input) 
        { 
            tokenizer.TryTokenize(input.First, input.Bytes, out var token); 
        }
    }
}

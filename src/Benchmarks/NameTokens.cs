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

        //30% slower. Yikes!!
        // [Benchmark] 
        // [ArgumentsSource(nameof(TestNameStrings))] 
        // public void ValidNamesV2(StringConverter.Result input) 
        // { 
        //     tokenizer.TryTokenize_v2(input.First, input.Bytes, out var token); 
        // }

        //Faster, but v4 is even faster
        // [Benchmark] 
        // [ArgumentsSource(nameof(TestNameStrings))] 
        // public void ValidNames_v3(StringConverter.Result input) 
        // { 
        //     tokenizer.TryTokenize_v3(input.First, input.Bytes, out var token); 
        // }

        [Benchmark] 
        [ArgumentsSource(nameof(TestNameStrings))] 
        public void ValidNames_v4(StringConverter.Result input) 
        { 
            tokenizer.TryTokenize_v4(input.First, input.Bytes, out var token); 
        }

        [Benchmark] 
        [ArgumentsSource(nameof(TestNameStrings))] 
        public void ValidNames_Latest(StringConverter.Result input) 
        { 
            tokenizer.TryTokenize(input.First, input.Bytes, out var token); 
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(TestNameStrings))] 
        public void Old_ValidNames(StringConverter.Result input) 
        { 
            tokenizer.TryTokenize_Original(input.First, input.Bytes, out var token); 
        }
    }
}

namespace Benchmarks {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using BenchmarkDotNet.Attributes;
    using UglyToad.PdfPig.IO;
    using UglyToad.PdfPig.Tokenization;
    public class NumericTokens {
        private readonly NumericTokenizer tokenizer = new NumericTokenizer();
        public string[] SmallStrings => new[] {"a"}; 

        public class ConvertedStrings 
        { 
            public ConvertedStrings(string[] initialValues) => 
                Values = initialValues 
                        .Select(v => StringConverter.Convert(v)) 
                        .ToArray(); 

            public StringConverter.Result[] Values { get; } 
        } 

        public IEnumerable<StringConverter.Result> TestValues() 
        { 
            return new ConvertedStrings(SmallStrings).Values.AsEnumerable(); 
        } 

        [Benchmark] 
        [ArgumentsSource(nameof(TestValues))] 
        public void InvalidNumbers(StringConverter.Result input) 
        { 
            tokenizer.TryTokenize(input.First, input.Bytes, out var token); 
        } 

        public string[] NumericStrings => new[] 
        { 
            "57473.3458382", "1.57e3" 
        }; 

        public IEnumerable<StringConverter.Result> TestNumericStrings() 
        { 
            return new ConvertedStrings(NumericStrings).Values.AsEnumerable(); 
        } 

        [Benchmark] 
        [ArgumentsSource(nameof(TestNumericStrings))] 
        public void ValidNumbers(StringConverter.Result input) 
        { 
            tokenizer.TryTokenize(input.First, input.Bytes, out var token); 
        }
    }
}

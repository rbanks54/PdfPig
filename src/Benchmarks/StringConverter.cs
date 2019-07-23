using System.Text;
using UglyToad.PdfPig.IO;

namespace Benchmarks {
    public static class StringConverter {
        public static Result Convert(string s, bool readFirst = true) 
        { 
            var input = new ByteArrayInputBytes(Encoding.UTF8.GetBytes(s)); 
            byte initialByte = 0; 
            if (readFirst) 
            { 
                input.MoveNext(); 
                initialByte = input.CurrentByte; 
            } 
            return new Result 
            { 
                OriginalString = s, 
                First = initialByte, 
                Bytes = input 
            }; 
        } 
        public class Result 
        { 
            public byte First { get; set; } 
            public IInputBytes Bytes { get; set; } 
            public string OriginalString { get; set; } 
            public override string ToString() => OriginalString; 
        }
       
    }
}

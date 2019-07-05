using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;

namespace PerformanceTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var iliadPath = @"C:\pd\pdfpig\pdfs\homers illiad.pdf";
            var warAndPeacePath = @"C:\pd\pdfpig\pdfs\War and Peace.pdf";


            var tests = new Dictionary<string, string[]>
            {
                { "just the iliad", new string[]{iliadPath} },
                //{ "just war and peace", new string[]{warAndPeacePath} },
                //{ "both", new string[]{iliadPath, warAndPeacePath} },
            };

            foreach (var test in tests)
            {
                new BenchmarkingEngine(test.Value, test.Key).Start();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PerformanceTester
{
    class BenchmarkingEngine
    {
        private readonly string[] files;
        private readonly string testName;

        public BenchmarkingEngine(string[] files, string testName)
        {
            this.files = files;
            this.testName = testName;
        }

        public void Start()
        {
            var timer = Stopwatch.StartNew();

            //for (int i = 0; i < 10; i++)
            //{
                foreach (var file in files)
                {
                    using (PdfDocument document = PdfDocument.Open(file))
                    {
                        int pageCount = document.NumberOfPages;

                        Page page = document.GetPage(1);

                        decimal widthInPoints = page.Width;
                        decimal heightInPoints = page.Height;

                        int wordCount = 0;
                        for (var p = 0; p < document.NumberOfPages; p++)
                        {
                            // This starts at 1 rather than 0.
                            page = document.GetPage(p + 1);

                            wordCount += page.GetWords().Count();
                        }
                    }
                }
            //}

            timer.Stop();

            Console.WriteLine($"{testName}: - Duration time: \t{timer.Elapsed.TotalSeconds}s");
        }
    }
}

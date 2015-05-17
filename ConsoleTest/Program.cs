using System;
using System.Collections.Generic;
using System.IO.Csv;
using System.Linq;
using System.Text;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter input CSV file: ");

            IEnumerable<IEnumerable<string>> data;
            using (var reader = new CsvReader(Console.ReadLine().Trim('"')))
            {
                data = reader.ReadData().ToArray();
                Console.Write("Break to inspect result or press ENTER to continue... ");
                Console.ReadLine();
            }

            using (var ms = new System.IO.MemoryStream())
            using (var writer = new CsvWriter(new System.IO.StreamWriter(ms)))
            {
                writer.WriteData(data);

                Console.WriteLine();
                Console.WriteLine("---< Write Output >---");
                Console.WriteLine(Encoding.Default.GetString(ms.ToArray()));
                Console.WriteLine("---< End of Stream >---");
                Console.WriteLine();
                Console.Write("Press ENTER to exit... ");
                Console.ReadLine();
            }
        }
    }
}

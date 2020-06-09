using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Nethermind.Comparer
{
    class Program
    {
        static void Main(string[] args)
        {
            B();
            // C(@"tx2_1845.txt", @"tx2_1834.txt");
        }

        public static void C(string aPath, string bPath)
        {
            using FileStream a = new FileStream(aPath, FileMode.Open);
            using FileStream b = new FileStream(bPath, FileMode.Open);

            using StreamReader readerA = new StreamReader(a);
            using StreamReader readerB = new StreamReader(b);

            int line = 0;
            while (true)
            {
                bool wasBreak = false;
                
                string lineA = readerA.ReadLine();
                string lineB = readerB.ReadLine();
                if (lineA != lineB || lineA == null)
                {
                    wasBreak = true;
                    Console.WriteLine($"Difference: {line}");
                    Console.WriteLine(lineA);
                    Console.WriteLine(lineB);
                }

                if (wasBreak)
                {
                    break;
                }

                line++;
            }
        }
        
        public static void B()
        {
            using FileStream stream1718 = new FileStream(@"1718indent.json", FileMode.Open);
            using FileStream stream1834 = new FileStream(@"1834indent.json", FileMode.Open);
            using FileStream stream1845 = new FileStream(@"1845indent.json", FileMode.Open);
            
            using StreamReader reader1718 = new StreamReader(stream1718);
            using StreamReader reader1834 = new StreamReader(stream1834);
            using StreamReader reader1845 = new StreamReader(stream1845);

            int line = 0;
            while (true)
            {
                bool wasBreak = false;
                
                string line1718 = reader1718.ReadLine();
                string line1834 = reader1834.ReadLine();
                string line1845 = reader1845.ReadLine();
                if (line1834 != line1845)
                {
                    wasBreak = true;
                    Console.WriteLine($"Difference 1834vs1845: {line}");
                    Console.WriteLine(line1834);
                    Console.WriteLine(line1845);
                }
                
                // if (line1718 != line1834)
                // {
                //     wasBreak = true;
                //     Console.WriteLine($"Difference 1718vs1834: {line}");
                //     Console.WriteLine(line1718);
                //     Console.WriteLine(line1834);
                // }
                //
                // if (line1718 != line1845)
                // {
                //     wasBreak = true;
                //     Console.WriteLine($"Difference 1718vs1845: {line}");
                //     Console.WriteLine(line1718);
                //     Console.WriteLine(line1845);
                //     
                // }

                if (wasBreak)
                {
                    break;
                }

                line++;
            }
        }

        public static void A()
        {
            using FileStream stream1845 = new FileStream(@"C:\Users\tksta\Downloads\consensus_master\trace1845.json", FileMode.Open);
            using FileStream stream1834 = new FileStream(@"C:\Users\tksta\Downloads\consensus_1.8.34\trace1834.json", FileMode.Open);
            using StreamReader reader1845 = new StreamReader(stream1845);
            using StreamReader reader1834 = new StreamReader(stream1834);

            int index1845 = 0;
            int index1834 = 0;
            while (true)
            {
                string line1845 = reader1845.ReadLine();
                string line1834 = reader1834.ReadLine();
                line1834 = line1834.Trim().Trim(',');
                line1845 = line1845.Trim().Trim(',');

                bool isInSortedStorage = false;
                while (true)
                {
                    if (isInSortedStorage)
                    {
                        if (line1845.Contains("},"))
                        {
                            isInSortedStorage = false;
                        }
                        
                        Console.WriteLine($"Skipping 1845 storage | {index1845} | {line1845}");
                        line1845 = reader1845.ReadLine().Trim();
                        index1845++;
                    }

                    if (line1834.Contains("\"error\": null"))
                    {
                        Console.WriteLine($"Skipping 1834 | {line1834}");
                        line1834 = reader1834.ReadLine().Trim();
                        index1834++;
                    }

                    if (line1845.Contains("storagesByDepth"))
                    {
                        Console.WriteLine($"Skipping 1845 | {index1845} | {line1845}");
                        line1845 = reader1845.ReadLine().Trim();
                        index1845++;
                    }
                    else if (line1845.Contains("sortedStorage\": {}"))
                    {
                        Console.WriteLine($"Skipping 1845 | {index1845} | {line1845}");
                        line1845 = reader1845.ReadLine().Trim();
                        index1845++;
                    }
                    else if (line1845 == "\"storage\": {")
                    {
                        Console.WriteLine($"Skipping 1845 | {index1845} | {line1845}");
                        isInSortedStorage = true;
                        line1845 = reader1845.ReadLine().Trim();
                        index1845++;
                    }
                    else if (line1845 == "],")
                    {
                        Console.WriteLine($"Skipping 1845 | {index1845} | {line1845}");
                        line1845 = reader1845.ReadLine().Trim();
                        index1845++;
                    }
                    else if (line1845 == "{}")
                    {
                        Console.WriteLine($"Skipping 1845 | {index1845} | {line1845}");
                        line1845 = reader1845.ReadLine().Trim();
                        index1845++;
                    }
                    else if(!isInSortedStorage)
                    {
                        break;
                    }
                }
                
                line1834 = line1834.Trim().Trim(',');
                line1845 = line1845.Trim().Trim(',');
                line1845 = line1845.Replace("sortedStorage", "storage");

                line1834 = ReplaceHex(line1834);
                line1845 = ReplaceHex(line1845);
                line1845 = ReplaceNegativeGas(line1845);

                line1834 = ReplaceNumberNoString(line1834);
                line1845 = ReplaceNumberNoString(line1845);

                if (line1834 != line1845)
                {
                    if (line1834.Contains("OutOfGas"))
                    {
                        index1834--;
                    }
                    else
                    {
                        Console.WriteLine($"1.8.34 | {index1834} | {line1834}");
                        Console.WriteLine($"1.8.45 | {index1845} | {line1845}");
                        break;    
                    }
                }

                index1834++;
                index1845++;
            }

            Console.ReadLine();
        }

        private static string ReplaceHex(string line)
        {
            const string pattern = "\"0x[0123456789abcdef]+\"";
            var matches = Regex.Match(line, pattern);
            if (matches.Success)
            {
                string replaced = matches.Value.Replace("\"", string.Empty).Replace("0x", string.Empty);
                replaced = "0" + replaced;
                BigInteger number = BigInteger.Parse(replaced, NumberStyles.HexNumber);
                line = Regex.Replace(line, pattern, $"\"{number.ToString()}\"");
            }

            return line;
        }
        
        private static string ReplaceNegativeGas(string line)
        {
            // const string pattern = "\\\"-\\d+\\\"";
            // var matches = Regex.Match(line, pattern);
            // if (matches.Success)
            // {
            //     string replaced = matches.Value.Replace("-", string.Empty);
            //     line = Regex.Replace(line, pattern, replaced);
            // }
            //
            return line;
        }

        private static string ReplaceNumberNoString(string line)
        {
            const string pattern = ": -?\\d+";
            var matches = Regex.Match(line, pattern);
            if (matches.Success)
            {
                string replaced = $": \"{matches.Value.Substring(2)}\"";
                line = Regex.Replace(line, pattern, replaced);
            }

            return line;
        }
    }
}
using System;
using System.IO;

namespace Comparer
{
    class Program
    {
        static void Main(string[] args)
        {
            File.ReadAllLines("ether");

            // string[] lines1845 = File.ReadAllLines(@"C:\Users\tksta\Downloads\consensus_master\trace1845.json");
            // string[] lines34 = File.ReadAllLines(@"C:\Users\tksta\Downloads\consensus_1.8.34\trace1834.json");
            //
            // for (int i = 0; i < lines1845.Length; i++)
            // {
            //     if (lines1845[i].Trim() != lines34[i].Trim())
            //     {
            //         Console.WriteLine($"Difference in line: {i}");
            //         Console.WriteLine($"1.8.45 {lines1845[i]}");
            //         Console.WriteLine($"1.8.34 {lines34[i]}");
            //         break;
            //     }
            // }
            //
            // Console.ReadLine();
        }
    }
}
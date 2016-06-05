﻿using AutoComplete.Core;
using AutoComplete.Desktop;
using System.Diagnostics;

namespace AutoComplete.Clients.ConsoleClient
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Search();
        }

        private static void Search()
        {
            string headerPath = "../../../resources/blobs/fs_header.xml";
            string indexPath = "../../../resources/blobs/fs_index.bin";

            IIndexSearcher searcher = new InMemoryIndexSearcher(headerPath, indexPath);

            var sw = new Stopwatch();

            while (true)
            {
                System.Console.WriteLine("Type any term");
                string term = System.Console.ReadLine();

                sw.Restart();
                SearchResult searchResult = searcher.Search(term, 5, false);
                sw.Stop();

                if (searchResult != null && searchResult.Items != null)
                {
                    foreach (var item in searchResult.Items)
                    {
                        System.Console.WriteLine(item);
                    }
                }

                System.Console.WriteLine("Elapsed ms: " + sw.Elapsed.TotalMilliseconds);
            }
        }
    }
}
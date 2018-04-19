using System;
using System.Collections.Generic;
using System.Linq;
using BGG;
using static System.Console;

namespace Sampler
{
    class Program
    {
        static void PrintHist(IEnumerable<IRating> list)
        {
            for (int i = 1; i <= 10; i++)
            {
                WriteLine($"{i} :: {list.Count(r => Math.Round(r.Value) == i)}");
            }
            WriteLine($"Average is {list.Select(r => r.Value).Average():f2}");
        }
        static void BuildDist(List<IRating> list)
        {
            var distribution =
                list
                .GroupBy(r => string.Format($"{r.TimeStamp.Year}-{r.TimeStamp.Month:D2}"))
                .OrderBy(grp => grp.Key)
                .Select(grp => new { Month = grp.Key, Count = grp.Count() });
            WriteLine();
            foreach (var item in distribution)
            {
                WriteLine($"{item.Month} - {item.Count}");
            }
        }
        static void BuildSamples(List<IRating> list, int chunks)
        {
            int count = list.Count;
            var sorted = list.OrderBy(r => r.TimeStamp);
            int skip = 0;
            int sampleSize = count / chunks;
            int remainder = count % chunks;
            WriteLine($"Number of votes: {count}");
            do
            {
                var sample = sorted.Skip(skip).Take(sampleSize + remainder);
                skip = skip + sampleSize + remainder;
                remainder = 0;
                WriteLine();
                WriteLine($"Number of sample votes: {sample.Count()}");
                WriteLine($"Start date: {sample.Min(r => r.TimeStamp)}");
                WriteLine($"Finish date: {sample.Max(r => r.TimeStamp)}");
                PrintHist(sample);
            } while (skip < count);
        }
        static void Main(string[] args)
        {
            int objectId;
            int chunks;
            do
            {
                WriteLine("Enter game id.");
            } while (!int.TryParse(ReadLine(), out objectId));
            // Get the list of ratings
            var api = new API(new APIConfig());
            List<IRating> result = api.GetRatingsAsync(objectId).Result;
            // Monthly distribution
            BuildDist(result);
            // Sampling
            do
            {
                do
                {
                    WriteLine("Enter sample size.");
                } while (!int.TryParse(ReadLine(), out chunks));
                Clear();
                BuildSamples(result, chunks);
                WriteLine("Another samle? Yes/No");
            } while (ReadLine() == "Y");
        }
    }
}

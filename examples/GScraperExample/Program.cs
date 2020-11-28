using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using GScraper;

namespace GScraperExample
{
    internal static class Program
    {
        private static async Task Main()
        {
            Console.WriteLine("GScraper Example Program");
            var scraper = new GoogleScraper();

            while (true)
            {
                Console.Write("Query (enter \'e\' to exit): ");
                string text = Console.ReadLine();
                if (text == null || text == "e")
                {
                    break;
                }

                Console.Write("Limit?: ");
                if (!int.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int limit)) continue;

                IReadOnlyList<ImageResult> images;
                try
                {
                    images = await scraper.GetImagesAsync(text, limit).ConfigureAwait(false);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine(e);
                    continue;
                }
                catch (GScraperException e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                foreach (var image in images)
                {
                    Console.WriteLine($"Title: {image.Title}");
                    Console.WriteLine($"Link: {image.Link}");
                    Console.WriteLine($"ThumbnailLink: {image.ThumbnailLink}");
                    Console.WriteLine($"ContextLink: {image.ContextLink}");
                    Console.WriteLine($"DisplayLink: {image.DisplayLink}");
                    Console.WriteLine($"Width: {image.Width}");
                    Console.WriteLine($"Height: {image.Height}");
                    Console.WriteLine();
                }
            }

            scraper.Dispose();
        }
    }
}
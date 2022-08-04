using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GScraper;
using GScraper.Google;

namespace GScraperExample;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("GScraper Example Program");
        using var scraper = new GoogleScraper();
        // Other scrapers:
        // using var scraper = new GScraper.DuckDuckGo.DuckDuckGoScraper();
        // using var scraper = new GScraper.Brave.BraveScraper();

        while (true)
        {
            Console.Write("Query (enter 'e' to exit): ");
            string? text = Console.ReadLine();

            if (string.IsNullOrEmpty(text))
                continue;

            if (text == "e")
                break;

            IEnumerable<IImageResult> images;
            try
            {
                images = await scraper.GetImagesAsync(text);
            }
            catch (Exception e) when (e is HttpRequestException or GScraperException)
            {
                Console.WriteLine(e);
                continue;
            }

            bool enumerateAll = false;
            bool stop = false;
            foreach (var image in images)
            {
                Console.WriteLine();
                Console.WriteLine(JsonSerializer.Serialize(image, image.GetType(), new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine();

                if (!enumerateAll)
                {
                    Console.Write("Press 'n' to send the next image, 'a' to enumerate all images and 's' to stop: ");
                    var key = Console.ReadKey().Key;
                    Console.WriteLine();

                    switch (key)
                    {
                        case ConsoleKey.A:
                            enumerateAll = true;
                            break;

                        case ConsoleKey.S:
                            stop = true;
                            break;

                        default:
                            break;
                    }
                }

                if (stop)
                {
                    break;
                }
            }
        }
    }
}
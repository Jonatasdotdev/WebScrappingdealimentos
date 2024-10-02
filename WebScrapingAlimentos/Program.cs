using System;
using System.Threading.Tasks;

namespace WebScrapingAlimentos
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var scraper = new Scraper();
            await scraper.ExtrairDadosAsync();
        }
    }
}

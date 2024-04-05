using HtmlAgilityPack;

namespace SinoptikUaWeather
{
    public class Program
    {
        public static HttpClient client = new HttpClient();

        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.Write("Введіть назву міста: ");
            string city = Console.ReadLine();

            string url = $"https://ua.sinoptik.ua/погода-{city.ToLower()}";

            try
            {
                var htmlDocument = await GetWeatherData(url);
                DisplayWeatherData(htmlDocument);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Виникла помилка: {e.Message}");
            }
        }

        public static async Task<HtmlDocument> GetWeatherData(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(body);

            return htmlDoc;
        }

        public static void DisplayWeatherData(HtmlDocument htmlDocument)
        {
            List<string> ids = new()
                { "bd1", "bd2", "bd3", "bd4", "bd5", "bd6", "bd7" };
            Console.WriteLine("{0,-10} {1,-5} {2,-7} {3,-33} {4,-11} {5,-25}",
                "День", "Дата", "Місяць", "Характеристика погоди", "Мін. темп.", "Макс. темп.");

            foreach (string id in ids)
            {
                var div = htmlDocument.GetElementbyId(id);
                if (div != null)
                    DisplayWeatherForDay(div);
                else
                    Console.WriteLine($"Не вдалося знайти div з id='{id}'");
            }
        }

        public static void DisplayWeatherForDay(HtmlNode div)
        {
            string day = GetInnerText(div, ".//p[@class='day-link']") ?? GetInnerText(div, ".//p");
            string date = GetInnerText(div, $"//*[@id='{div.Id}']/p[2]");
            string month = GetInnerText(div, ".//p[@class='month']");
            string weather = div.SelectSingleNode($"//*[@id='{div.Id}']/div[1]")?.GetAttributeValue("title", "");
            string minTemp = GetInnerText(div, ".//div[@class='temperature']/div[@class='min']/span")?.Replace("&deg;", "°");
            string maxTemp = GetInnerText(div, ".//div[@class='temperature']/div[@class='max']/span")?.Replace("&deg;", "°");

            Console.WriteLine("{0,-10} {1,-5} {2,-7} {3,-33} {4,-11} {5,-25}",
                day, date, month, weather, minTemp, maxTemp);
        }

        public static string GetInnerText(HtmlNode node, string xPath)
        {
            return node.SelectSingleNode(xPath)?.InnerText;
        }
    }
}

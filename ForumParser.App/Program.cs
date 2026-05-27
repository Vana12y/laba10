using System.Net.Http;
using HtmlAgilityPack;
using ForumParser.DbLibrary;

class Program
{
    static async Task Main(string[] args)
    {
        string url = "https://news.ycombinator.com/item?id=352343";
        string dbPath = "forum_data.db";
        var db = new DbManager(dbPath);

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        try
        {
            Console.WriteLine($"Подключение к источнику: {url}");
            string html = await http.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'comtr')]");

            if (nodes == null)
            {
                Console.WriteLine("Ошибка: не удалось найти комментарии на странице. Проверьте URL.");
                return;
            }

            Console.WriteLine($"Найдено {nodes.Count} комментариев. Начинаем импорт в БД");
            int saved = 0;

            foreach (var n in nodes)
            {
                try
                {
                    string rawId = n.GetAttributeValue("id", "0");
                    long id = long.Parse(rawId);
                    var userNode = n.SelectSingleNode(".//a[@class='hnuser']");
                    string name = userNode != null ? userNode.InnerText.Trim() : "[deleted]";
                    var msgNode = n.SelectSingleNode(".//span[contains(@class, 'commtext')]");
                    if (msgNode == null) continue;
                    string msg = msgNode.InnerHtml.Trim();

                    if (name.Length > 256) name = name.Substring(0, 256);
                    if (msg.Length > 8096) msg = msg.Substring(0, 8096);

                    if (db.GetById(id) == null)
                    {
                        db.Add(new ForumMessage { Id = id, Name = name, Message = msg });
                        saved++;
                        Console.WriteLine($"Успешно добавлен пост #{id} от пользователя {name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Предупреждение: пропущен один комментарий из-за ошибки: {ex.Message}");
                }
            }

            Console.WriteLine("\n");
            Console.WriteLine($"Работа завершена успешно!");
            Console.WriteLine($"Добавлено новых записей в БД: {saved}");
            Console.WriteLine("");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка при работе парсера: {ex.Message}");
        }
    }
}
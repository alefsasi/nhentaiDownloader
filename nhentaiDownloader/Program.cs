using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace nhentaiDownloader
{
    public class HentaiInfo
    {

        public HentaiInfo(HtmlDocument page, string hentaiCode)
        {
            NumberOfPages = GetNumberOfPages(page);
            HentaiName = GetHentaiName(page);
            GalleryLink = GetGalleryLink(page);
            HentaiCode = hentaiCode;

        }

        public int NumberOfPages { get; set; }
        public string HentaiCode { get; set; }
        public string HentaiName { get; set; }
        public string GalleryLink { get; set; }


        private string GetHentaiName(HtmlDocument page)
        {
            var node = page.DocumentNode.SelectNodes("//h1[contains(@class, 'title')]/span[contains(@class, 'pretty')]");
            return node.FirstOrDefault().InnerText;
        }
        private string GetGalleryLink(HtmlDocument page)
        {
            var node = page.DocumentNode.SelectNodes("//div[contains(@id, 'cover')]/a/img");
            var dataSource = node.FirstOrDefault().GetAttributeValue("data-src", string.Empty);

            return dataSource.Replace("cover.jpg", "").Replace("t.", "i.");
        }
        private int GetNumberOfPages(HtmlDocument page)
        {
            try
            {
                var node = page.DocumentNode.SelectNodes("//div[contains(@class, 'tag-container field-name')]");
                var tagValue = node.FirstOrDefault(x => x.InnerText.Contains("Pages:"));
                var numberOfPages = Regex.Match(tagValue.InnerText.Trim(), @"\d+").Value;

                return int.Parse(numberOfPages);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }

    class Program
    {
        static string NHENTAILINK = "https://nhentai.net/g/";

        static void Main(string[] args)
        {

            //"368046"
            Console.Write("Digite o código de um hentai: ");
            var hentaicode = Console.ReadLine();

            var hentai = LoadHentai(hentaicode);

            if (hentai == null)
                Console.WriteLine("Esse não é um código válido");

            DownloadHentai(hentai);

            Console.WriteLine(hentai.HentaiName);

            Console.ReadKey();

        }
        public static void CreateFolder(string folderName)
        {
            try
            {

                if (Directory.Exists(folderName))
                {
                    return;
                }
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(folderName);
            }
            catch (Exception)
            {
                throw new Exception("A criação da Pasta Falhou");
            }
        }
        private static void DownloadHentai(HentaiInfo hentai)
        {
            var client = new WebClient();

            CreateFolder(hentai.HentaiName);

            for (var i = 1; i <= hentai.NumberOfPages; i++)
            {
                var capLink = $"{hentai.GalleryLink}{i}.jpg";
                var pageName = $"{hentai.HentaiName}//{FormatPageName(hentai.NumberOfPages, i)}";

                if (!File.Exists(pageName))
                {
                    client.DownloadFile(new Uri(capLink), pageName);
                    Thread.Sleep(500);
                }
                Console.WriteLine($"Baixando: {hentai.HentaiName}.... Página: {i}");
            }

            Console.WriteLine("Download Concluído....");
            client.Dispose();
        }
        private static string FormatPageName(int numberOfPages, int numberPage)
        {
            if (numberOfPages > 99)
                return numberPage.ToString().PadLeft(3, '0') + ".jpg";
            if (numberOfPages > 9)
                return numberPage.ToString().PadLeft(2, '0') + ".jpg";

            return numberPage + ".jpg";
        }
        private static HentaiInfo LoadHentai(string hentaiCode)
        {
            try
            {
                var page = new HtmlWeb().Load($"{NHENTAILINK}{hentaiCode}");
                var isValidPage = page.DocumentNode.SelectNodes("//div[contains(@class, 'container error')]");

                if (isValidPage != null)
                    return null;

                return new HentaiInfo(page, hentaiCode);
            }
            catch (Exception ex)
            {
                throw;
            }

        }



    }
}

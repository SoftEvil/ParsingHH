using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections;
using System.Threading.Tasks;

namespace TestParsing
{
    class Page
    {
        PageDocument doc = new PageDocument();
      //  private static string URL { get; set; }
   //     private static string PageEnd { get; set; }
    //    private static HtmlAgilityPack.HtmlDocument[] DocumentArray { get; set; }
        public async Task GetLastPage(int Area, int Period)
        {
            string URL = "https://hh.ru/search/resume?exp_period=all_time&order_by=relevance&specialization=17&no_magic=true&area=" + Area + "&text=&pos=full_text&logic=normal&clusters=true&search_period=" + Period;

            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(URL);
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(html);
            //список кнопок выбора страницы
            var PageList = htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("data-qa", "").Equals("pager-block")).ToList();
            //лист кнопки
            var PageListItem = PageList[0].Descendants("a").Where(node => node.GetAttributeValue("data-qa", "").Equals("pager-page")).ToList();
            //номер последней страницы
            string PageEnd = PageListItem[PageListItem.Count - 1].GetAttributeValue("data-page", "").ToString();
            HtmlDocument[] DocumentArray = new HtmlAgilityPack.HtmlDocument[Convert.ToInt32(PageEnd) + 1];

            await doc.CreateDocument(URL, Convert.ToInt32(PageEnd), DocumentArray);
        }
    }

}

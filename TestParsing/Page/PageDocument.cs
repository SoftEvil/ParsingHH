using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestParsing
{
    class PageDocument
    {
        Hash hash = new Hash();
        public async Task CreateDocument(string URL, int PageEnd, HtmlDocument[] DocumentArray)
        {
            for (int page = 0; page <= Convert.ToInt32(PageEnd); page++)
            {
                //открываем стиницу номер "page"
                var urlPage = URL + "&page=" + page;

                var httpClientPage = new HttpClient();
                var htmlPage = await httpClientPage.GetStringAsync(urlPage);
                var htmlPageDocument = new HtmlAgilityPack.HtmlDocument();
                htmlPageDocument.LoadHtml(htmlPage);
                DocumentArray[page] = htmlPageDocument;
            }
            await hash.ReadRezumeHash(PageEnd, DocumentArray);
        }
    }
}

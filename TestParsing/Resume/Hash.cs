using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestParsing
{
    public class Hash
    {
        ResumeReader read = new ResumeReader();
        //private static int Salary { get; set; }
        public async Task ReadRezumeHash(int PageEnd, HtmlDocument[] DocumentArray)
        {
            List<string> ResumeHash = new List<string>();
            int Salary = 0;

            //считываем все страницы
            for (int id = 0; id <= Convert.ToInt32(PageEnd); id++)
            {
                //лист со списком резюме
                var ResumeListHtml = DocumentArray[id].DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("data-qa", "").Equals("resume-serp__results-search")).ToList();
                //список резюме текущей страницы
                var ResumeListItems = ResumeListHtml[0].Descendants("div").Where(node => node.GetAttributeValue("itemscope", "").Equals("itemscope")).ToList();
                //поле ЗП
                var ResumeListCompensation = ResumeListHtml[0].Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("output__compensation")).ToList(); ;

                int i = 0;  //номер резюме на стринице
                foreach (var ResumeListItem in ResumeListItems)
                {
                    var ResumeCompensation = ResumeListCompensation[i].FirstChild.InnerText.ToString();
                    ResumeCompensation = ResumeCompensation.Trim();
                    //если не пустое, то сохраняем хэш резюме
                    if (ResumeCompensation.Length > 0 && ResumeCompensation.Contains("руб.") && ResumeHash.Any(r => r == ResumeListItems[i].GetAttributeValue("data-hh-resume-hash", "").ToString()) == false)
                    {
                        ResumeCompensation = ResumeCompensation.Remove(ResumeCompensation.IndexOf(' '));

                        Salary += Convert.ToInt32(ResumeCompensation);
                        //значение хэша резюме
                        ResumeHash.Add(ResumeListItems[i].GetAttributeValue("data-hh-resume-hash", "").ToString());
                        //количество резюме всего
                    }
                    i++;
                }
            }
            //считываем данные из резюме
            await read.ReadResume(Salary, ResumeHash);

        }
    }
}

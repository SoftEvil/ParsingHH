using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestParsing
{
    class ResumeReader
    {
        
        public async Task ReadResume(int Salary, List<string> ResumeHash)
        {
            using (var db = new DBParsingEntities1())
            {
                var ResumeTable = db.Resume;
                var LangTable = db.Language;
                var SpecTable = db.Spec;
                var SkillTable = db.Skills;

                var sredSalary = Salary / ResumeHash.Count;
                var startSalary = sredSalary * 0.9;
                var endSalary = sredSalary * 1.1;
                for (int id = 0; id < ResumeHash.Count; id++)
                {
                    var urlPage = "https://hh.ru/resume/" + ResumeHash[id];

                    var httpClientPage = new HttpClient();
                    var htmlPage = await httpClientPage.GetStringAsync(urlPage);
                    var htmlPageDocument = new HtmlAgilityPack.HtmlDocument();
                    htmlPageDocument.LoadHtml(htmlPage);

                    //контейнер резюме
                    var PageList = htmlPageDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("resume-applicant")).ToList();
                    var CompensationBlock = PageList[0].Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("resume-block__salary")).ToList();
                    var Compensation = CompensationBlock[0].FirstChild.InnerText.ToString();
                    Compensation = Regex.Replace(Compensation, @"\s*", "").Trim();
                    var SalaryResume = Convert.ToInt32(Compensation.Remove(Compensation.IndexOf('р')));

                    //если входит в +-10% от средней
                    if (SalaryResume >= startSalary && SalaryResume <= endSalary && await ResumeTable.FindAsync(ResumeHash[id])==null)
                    {
                        Resume resume = new Resume();

                        resume.Hash = ResumeHash[id];
                        resume.Salary = SalaryResume;
                        /////////////////Шапка резюме////////////////
                        var HeaderBlock = PageList[0].Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("resume-header-block")).ToList();
                        if (HeaderBlock[0].InnerHtml.Contains("gender"))
                        {
                            resume.Gender = HeaderBlock[0].Descendants("span").Where(node => node.GetAttributeValue("itemprop", "").Equals("gender")).FirstOrDefault().InnerText;

                        }
                        //получаем дату
                        if (HeaderBlock[0].InnerHtml.Contains("birthDate"))
                        {
                            var HeaderMainInfoDataList = HeaderBlock[0].Descendants("meta").Where(node => node.GetAttributeValue("itemprop", "").Equals("birthDate")).ToList();
                            resume.Birthday = Convert.ToDateTime(HeaderMainInfoDataList[0].GetAttributeValue("content", "").ToString());
                        }
                        //получаем место
                        if (HeaderBlock[0].InnerHtml.Contains("addressLocality"))
                        {
                            resume.Address = HeaderBlock[0].Descendants("span").Where(node => node.GetAttributeValue("itemprop", "").Equals("addressLocality")).FirstOrDefault().InnerText;
                        }

                        ///////////////////////Основная часть резюме//////////////////////
                        var MainBlock = PageList[0].Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("resume-wrapper")).ToList();
                        //должность
                        if (MainBlock[0].InnerHtml.Contains("resume-block-title-position"))
                        {
                            resume.Position = MainBlock[0].Descendants("span").Where(node => node.GetAttributeValue("data-qa", "").Equals("resume-block-title-position")).FirstOrDefault().InnerText;
                        }

                        var TimeTableList = MainBlock[0].Descendants("p").ToList();
                        if (TimeTableList[0].InnerText.Contains("Занятость:"))
                            resume.Employment = TimeTableList.FindAll(nat => nat.InnerText.Contains("Занятость")).FirstOrDefault().InnerText.ToString();
                        if (TimeTableList[1].InnerText.Contains("График"))
                            resume.WorkTime = TimeTableList[1].FirstChild.InnerText.ToString();

                        //опыт работы
                        if (MainBlock[0].InnerHtml.Contains("worksFor"))
                        {
                            var Experience = MainBlock[0].Descendants("div").Where(node => node.GetAttributeValue("data-qa", "").Equals("resume-block-experience")).ToList();
                            //Список предыдущих работ
                            if (Experience.Count > 0)
                            {
                                var WorkExperience = Experience[0].Descendants("span").FirstOrDefault().InnerText.Trim().ToString();
                                resume.Experience = WorkExperience;
                            }
                        }
                        //Обо мне
                        if (MainBlock[0].InnerHtml.Contains("resume-block-skills"))
                        {
                            var Skills = MainBlock[0].Descendants("div").Where(node => node.GetAttributeValue("data-qa", "").Equals("resume-block-skills")).FirstOrDefault().InnerText.ToString();
                            resume.Info = Skills;
                        }
                        //образование
                        if (MainBlock[0].InnerHtml.Contains("resume-block-education"))
                        {
                            var EducationBlok = MainBlock[0].Descendants("div").Where(node => node.GetAttributeValue("data-qa", "").Equals("resume-block-education")).ToList();
                            if (EducationBlok.Count > 0)
                            {
                                string Education = string.Empty;
                                var EducationDegree = EducationBlok[0].Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("resume-block__title-text resume-block__title-text_sub")).FirstOrDefault().InnerText.ToString();
                                if (EducationBlok[0].InnerHtml.Contains("resume-block-item-gap"))
                                {
                                    Education = EducationBlok[0].Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("resume-block-item-gap")).FirstOrDefault().InnerText.ToString();
                                }
                                else
                                {
                                    Education = EducationBlok[0].Descendants("p").FirstOrDefault().InnerText.ToString();
                                }
                                Education = Education.Insert(4, "   ");
                                resume.Education = EducationDegree + "  " + Education;
                            }
                        }


                        //специализация

                        if (MainBlock[0].InnerHtml.Contains("resume-block-position-specialization"))
                        {
                            var Specialization = MainBlock[0].Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("bloko-gap bloko-gap_bottom")).ToList();
                            if (Specialization.Count > 0)
                            {
                                
                                var SpecializationsList = Specialization[0].Descendants("li").Where(node => node.GetAttributeValue("data-qa", "").Equals("resume-block-position-specialization")).ToList();
                                //список специализаций
                                for (int i = 0; i < SpecializationsList.Count; i++)
                                {
                                    Spec spec = new Spec();
                                    var PromSpec = SpecializationsList[i].InnerText.Trim().ToString();
                                    if (await SpecTable.AnyAsync(s => s.Specialization == PromSpec)==false)
                                    {
                                        spec.Specialization = PromSpec;
                                        db.Spec.Add(spec);
                                        resume.Spec.Add(spec);
                                    }
                                    else
                                    {
                                        spec = await SpecTable.FirstOrDefaultAsync(s => s.Specialization == PromSpec);
                                        resume.Spec.Add(spec);                                    }
                                    await db.SaveChangesAsync();
                                }
                               
                            }
                        }

                        //ключевые навыки                        
                        if (MainBlock[0].InnerHtml.Contains("skills-table"))
                        {
                            var SkillsTable = MainBlock[0].Descendants("div").Where(node => node.GetAttributeValue("data-qa", "").Equals("skills-table")).ToList();
                            if (SkillsTable.Count > 0)
                            {
                                
                                var SkillsList = SkillsTable[0].Descendants("span").Where(node => node.GetAttributeValue("data-qa", "").Equals("bloko-tag_inline")).ToList();

                                for (int s = 0; s < SkillsList.Count; s++)
                                {
                                    Skills skills = new Skills();
                                    var PromSkill = SkillsList[s].Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("Bloko-TagList-Text")).FirstOrDefault().InnerText.Trim().ToString();
                                    if (await SkillTable.AnyAsync(q => q.Skill == PromSkill) == false)
                                    {
                                        skills.Skill = PromSkill;
                                        db.Skills.Add(skills);
                                        resume.Skills.Add(skills);
                                        
                                    }
                                    else
                                    {
                                        skills = await SkillTable.FirstOrDefaultAsync(q => q.Skill == PromSkill);
                                        resume.Skills.Add(skills);
                                    }
                                    await db.SaveChangesAsync();
                                }
                                
                            }
                        }
                        //языки
                        
                        if (MainBlock[0].InnerHtml.Contains("resume-block-language-item"))
                        {
                            var Languages = MainBlock[0].Descendants("p").Where(node => node.GetAttributeValue("data-qa", "").Equals("resume-block-language-item")).ToList();
                            if (Languages.Count > 0)
                            {
                                for (int l = 0; l < Languages.Count; l++)
                                {
                                    Language lang = new Language();
                                    var PromLang = Languages[l].FirstChild.InnerText.Trim().ToString();
                                    if (await LangTable.AnyAsync(q => q.Language1 == PromLang) == false)
                                    {
                                        lang.Language1 = PromLang;
                                        db.Language.Add(lang);
                                        resume.Language.Add(lang);                                        
                                    }
                                    else
                                    {
                                        lang = await LangTable.FirstOrDefaultAsync(q => q.Language1 == PromLang);
                                        resume.Language.Add(lang);
                                    }
                                    await db.SaveChangesAsync();
                                }                 
                            }
                        }
                        db.Resume.Add(resume);
                        await db.SaveChangesAsync();
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}

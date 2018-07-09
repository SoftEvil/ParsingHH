using System;
using System.Net.Http;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TestParsing
{
    public partial class Form1 : Form
    {
        Page page = new Page();
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 2;
            comboBox2.SelectedIndex = 0;
            btn_CancelSearch.Enabled = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            await page.GetLastPage(Area, Period);
            resumeDataGridView.Update();
            resumeDataGridView.Refresh();
            UpdateTable();
            button1.Enabled = true;
        }

        private static int Area { get; set; }
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1) Area = 1;  //москва
            else if (comboBox1.SelectedIndex == 2) Area = 1806;  //ярославcкая обл.
            else if (comboBox1.SelectedIndex == 0) Area = 113;  //россия
        }
        private static int Period { get; set; }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0) Period = 1;  //Сутки
            else if (comboBox2.SelectedIndex == 1) Period = 3;  //Последние 3 дня
            else if (comboBox2.SelectedIndex == 2) Period = 7;  //Неделя
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dBParsingDataSet.Resume". При необходимости она может быть перемещена или удалена.
            this.resumeTableAdapter.Fill(this.dBParsingDataSet.Resume);


            DataSet();
        }

        private void ClearList()
        {
            listLang.Items.Clear();
            listSkills.Items.Clear();
            listSpec.Items.Clear();
        }
        private void DataSet()
        {
            txtURL.Text = "https://hh.ru/resume/" + hashTextBox.Text;
            ClearList();
            using (var db = new DBParsingEntities1())
            {
                var resume = db.Resume.Find(hashTextBox.Text);
                var ResumeLang = resume.Language.ToArray();
                var ResumeSkills = resume.Skills.ToArray();
                var ResumeSpec = resume.Spec.ToArray();
                for (int l = 0; l < ResumeLang.Length; l++)
                {
                    listLang.Items.Add(ResumeLang[l].Language1.ToString());
                }
                for (int l = 0; l < ResumeSkills.Length; l++)
                {
                    listSkills.Items.Add(ResumeSkills[l].Skill.ToString());
                }
                for (int l = 0; l < ResumeSpec.Length; l++)
                {
                    listSpec.Items.Add(ResumeSpec[l].Specialization.ToString());
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (txtURL.Text != "")
            {
                Clipboard.SetText(txtURL.Text);
            }
        }


        private void SearchBox_SelectedValueChanged(object sender, EventArgs e)
        {
            using (var db = new DBParsingEntities1())
            {
                var resume = db.Resume;
                EqualsBox.Items.Clear();
                List<string> EqualList = new List<string>();

                if (SearchBox.SelectedIndex == 0)  //Salary
                {
                    foreach (var r in resume)
                    {
                        EqualList.Add(r.Salary.ToString());
                    }
                }
                if (SearchBox.SelectedIndex == 1)  //Gender
                {
                    foreach (var r in resume)
                    {
                        EqualList.Add(r.Gender.ToString());
                    }
                }
                if (SearchBox.SelectedIndex == 2)  //Address
                {
                    foreach (var r in resume)
                    {
                        EqualList.Add(r.Address.ToString());
                    }
                }
                if (SearchBox.SelectedIndex == 3)  //Position
                {
                    foreach (var r in resume)
                    {
                        EqualList.Add(r.Position.ToString());
                    }
                }
                if (SearchBox.SelectedIndex == 4)  //Employment
                {
                    foreach (var r in resume)
                    {
                        EqualList.Add(r.Employment.ToString());
                    }
                }
                if (SearchBox.SelectedIndex == 5)  //Work Time
                {
                    foreach (var r in resume)
                    {
                        EqualList.Add(r.WorkTime.ToString());
                    }
                }
                if (SearchBox.SelectedIndex == 6)  //Education
                {
                    foreach (var r in resume)
                    {
                        EqualList.Add(r.Education.ToString());
                    }
                }
                if (SearchBox.SelectedIndex == 7)  //Lang
                {
                    foreach (var r in resume)
                    {
                        var ResumeLang = r.Language.ToArray();
                        for (int l = 0; l < ResumeLang.Length; l++)
                        {
                            EqualList.Add(ResumeLang[l].Language1.ToString());
                        }
                    }
                }
                if (SearchBox.SelectedIndex == 8)  //Skills
                {
                    foreach (var r in resume)
                    {
                        var ResumeSkills = r.Skills.ToArray();
                        for (int l = 0; l < ResumeSkills.Length; l++)
                        {
                            EqualList.Add(ResumeSkills[l].Skill.ToString());
                        }
                    }
                }
                if (SearchBox.SelectedIndex == 9)  //Spec
                {
                    foreach (var r in resume)
                    {
                        var ResumeSpec = r.Spec.ToArray();
                        for (int l = 0; l < ResumeSpec.Length; l++)
                        {
                            EqualList.Add(ResumeSpec[l].Specialization.ToString());
                        }
                    }
                }
                EqualsBox.Items.AddRange(EqualList.Distinct().ToArray());
            }
        }

        private void btn_Search_Click(object sender, EventArgs e)
        {
            btn_CancelSearch.Enabled = true;
            using (var db = new DBParsingEntities1())
            {
                var resume = db.Resume;
                List<Resume> EqualList = new List<Resume>();

                if (SearchBox.SelectedIndex == 0)  //Salary
                {
                    foreach (var r in resume)
                    {
                        if (r.Salary.ToString() == EqualsBox.Text)
                            EqualList.Add(r);
                    }
                }
                if (SearchBox.SelectedIndex == 1)  //Gender
                {
                    foreach (var r in resume)
                    {
                        if (r.Gender.ToString() == EqualsBox.Text)
                            EqualList.Add(r);
                    }
                }
                if (SearchBox.SelectedIndex == 2)  //Address
                {
                    foreach (var r in resume)
                    {
                        if (r.Address.ToString() == EqualsBox.Text)
                            EqualList.Add(r);
                    }
                }
                if (SearchBox.SelectedIndex == 3)  //Position
                {
                    foreach (var r in resume)
                    {
                        if (r.Position.ToString() == EqualsBox.Text)
                            EqualList.Add(r);
                    }
                }
                if (SearchBox.SelectedIndex == 4)  //Employment
                {
                    foreach (var r in resume)
                    {
                        if (r.Employment.ToString() == EqualsBox.Text)
                            EqualList.Add(r);
                    }
                }
                if (SearchBox.SelectedIndex == 5)  //Work Time
                {
                    foreach (var r in resume)
                    {
                        if (r.WorkTime.ToString() == EqualsBox.Text)
                            EqualList.Add(r);
                    }
                }
                if (SearchBox.SelectedIndex == 6)  //Education
                {
                    foreach (var r in resume)
                    {
                        if (r.Education.ToString() == EqualsBox.Text)
                            EqualList.Add(r);
                    }
                }
                if (SearchBox.SelectedIndex == 7)  //Lang
                {
                    foreach (var r in resume)
                    {
                        var ResumeLang = r.Language.ToArray();
                        for (int l = 0; l < ResumeLang.Length; l++)
                        {
                            if (ResumeLang[l].Language1.ToString() == EqualsBox.Text)
                                EqualList.Add(r);
                        }
                    }
                }
                if (SearchBox.SelectedIndex == 8)  //Skills
                {
                    foreach (var r in resume)
                    {
                        var ResumeSkills = r.Skills.ToArray();
                        for (int l = 0; l < ResumeSkills.Length; l++)
                        {
                            if (ResumeSkills[l].Skill.ToString() == EqualsBox.Text)
                                EqualList.Add(r);
                        }
                    }
                }
                if (SearchBox.SelectedIndex == 9)  //Spec
                {
                    foreach (var r in resume)
                    {
                        var ResumeSpec = r.Spec.ToArray();
                        for (int l = 0; l < ResumeSpec.Length; l++)
                        {
                            if (ResumeSpec[l].Specialization.ToString() == EqualsBox.Text)
                                EqualList.Add(r);
                        }
                    }
                }
                resumeDataGridView.DataSource = EqualList;
            }
        }

        private void btn_CancelSearch_Click(object sender, EventArgs e)
        {
            btn_CancelSearch.Enabled = false;
            UpdateTable();
        }
        private void UpdateTable()
        {
            using (var db = new DBParsingEntities1())
            {
                var resumeDS = db.Resume.ToArray();
                resumeDataGridView.DataSource = resumeDS;
            }
        }

        private void resumeDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            DataSet();
        }
    }
}

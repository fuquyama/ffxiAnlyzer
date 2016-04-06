using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HtmlAgilityPack;

namespace ffxiAnlyzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            //if (openFileDialog1.ShowDialog() != DialogResult.OK)
            //{
            //    return;
            //}

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(@"<font color=""#808480""><b><!--6a,00,00,80808480,000050ed,00005faf,000f,00,01,02,00,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00--> Jajaの震天動地の章！</b></font><br>");
            var nodes = doc.DocumentNode.SelectNodes("//b");

            foreach (var b in nodes)
            {
                string str = b.LastChild.InnerText;
            }
        }
    }
}

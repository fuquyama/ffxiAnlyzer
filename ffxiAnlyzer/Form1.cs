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
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(openFileDialog1.FileName);
            //doc.LoadHtml(@"<font color=""#808480""><b><!--6a,00,00,80808480,000050ed,00005faf,000f,00,01,02,00,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00--> Jajaの震天動地の章！</b></font><br>");
            var nodes = doc.DocumentNode.SelectNodes("//b");

            List<string> innerTexts = new List<string>();
            foreach (var b in nodes)
            {
                string str = b.LastChild.InnerText;
                innerTexts.Add(str);
            }

            dmgDataSet[] ds = parseDamageLog(innerTexts.ToArray());
        }


        private dmgDataSet[] parseDamageLog(string[] messages)
        {
            var ds = new List<dmgDataSet>();
            bool damageCountStart = false;
            string name = string.Empty;
            string source = string.Empty;

            dmgDataSet localSet = new dmgDataSet();
            for (int i = 0; i < messages.Length; i++)
            {
                string mes = messages[i].Trim();
                if (mes.IndexOf("が発動。") > 0)
                {
                    name = mes.Substring(0, mes.IndexOf("の"));
                    source = mes.Substring(mes.IndexOf("の") + 1, mes.IndexOf("が") - (mes.IndexOf("の") + 1));
                    damageCountStart = true;
                    continue;
                }
                if (damageCountStart && (mes.IndexOf("→") == 0) && (mes.IndexOf("に、") > 0))
                {
                    if (mes.IndexOf("→マジックバースト！") >= 0)
                    {
                        localSet.name = name;
                        localSet.source = source;
                        localSet.MB = true;
                        localSet.target = mes.Substring("→マジックバースト！".Length, mes.IndexOf("に") - "→マジックバースト！".Length);
                        localSet.damage = int.Parse(mes.Substring(mes.IndexOf("に、") + "に、".Length, mes.IndexOf("ダメージ。") - (mes.IndexOf("に、") + "に、".Length)));
                    }
                    else
                    {
                        localSet.name = name;
                        localSet.source = source;
                        localSet.MB = false;
                        localSet.target = mes.Substring("→".Length, mes.IndexOf("に、") - "→".Length);
                        localSet.damage = int.Parse(mes.Substring(mes.IndexOf("に、") + "に、".Length, mes.IndexOf("ダメージ。") - (mes.IndexOf("に、") + "に、".Length)));
                    }
                    ds.Add(localSet);
                    localSet = new dmgDataSet();
                }
                else
                {
                    damageCountStart = false;
                }
            }

            return ds.ToArray();
        }

    }




    public class dmgDataSet
    {
        public string name;
        public string source;
        public string target;
        public int damage;
        public bool MB;

        public dmgDataSet()
        {
            name = string.Empty;
            source = string.Empty;
            target = string.Empty;
            damage = 0;
            MB = false;
        }
    }
}

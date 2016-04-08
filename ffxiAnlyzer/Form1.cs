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
        string fileName = string.Empty;

        DataSet1 dmgDataSet;
        List<string> sources;

        public Form1()
        {
            InitializeComponent();

            dmgDataSet = new DataSet1();
            sources = new List<string>();
        }

        /// <summary>
        /// 解析開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTest_Click(object sender, EventArgs e)
        {
            var dmgDataSetList = new List<DmgDataSet>();

            dmgDataSet.DataTable2.Clear();

            var names = checkedListBox1.CheckedItems;
            foreach (string name in names)
            {
                var targets = checkedListBox2.CheckedItems;
                foreach (string target in targets)
                {
                    foreach (string source in sources)
                    {
                        var ds1 = dmgDataSet.DataTable1.Select(string.Format("name = '{0}' AND target = '{1}' AND source = '{2}'", name, target, source));

                        if (ds1.Length > 0)
                        {
                            UInt32 dmg = 0;
                            UInt32 dmgSum = 0;
                            UInt32 count = 0;
                            UInt32 dmgMax = UInt32.MinValue;
                            UInt32 dmgMin = UInt32.MaxValue;
                            foreach (var dr in ds1)
                            {
                                dmg = (UInt32)dr["damage"];
                                dmgSum += dmg;
                                if (dmg > dmgMax)
                                {
                                    dmgMax = dmg;
                                }
                                if (dmg < dmgMin)
                                {
                                    dmgMin = dmg;
                                }
                                count++;
                            }
                            var dds = new DmgDataSet();
                            dds.name = name;
                            dds.target = target;
                            dds.source = source;
                            dds.damage = dmgSum;
                            dds.damageMax = dmgMax;
                            dds.damageMin = dmgMin;
                            dds.count = count;
                            dmgDataSetList.Add(dds);
                            var dr2 = (DataSet1.DataTable2Row)dmgDataSet.DataTable2.NewRow();
                            dr2.name = name;
                            dr2.target = target;
                            dr2.source = source;
                            dr2.damageTot = dmgSum;
                            dr2.damageMax = dmgMax;
                            dr2.damageMin = dmgMin;
                            dr2.damageAve = dds.avarage;
                            dr2.count = count;
                            dmgDataSet.DataTable2.Rows.Add(dr2);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// パーサー
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private DataSet1 parseDamageLog(string[] messages)
        {
            var ds = new DataSet1();
            bool damageCountStart = false;
            string name = string.Empty;
            string source = string.Empty;

            var dsRaw = ds.DataTable1.NewRow();
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
                        dsRaw["name"] = name;
                        dsRaw["source"] = source + "_MB";
                        dsRaw["target"] = mes.Substring("→マジックバースト！".Length, mes.IndexOf("に") - "→マジックバースト！".Length);
                        dsRaw["damage"] = UInt32.Parse(mes.Substring(mes.IndexOf("に、") + "に、".Length, mes.IndexOf("ダメージ。") - (mes.IndexOf("に、") + "に、".Length)));
                    }
                    else
                    {
                        dsRaw["name"] = name;
                        dsRaw["source"] = source;
                        dsRaw["target"] = mes.Substring("→".Length, mes.IndexOf("に、") - "→".Length);
                        dsRaw["damage"] = UInt32.Parse(mes.Substring(mes.IndexOf("に、") + "に、".Length, mes.IndexOf("ダメージ。") - (mes.IndexOf("に、") + "に、".Length)));
                    }
                    ds.DataTable1.Rows.Add(dsRaw);
                    dsRaw = ds.DataTable1.NewRow();
                }
                else
                {
                    damageCountStart = false;
                }
            }

            return ds;
        }


        /// <summary>
        /// メニューバー：ファイル：開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            fileName = openFileDialog1.FileName;

            this.Cursor = Cursors.WaitCursor;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(fileName);
            //doc.LoadHtml(@"<font color=""#808480""><b><!--6a,00,00,80808480,000050ed,00005faf,000f,00,01,02,00,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00--> Jajaの震天動地の章！</b></font><br>");
            var nodes = doc.DocumentNode.SelectNodes("//b");

            List<string> innerTexts = new List<string>();
            foreach (var b in nodes)
            {
                string str = b.LastChild.InnerText;
                innerTexts.Add(str);
            }

            dmgDataSet = parseDamageLog(innerTexts.ToArray());

            // Nameリストに名前を追加
            foreach (var dr in dmgDataSet.DataTable1)
            {
                bool newName = true;
                foreach (string item in checkedListBox1.Items)
                {
                    if (item == dr.name)
                    {
                        newName = false;
                    }
                }
                if (newName)
                {
                    checkedListBox1.Items.Add(dr.name, CheckState.Unchecked);
                }
            }

            // Targetリストに名前を追加
            foreach (var dr in dmgDataSet.DataTable1)
            {
                bool newName = true;
                foreach (string item in checkedListBox2.Items)
                {
                    if (item == dr.target)
                    {
                        newName = false;
                    }
                }
                if (newName)
                {
                    checkedListBox2.Items.Add(dr.target, CheckState.Unchecked);
                }
            }

            // Sourceリストに名前を追加
            foreach (var dr in dmgDataSet.DataTable1)
            {
                bool newName = true;
                foreach (string item in sources)
                {
                    if (item == dr.source)
                    {
                        newName = false;
                    }
                }
                if (newName)
                {
                    sources.Add(dr.source);
                }
            }

            this.Cursor = Cursors.Default;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedItem.ToString())
            {
                case "Name毎(Totalダメージ)":
                    break;
                case "Target毎":
                    break;
                case "ダメージソース毎":
                    break;
                default:
                    break;
            }
        }
    }


    class DmgDataSet
    {
        public string name;
        public string target;
        public string source;
        public UInt32 damage;
        public UInt32 damageMax;
        public UInt32 damageMin;
        public UInt32 count;
        public double avarage { get { return (double)damage / (double)count; } }

        public DmgDataSet()
        {
            name = "";
            target = "";
            source = "";
            damage = 0;
            damageMax = 0;
            damageMin = 0;
            count = 0;
        }
    }

}

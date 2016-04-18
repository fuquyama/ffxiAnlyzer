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

        public Form1()
        {
            InitializeComponent();

            dmgDataSet = new DataSet1();
        }

        /// <summary>
        /// 解析開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTest_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            // name,target,sourceの統計(Table2)
            {
                dmgDataSet.DataTable2.Clear();
                var names = checkedListBox1.CheckedItems;
                foreach (string name in names)
                {
                    var targets = checkedListBox2.CheckedItems;
                    foreach (string target in targets)
                    {
                        string repalcedName = name.Replace("'", "''");
                        string repalcedTarget = target.Replace("'", "''");
                        var sources = checkedListBox3.CheckedItems;
                        foreach (string source in sources)
                        {
                            var ds1 = dmgDataSet.DataTable1.Select(string.Format("name = '{0}' AND target = '{1}' AND source = '{2}'", repalcedName, repalcedTarget, source));

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
                                var dr2 = (DataSet1.DataTable2Row)dmgDataSet.DataTable2.NewRow();
                                dr2.name = name;
                                dr2.target = target;
                                dr2.source = source;
                                dr2.damageTot = dmgSum;
                                dr2.damageMax = dmgMax;
                                dr2.damageMin = dmgMin;
                                dr2.damageAve = (double)dmgSum / (double)count;
                                dr2.count = count;
                                dmgDataSet.DataTable2.Rows.Add(dr2);
                            }
                        }
                    }
                }
            }

            // name毎の集計(Table3)とtarget毎の集計(Table4)
            {
                dmgDataSet.DataTable3.Clear();
                dmgDataSet.DataTable4.Clear();
                var names = checkedListBox1.CheckedItems;
                foreach (string name in names)
                {
                    var targets = checkedListBox2.CheckedItems;
                    foreach (string target in targets)
                    {
                        string repalcedName = name.Replace("'", "''");
                        string repalcedTarget = target.Replace("'", "''");
                        var ds1 = dmgDataSet.DataTable1.Select(string.Format("name = '{0}' AND target = '{1}'", repalcedName, repalcedTarget));

                        if (ds1.Length > 0)
                        {
                            UInt32 dmg = 0;
                            UInt32 dmgSum = 0;
                            foreach (var dr in ds1)
                            {
                                dmg = (UInt32)dr["damage"];
                                dmgSum += dmg;
                            }
                            var dr2 = (DataSet1.DataTable4Row)dmgDataSet.DataTable4.NewRow();
                            dr2.target = target;
                            dr2.name = name;
                            dr2.damageTot = dmgSum;
                            dmgDataSet.DataTable4.Rows.Add(dr2);
                        }
                    }

                    var ds2 = dmgDataSet.DataTable4.Select(string.Format("name = '{0}'", name));
                    if (ds2.Length > 0)
                    {
                        UInt32 dmg = 0;
                        UInt32 dmgSum = 0;
                        foreach (var dr in ds2)
                        {
                            dmg = (UInt32)dr["damageTot"];
                            dmgSum += dmg;
                        }
                        var dr2 = (DataSet1.DataTable3Row)dmgDataSet.DataTable3.NewRow();
                        dr2.name = name;
                        dr2.damageTot = dmgSum;
                        dmgDataSet.DataTable3.Rows.Add(dr2);
                    }
                }
            }

            // ダメージソース毎の集計(Table5)
            {
                dmgDataSet.DataTable5.Clear();
                var names = checkedListBox1.CheckedItems;
                foreach (string name in names)
                {
                    var sources = checkedListBox3.CheckedItems;
                    foreach (string source in sources)
                    {
                        string repalcedName = name.Replace("'", "''");
                        var ds1 = dmgDataSet.DataTable1.Select(string.Format("name = '{0}' AND source = '{1}'", repalcedName, source));

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
                            var dr2 = (DataSet1.DataTable5Row)dmgDataSet.DataTable5.NewRow();
                            dr2.name = name;
                            dr2.source = source;
                            dr2.damageTot = dmgSum;
                            dr2.damageMax = dmgMax;
                            dr2.damageMin = dmgMin;
                            dr2.damageAve = (double)dmgSum / (double)count;
                            dr2.count = count;
                            dmgDataSet.DataTable5.Rows.Add(dr2);
                        }
                    }
                }
            }

            comboBox1.SelectedIndex = 0;
            comboBox1_SelectedIndexChanged(null, null);

            this.Cursor = Cursors.Default;
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
                if (damageCountStart && (mes.IndexOf("→") == 0) && (mes.IndexOf("に、") > 0) && (mes.IndexOf("ダメージ。") > 0))
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
                foreach (string item in checkedListBox3.Items)
                {
                    if (item == dr.source)
                    {
                        newName = false;
                    }
                }
                if (newName)
                {
                    checkedListBox3.Items.Add(dr.source, CheckState.Checked);
                }
                checkBoxSource.Checked = true;
            }

            this.Cursor = Cursors.Default;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedItem.ToString())
            {
                case "詳細":
                    dataGridView1.DataSource = dmgDataSet.DataTable2;
                    dataGridView1.Columns["damageAve"].DefaultCellStyle.Format = ".000";
                    break;
                case "Name毎(Totalダメージ)":
                    dataGridView1.DataSource = dmgDataSet.DataTable3;
                    break;
                case "Target毎":
                    dataGridView1.DataSource = dmgDataSet.DataTable4;
                    break;
                case "ダメージソース毎":
                    dataGridView1.DataSource = dmgDataSet.DataTable5;
                    dataGridView1.Columns["damageAve"].DefaultCellStyle.Format = ".000";
                    break;
                default:
                    break;
            }
            dataGridView1.Update();
        }

        private void checkBoxName_CheckedChanged(object sender, EventArgs e)
        {
            var state = CheckState.Checked;
            if (checkBoxName.Checked)
            {
                state = CheckState.Checked;
            }
            else
            {
                state = CheckState.Unchecked;
            }

            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemCheckState(i, state);
            }
        }

        private void checkBoxTarget_CheckedChanged(object sender, EventArgs e)
        {
            var state = CheckState.Checked;
            if (checkBoxTarget.Checked)
            {
                state = CheckState.Checked;
            }
            else
            {
                state = CheckState.Unchecked;
            }

            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                checkedListBox2.SetItemCheckState(i, state);
            }
        }

        private void checkBoxSource_CheckedChanged(object sender, EventArgs e)
        {
            var state = CheckState.Checked;
            if (checkBoxSource.Checked)
            {
                state = CheckState.Checked;
            }
            else
            {
                state = CheckState.Unchecked;
            }

            for (int i = 0; i < checkedListBox3.Items.Count; i++)
            {
                checkedListBox3.SetItemCheckState(i, state);
            }
        }

    }



}

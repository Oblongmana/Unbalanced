using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GenericParsing;

namespace Unbalanced
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static void BuildPaths(List<List<String>> Operations, List<String> pathSoFar, int count, int stop)
        {
            if (count < stop)
            {
                pathSoFar.Add("+");
                BuildPaths(Operations,new List<String>(pathSoFar), count + 1, stop);

                pathSoFar.RemoveAt(count);
                pathSoFar.Add("-");
                BuildPaths(Operations,new List<String>(pathSoFar), count + 1, stop);
            }
            else
            {
                Operations.Add(pathSoFar);
            }
        }

        private static void SumPaths(List<Decimal> DecimalsToSum, List<Decimal> Sums, Decimal sumSoFar, int count, int stop)
        {
            if (count < stop)
            {
                sumSoFar += DecimalsToSum[count];
                SumPaths(DecimalsToSum, Sums, sumSoFar, count + 1, stop);

                sumSoFar -= (DecimalsToSum[count]*2);
                SumPaths(DecimalsToSum, Sums, sumSoFar, count + 1, stop);
            }
            else
            {
                Sums.Add(sumSoFar);
            }
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            DialogResult result = ofd1.ShowDialog();
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            //ArgumentParser argParser = new ArgumentParser(args);

            String dataSource = ofd1.FileName;//argParser["o"];

            if (dataSource != null)
            {
                try
                {
                    GenericParser parser = new GenericParser();
                    parser.SetDataSource(dataSource);
                    parser.FirstRowHasHeader = true;
                    parser.Read();

                    int columnCount = parser.ColumnCount;
                    string[] ItemNames = new string[columnCount];
                    for (int i = 0; i < columnCount; i++)
                    {
                        ItemNames[i] = parser.GetColumnName(i);
                    }
                    List<List<String>> Operations = new List<List<String>>();
                    BuildPaths(Operations, new List<String>(), 0, columnCount);

                    List<Decimal> DecimalsToSum = new List<decimal>();
                    List<Decimal> Sums = new List<decimal>();
                    for (int decimalCounter = 0; decimalCounter < columnCount; decimalCounter++)
                    {
                        DecimalsToSum.Add(Decimal.Parse(parser[decimalCounter]));
                    }
                    SumPaths(DecimalsToSum, Sums, 0, 0, columnCount);

                    String dump = "";
                    int countSuccess = 0;
                    for (int i = 0; i < Operations.Count; i++)
                    {
                        if (Sums[i] == 0)
                        {
                            countSuccess++;
                            int innerCount = Operations[i].Count;
                            //dump += '(';
                            for (int j = 0; j < innerCount; j++)
                            {
                                //dump += ("[" + Operations[i][j] + parser.GetColumnName(j) + ":" + DecimalsToSum[j] + "]");
                                dump += (parser.GetColumnName(j).PadLeft(30) + ":" + DecimalsToSum[j].ToString().PadLeft(15).PadRight(4) + "(" + Operations[i][j] + ")" + Environment.NewLine);
                            }
                            dump += "Balances To".PadLeft(30) + ":" + Sums[i].ToString().PadLeft(15) + Environment.NewLine + Environment.NewLine;
                            //dump += ("    BalancesTo:" + Sums[i] + ')' + Environment.NewLine);
                        }
                    }

                    if (countSuccess > 0) textBox1.Text = dump;
                    else textBox1.Text = "This combination of numbers has no possible way to balance";
                }
                catch (Exception ex)
                {
                    textBox1.Text = ex.Message;
                }

                
            }    
        }

        
    }

    
}

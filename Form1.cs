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

		private static void BuildPathsAndSumsTogether (List<Tuple<Decimal,List<String>>> SumsAndOpps, List<Tuple<Decimal,List<String>>> AbsSumsAndOpps, List<Decimal> DecimalsToSum, List<String> oppsSoFar, Decimal sumSoFar, int count, int stop)
		{
			if (count < stop)
			{
				Decimal amount = DecimalsToSum[count];
				if(amount == 0){
					oppsSoFar.Add("+-");
					sumSoFar += 0;
					BuildPathsAndSumsTogether(SumsAndOpps, AbsSumsAndOpps, DecimalsToSum, new List<String>(oppsSoFar), sumSoFar, count + 1, stop);
				}
				else {
					oppsSoFar.Add("+");
					sumSoFar += amount;
					BuildPathsAndSumsTogether(SumsAndOpps, AbsSumsAndOpps, DecimalsToSum, new List<String>(oppsSoFar), sumSoFar, count + 1, stop);

					oppsSoFar.RemoveAt(count);
					oppsSoFar.Add("-");
					sumSoFar -= (amount*2);
					BuildPathsAndSumsTogether(SumsAndOpps, AbsSumsAndOpps, DecimalsToSum, new List<String>(oppsSoFar), sumSoFar, count + 1, stop);
				}
			}
			else
			{
				SumsAndOpps.Add(new Tuple<decimal, List<string>>(sumSoFar,oppsSoFar));
				AbsSumsAndOpps.Add(new Tuple<decimal, List<string>>(Math.Abs(sumSoFar), oppsSoFar));
			}
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

					//Work out the names of the Accounts we need to sum
                    List<string> ItemNames = new List<string>();
                    for (int i = 0; i < columnCount; i++)
                    {
                        ItemNames.Add (parser.GetColumnName(i));
                    }

					//Work out the numbers we need to sum
					List<Decimal> DecimalsToSum = new List<decimal>();
					for (int decimalCounter = 0; decimalCounter < columnCount; decimalCounter++)
					{
						DecimalsToSum.Add(Decimal.Parse(parser[decimalCounter]));
					}
                    
					List<Tuple<Decimal,List<String>>> SumsAndOpps = new List<Tuple<Decimal,List<String>>>();
					List<Tuple<Decimal,List<String>>> AbsSumsAndOpps = new List<Tuple<Decimal,List<String>>>();
					List<String> OppsSoFar = new List<string>();

					BuildPathsAndSumsTogether (SumsAndOpps, AbsSumsAndOpps, DecimalsToSum, OppsSoFar, 0, 0, columnCount);
					SumsAndOpps.Sort( (a,b) => a.Item1.CompareTo(b.Item1) );
					AbsSumsAndOpps.Sort( (a,b) => a.Item1.CompareTo(b.Item1) );

					int countSuccess = 0;
					textBox1.Text = "";
					foreach(Tuple<Decimal,List<string>> sumWithOpps in SumsAndOpps) {
						if(sumWithOpps.Item1 == 0) {
							countSuccess++;

							for(int account = 0; account < ItemNames.Count; account++) {
								textBox1.Text += (ItemNames[account].PadLeft(30) + ":" 
								                  + DecimalsToSum[account].ToString().PadLeft(15).PadRight(4) 
								                  + "(" + sumWithOpps.Item2[account] + ")" 
								                  + Environment.NewLine);
							}
							textBox1.Text += ("Balances To".PadLeft(30) + ":" 
							                  + sumWithOpps.Item1.ToString().PadLeft(15) 
							                  + Environment.NewLine + Environment.NewLine);
						}
					}

					if(countSuccess == 0) {
						int n = 8;

						textBox1.Text = "This combination of numbers has no possible way to balance. The " + n + " solutions closest to 0 follow." + Environment.NewLine;

						for(int closeResultPos = 0 ; closeResultPos < n; closeResultPos++) {
							Tuple<Decimal,List<string>> sumWithOpps = AbsSumsAndOpps[closeResultPos];
							for(int account = 0; account < ItemNames.Count; account++) {
								textBox1.Text += (ItemNames[account].PadLeft(30) + ":" 
								                  + DecimalsToSum[account].ToString().PadLeft(15).PadRight(4) 
								                  + "(" + sumWithOpps.Item2[account] + ")" 
								                  + Environment.NewLine);
							}
							textBox1.Text += ("Balances To".PadLeft(30) + ":" 
							                  + sumWithOpps.Item1.ToString().PadLeft(15) 
							                  + Environment.NewLine + Environment.NewLine);
						}
					}

					/*List<List<String>> Operations = new List<List<String>>();
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
                            
                            for (int j = 0; j < innerCount; j++)
                            {
                                dump += (parser.GetColumnName(j).PadLeft(30) + ":" + DecimalsToSum[j].ToString().PadLeft(15).PadRight(4) + "(" + Operations[i][j] + ")" + Environment.NewLine);
                            }
                            dump += "Balances To".PadLeft(30) + ":" + Sums[i].ToString().PadLeft(15) + Environment.NewLine + Environment.NewLine;
                        }
                    }

                    if (countSuccess > 0) textBox1.Text = dump;
                    else {
						int n = 4;

						textBox1.Text = "This combination of numbers has no possible way to balance. The " + n + " solutions closest to 0 follow." + Environment.NewLine;


						int actualFoundCount = 0;
						List<Decimal> lowestNSums = new List<decimal>();
						List<Decimal> sumsCpy = new List<decimal>(Sums);

						SortedDictionary<Decimal,List<int>> absSumToPositions = new SortedDictionary<decimal, List<int>>();
						for(int position = 0 ; position < Sums.Count; position++) {
							List<int> value = new List<int>();
							Decimal absValue = Math.Abs(Sums[position]);
							if(absSumToPositions.TryGetValue(absValue, out value)) {
								value.Add(position);
							}
							else {
								absSumToPositions.Add(absValue,new List<int>(position));
							}
						}

						List<int> positionOfOutputItems = new List<int>();
						int countItemsOutput = 0;
						foreach(KeyValuePair<Decimal, List<int>> item in absSumToPositions)
						{
							foreach(int pos in item.Value) {
								if(countItemsOutput >= n) break;
								positionOfOutputItems.Add(pos);
								countItemsOutput++;
							}
							if(countItemsOutput >= n) break;
						}

						foreach(int posToOutput in positionOfOutputItems) {
							for (int j = 0; j < Operations[posToOutput].Count; j++)
							{
								textBox1.Text += (parser.GetColumnName(j).PadLeft(30) + ":" + DecimalsToSum[j].ToString().PadLeft(15).PadRight(4) + "(" + Operations[posToOutput][j] + ")" + Environment.NewLine);
							}
							textBox1.Text += "Balances To".PadLeft(30) + ":" + Sums[posToOutput].ToString().PadLeft(15) + Environment.NewLine + Environment.NewLine;
						}

					}*/
                }
                catch (Exception ex)
                {
                    textBox1.Text = ex.Message;
                }

                
            }    
        }

        
    }

    
}

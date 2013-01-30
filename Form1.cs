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

		/// <summary>
		/// Recursively run through every single way to add/subtract the numbers together, producing a List describing the different
		/// possible balances, and the operations conducted to get said balance. Brute-force, with the sole enhancement being that if 
		/// one of the numbers is Zero, then instead of branching down both the Add and Subtract paths, will go down a single 
		/// "Plus-Minus" path, bringing this algorithm to 2^(n - [Count of Accounts with Zero balance])
		/// 
		/// </summary>
		/// <param name="SumsAndOps">List of Tuples, where each Tuple represents a Sum and the Operations conducted to obtain the sum.</param>
		/// <param name="AbsSumsAndOps">As for SumsAndOps, but the Tuple has an additional item (item 1) which is the absolute value of the Sum</param>
		/// <param name="DecimalsToSum">The numbers we are attempting to balance to Zero.</param>
		/// <param name="oppsSoFar">The operations conducted so far.</param>
		/// <param name="sumSoFar">The Sum of all operations so far.</param>
		/// <param name="count">How many of the numbers we're attempting to balance that we've got through so far.</param>
		/// <param name="stop">At what point the recursive method stops (this should be set to the sidze of the DecimalsToSum list).</param>
		private static void BuildPathsAndSumsTogether (List<Tuple<Decimal,List<String>>> SumsAndOps, List<Tuple<Decimal,Decimal,List<String>>> AbsSumsAndOps, List<Decimal> DecimalsToSum, List<String> oppsSoFar, Decimal sumSoFar, int count, int stop)
		{
			if (count < stop)
			{ 
				Decimal amount = DecimalsToSum[count];
				if(amount == 0){
					oppsSoFar.Add("+-");
					sumSoFar += 0;
					BuildPathsAndSumsTogether(SumsAndOps, AbsSumsAndOps, DecimalsToSum, new List<String>(oppsSoFar), sumSoFar, count + 1, stop);
				}
				else {
					oppsSoFar.Add("+");
					sumSoFar += amount;
					BuildPathsAndSumsTogether(SumsAndOps, AbsSumsAndOps, DecimalsToSum, new List<String>(oppsSoFar), sumSoFar, count + 1, stop);
					
					oppsSoFar.RemoveAt(count);
					oppsSoFar.Add("-");
					sumSoFar -= (amount*2);
					BuildPathsAndSumsTogether(SumsAndOps, AbsSumsAndOps, DecimalsToSum, new List<String>(oppsSoFar), sumSoFar, count + 1, stop);
				}
			}
			else
			{
				SumsAndOps.Add(new Tuple<decimal, List<string>>(sumSoFar,oppsSoFar));
				AbsSumsAndOps.Add(new Tuple<decimal, decimal, List<string>>(Math.Abs(sumSoFar), sumSoFar, oppsSoFar));
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
					
					List<Tuple<Decimal,List<String>>> SumsAndOps = new List<Tuple<Decimal,List<String>>>();
					List<Tuple<Decimal,Decimal,List<String>>> AbsSumsAndOps = new List<Tuple<Decimal,Decimal,List<String>>>();
					List<String> OpsSoFar = new List<string>();
					
					BuildPathsAndSumsTogether (SumsAndOps, AbsSumsAndOps, DecimalsToSum, OpsSoFar, 0, 0, columnCount);
					SumsAndOps.Sort( (a,b) => a.Item1.CompareTo(b.Item1) );
					AbsSumsAndOps.Sort( (a,b) => a.Item1.CompareTo(b.Item1) );
					
					int countSuccess = 0;
					textBox1.Text = "";
					foreach(Tuple<Decimal,List<string>> sumWithOps in SumsAndOps) {
						if(sumWithOps.Item1 == 0) {
							countSuccess++;
							
							for(int account = 0; account < ItemNames.Count; account++) {
								textBox1.Text += (ItemNames[account].PadLeft(30) + ":" 
								                  + DecimalsToSum[account].ToString().PadLeft(15).PadRight(4) 
								                  + "(" + sumWithOps.Item2[account] + ")" 
								                  + Environment.NewLine);
								Console.WriteLine(ItemNames[account]+":"+DecimalsToSum[account]+":"+sumWithOps.Item2[account]);
							}
							textBox1.Text += ("Balances To".PadLeft(30) + ":" 
							                  + sumWithOps.Item1.ToString().PadLeft(15) 
							                  + Environment.NewLine + Environment.NewLine);
							Console.WriteLine("Balances To" +":"+ sumWithOps.Item1);
							Console.WriteLine();
						}
					}
					
					if(countSuccess == 0) {
						int n = 100;
						
						textBox1.Text = "This combination of numbers has no possible way to balance. The " + n + " solutions closest to 0 follow." + Environment.NewLine;
						
						for(int closeResultPos = 0 ; closeResultPos < n || closeResultPos >= AbsSumsAndOps.Count; closeResultPos++) {
							Tuple<Decimal,Decimal,List<string>> sumWithOps = AbsSumsAndOps[closeResultPos];
							for(int account = 0; account < ItemNames.Count; account++) {
								textBox1.Text += (ItemNames[account].PadLeft(30) + ":" 
								                  + DecimalsToSum[account].ToString().PadLeft(15).PadRight(4) 
								                  + "(" + sumWithOps.Item3[account] + ")" 
								                  + Environment.NewLine);
								Console.WriteLine(ItemNames[account]+":"+DecimalsToSum[account]+":"+sumWithOps.Item3[account]);
							}

							textBox1.Text += ("Balances To".PadLeft(30) + ":" 
							                  + sumWithOps.Item2.ToString().PadLeft(15) 
							                  + Environment.NewLine + Environment.NewLine);
							Console.WriteLine("Balances To" +":"+ sumWithOps.Item2);
							Console.WriteLine();
						}
					}
				}
				catch (Exception ex)
				{
					textBox1.Text = ex.Message;
				}
				
				
			}    
		}
		
		
	}
	
	
}

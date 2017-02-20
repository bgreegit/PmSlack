using System;
using System.Threading;

namespace PmSlack
{
	class Program
	{
		static void Main(string[] args)
		{
			PmProcessChecker checker = new PmProcessChecker();
			Timer t = new Timer(TimerCallback, checker, 0, 1000);
			Console.WriteLine("Type any key to stop");
			Console.ReadLine();
			t.Dispose();
		}

		private static void TimerCallback(object o)
		{
			PmProcessChecker checker = o as PmProcessChecker;

			try
			{
				lock (checker)
				{
					checker.Check();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Unknown exception e={0}", e.ToString());
			}
		}
	}
}

using System;

namespace TestCoturnClients
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.ResetColor();
			Console.WriteLine("Begin Test");

			TestCoturnClient testCoturnClient = new TestCoturnClient();
			//testCoturnClient.Execute();
			testCoturnClient.Execute1();
		}
	}
}

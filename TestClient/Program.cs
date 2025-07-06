using System;
using System.IO;
using System.Threading;

namespace IziHardGames
{
	/// <summary>
	/// Client
	/// <cref="https://www.nuget.org/packages/PreEmptive.Protection.Checks.Attributes/2.0.0"/> ??? обфускатор
	/// <cref="http://www.columbia.edu/~fdc/sample.html"/>
	/// В данный момент используется <see cref="Proxy.PassThroughProxy"/> сервер. в <see cref="ProxyServerAdapted"/> <cref="https://www.nuget.org/packages/PassThroughProxy/"/>
	/// В браузере нужно указать айпи на котором запускается данное приложение и порт из config.json $.server.port
	/// Ауетентификация будет запрошена в браузере. Данные для аутентификации config.json $.authentication
	/// Текущие задачи: перехват трафика c <see cref="Proxy.PassThroughProxy"/> для перенаправления на cotrun сервер
	/// </summary>
	public class Program
	{
		public const string configFileName = "config.json";

		static void Main(string[] args)
		{
			Console.WriteLine($"Program started!");
			//var hash = StunHeader.GenerateHashKey("user", "realm", "pass");
			//Console.WriteLine(Encoding.UTF8.GetString(hash));
			//Console.WriteLine(BitConverter.ToString(hash));

			//RestrictMultipleInstanceRun();
#if DEBUG
			for (int i = 0; i < args.Length; i++)
			{
				Console.WriteLine($"ARG {i} var [{args[i]}]");
			}
#endif
			IziHardGames.ConfigurationTools.Configuration.ImportConfig(Path.Combine(Directory.GetCurrentDirectory(), configFileName));

			ManagerMainTurnClient managerMain = new ManagerMainTurnClient();
			managerMain.Execute(args);

			ConsoleWrap.Green($"END MAIN");
			Console.ReadLine();
		}
		/// <summary>
		/// Ограничение запуска нескольких версий
		/// </summary>
		public static void RestrictMultipleInstanceRun()
		{
			if (!CheckSingleInstance())
			{
#if DEBUG
				Console.WriteLine("Один экземпляр приложения уже запущен");
				throw new Exception();
#else
				Environment.Exit(0);
#endif
			}
		}
		public static bool CheckSingleInstance()
		{
			string m_appName = "TurnClient";
			Mutex mutex = new Mutex(true, m_appName);
			return mutex.WaitOne(TimeSpan.Zero, true);
		}
	}
}

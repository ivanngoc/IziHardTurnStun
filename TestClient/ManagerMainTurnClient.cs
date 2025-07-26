using Microsoft.Extensions.Configuration;
using IziHardGames.STUN;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.STUN.STUN;

namespace IziHardGames
{
	/// <summary>
	/// ??? Что делает???
	/// Пока по прикидке
	/// </summary>
	class ManagerMainTurnClient
	{
		//private TrafficCounter trafficCounter = new TrafficCounter();
		//private ProxyServerAdapted proxyServerAdapted = new ProxyServerAdapted();
		//private ManagerSessionContext managerSessionContext = new ManagerSessionContext();
		private CoturnRestApi coturnRestApi;
		private IConfiguration configuration;
		private StunConnectionConfig coturnConnection = new StunConnectionConfig();

		private void Initilize()
		{
			//proxyServerAdapted.managerSessionContext = managerSessionContext;
			//trafficCounter.managerSessionContext = managerSessionContext;

			ConfigurationBuilder builder = new ConfigurationBuilder();
			builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
			configuration = builder.Build();
			coturnRestApi = new CoturnRestApi(configuration);

			//configuration.GetSection("CoturnConnections:Softorium").Bind(coturnConnection);
			//configuration.GetSection("CoturnConnections:VirtualMachine").Bind(coturnConnection);
			configuration.GetSection("CoturnConnections:VirtualBox").Bind(coturnConnection);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args">
		/// 0 - turnIP<br/>
		/// 1 - turnPort<br/>
		/// 2 - browserIp он же peerIp<br/>
		/// Debug Application Arguments 
		/// 34.122.44.19 3478 109.254.3.161
		/// На момент старта разработки
		/// </param>
		public void Execute(string[] args)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}	{method.DeclaringType.FullName}.{method.Name}() thread	{Thread.CurrentThread.ManagedThreadId} PROGRAM STARTED");

			Initilize();

			while (true)
			{

			};

			string turnIP, turnPort, peerIp;

			if (args.Length != 3)
			{
				Console.WriteLine("Аргументы должны быть следующими: IP turn сервера, порт turn сервера, ip компьютера c браузером.");
				return;
			}
			else
			{
				turnIP = args[0];
				turnPort = args[1];
				peerIp = args[2];
			}
		
			//Task serverRunning = Task.Factory.StartNew(() => proxyServerAdapted.Start());

		
			Console.WriteLine("Supress loop");

			//trafficCounter.dateTimeLastRefresh = DateTime.Now;

			//while (true)
			//{
			//	//DateTime start = DateTime.Now;
			//	//Console.WriteLine($"Start {start}");
			//	if (serverRunning.IsCompleted)
			//	{
			//		throw new Exception($"Server task is Completed. Must be running");
			//	}
			//	if (serverRunning.Exception != null)
			//	{
			//		throw serverRunning.Exception;
			//	}
			//	trafficCounter.Display();
			//	//Console.WriteLine((DateTime.Now - start).TotalMilliseconds + $" {Thread.CurrentThread.ManagedThreadId}");
			//}
		}	
		
	}
}

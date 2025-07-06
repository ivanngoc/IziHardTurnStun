using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace IziHardGames
{
	public static class LogTool
	{
		public static string pathToFile;
		public static string dir;

		public static bool isLogInfo = true;
		public static bool isLogWarning = true;
		public static bool isLogError = true;
		public static bool isLogRequest = true;
		public static bool isMethodTrace = true;
		public static bool isDuplicateToConsole = true;

		public static void Init()
		{
			pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "log.txt");
			dir = Path.GetDirectoryName(pathToFile);

			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			if (!File.Exists(pathToFile))
			{
				File.CreateText(pathToFile).Close();
			}
		}

		public static void LogHTTPRequest(System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> response, MethodInfo methodInfo = null)
		{
			if (isLogRequest && isLogInfo)
			{
				string message = $"STATUS {response.Result.StatusCode.ToString()} RequestMessage {response.Result.RequestMessage}";
				File.AppendAllText(pathToFile, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}	INFO	{GetMethodInfoAsString(methodInfo)}{message}{Environment.NewLine}");

				if (isDuplicateToConsole)
				{
					Console.WriteLine(message);
				}
			}
		}
		public static void LogMessage(string message, MethodInfo methodInfo = null)
		{
			if (isLogInfo)
			{
				string toWrite = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}	{GetMethodInfoAsString(methodInfo)} {message}{Environment.NewLine}";
				File.AppendAllText(pathToFile, toWrite);

				if (isDuplicateToConsole)
				{
					Console.WriteLine(toWrite);
				}
			}
		}
		public static void LogException(Exception exception, MethodInfo methodInfo = null)
		{
			string message = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}	{GetMethodInfoAsString(methodInfo)}{exception}{Environment.NewLine}";
			Console.WriteLine(message);
			File.AppendAllText(pathToFile, message);

			if (isDuplicateToConsole)
			{
				Console.WriteLine(message);
			}
		}
		public static void LogException(Exception exception, string message, MethodInfo methodInfo = null)
		{
			string toWrite = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}	{GetMethodInfoAsString(methodInfo)} {message}	{exception}{Environment.NewLine}";
			Console.WriteLine(toWrite);
			File.AppendAllText(pathToFile, toWrite);

			if (isDuplicateToConsole)
			{
				Console.WriteLine(toWrite);
			}
		}

		public static string GetMethodInfoAsString(MethodInfo methodInfo)
		{
			if (methodInfo != null)
			{
				return $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}() ";
			}
			else
			{
				return string.Empty;
			}
		}


		public static void SetLogLevel(ELogLevel eLogCategory)
		{
			if (eLogCategory.HasFlag(ELogLevel.All))
			{
				LogsInfoEnable();
				isLogWarning = true;
				isLogError = true;
				isLogRequest = true;
				return;
			}
			if (eLogCategory.HasFlag(ELogLevel.None))
			{
				return;
			}
			if (eLogCategory.HasFlag(ELogLevel.INFO))
			{
				LogsInfoEnable();
			}
			if (eLogCategory.HasFlag(ELogLevel.WARNING))
			{
				isLogWarning = true;
			}
			if (eLogCategory.HasFlag(ELogLevel.ERROR))
			{
				isLogError = true;
			}
			if (eLogCategory.HasFlag(ELogLevel.REQUEST))
			{
				isLogRequest = true;
			}
			if (eLogCategory.HasFlag(ELogLevel.METHOD_TRACE))
			{
				isMethodTrace = true;
			}
		}
		public static void LogsEnable()
		{
			SetLogLevel(ELogLevel.All);
		}
		public static void LogsDisable()
		{
			SetLogLevel(ELogLevel.None);
		}
		public static void LogsInfoEnable()
		{
			isLogInfo = true;
		}
		public static void LogsInfoDisable()
		{
			isLogInfo = false;
		}

		[Flags]
		public enum ELogLevel
		{
			All = -1,
			None = 0,
			INFO = 1,
			WARNING = 2,
			ERROR = 4,
			REQUEST = 8,
			METHOD_TRACE = 16,
		}
	}
}


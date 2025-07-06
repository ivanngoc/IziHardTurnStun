using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace IziHardGames.ConfigurationTools
{
	public class Configuration
	{
		public const string configPath = "config/appsettings.json";
		public static IConfiguration configurationShared;
		public static void TryImportConfig()
		{
			try
			{
				ImportConfig();
			}
			catch (Exception ex)
			{
				string message = $"Не удалось получить файл конфигурации. Проверьте файл конфигурации {Path.Combine(Directory.GetCurrentDirectory(), configPath)} {ex.Message}";
				Console.WriteLine(message);
			}
		}
		public static void ImportConfig(string pathAbs)
		{
			ImportConfig(Path.GetDirectoryName(pathAbs), Path.GetFileName(pathAbs));
		}
		public static void ImportConfig()
		{
			string curDir = Directory.GetCurrentDirectory();
			string pathAbs = Path.Combine(curDir, configPath);

			ImportConfig(Path.GetDirectoryName(pathAbs), Path.GetFileName(pathAbs));
		}
		public static void ImportConfig(string basePath, string fileName)
		{
			ConfigurationBuilder builder = new ConfigurationBuilder();
			builder.SetBasePath(basePath);
			builder.AddJsonFile(fileName);
			// создаем конфигурацию
			configurationShared = builder.Build();
		}
	}

	public static class IIConfigurationSectionExtensions
	{
		public static int GetInt(this IConfigurationSection section)
		{
			return int.Parse(section.Value);
		}
	}
}
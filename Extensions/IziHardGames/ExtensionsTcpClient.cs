using System;
using System.Net;
using System.Text;

namespace System
{
	public static class ExtensionsString
	{
		public static byte[] GetBytes(this string str)
		{
			return Encoding.UTF8.GetBytes(str);
		}
	}
	public static class ExtensionsArray
	{
		public static string ToStringUtf8(this byte[] array)
		{
			return Encoding.UTF8.GetString(array);
		}
	}
	public static class ExtensionsDouble
	{
		public static double KibiByteToMebiByte(this double kibiByte)
		{
			return kibiByte / 1024.0;
		}
	}
}

namespace System.Net.Sockets
{
	public static class ExtensionsTcpClient
	{
		/// <summary>
		/// Очень медленный
		/// </summary>
		/// <param name="tcpClient"></param>
		/// <returns></returns>
		public static string GetHostName(this TcpClient tcpClient)
		{
			try
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(tcpClient.AsIPEndPointRemote().Address);
				return hostEntry.HostName?.Trim() ?? "NoHostEntry";
			}
			catch (Exception)
			{
				return $"No such host is known";
			}
		}
		public static IPEndPoint AsIPEndPointLocal(this TcpClient tcpClient)
		{
			return (IPEndPoint)tcpClient.Client.LocalEndPoint;
		}
		public static IPEndPoint AsIPEndPointRemote(this TcpClient tcpClient)
		{
			return (IPEndPoint)tcpClient.Client.RemoteEndPoint;
		}
		public static string GetAdressRemoteAsString(this TcpClient tcpClient)
		{
			return tcpClient?.AsIPEndPointRemote().Address.ToString() ?? "null tcpClient";
		}
		public static string GetAdressLocalAsString(this TcpClient tcpClient)
		{
			return tcpClient.AsIPEndPointLocal().Address.ToString();
		}
	}
}

using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames
{

	public class CoturnRestApi
	{
		public IConfiguration configuration;
		public string adressTurnServer;
		/// <summary>
		/// doc: an optional user id to be associated with the credentials<br/>
		/// выбирается произвольно. По нему будут созданы данные для аутентификации на сервере
		/// </summary>
		public string username;
		public string password;
		public string portTurnAsString;
		public int portTurn;
		private Uri uri;

		public CoturnRestApi(IConfiguration configuration)
		{
			this.configuration = configuration;

			adressTurnServer = configuration.GetSection("coturn:adress").Value;
			portTurnAsString = configuration.GetSection("coturn:port_turn").Value;
			username = configuration.GetSection("coturn:username").Value;

			password = configuration.GetSection("authentication:password").Value;

			uri = new UriBuilder(adressTurnServer + ":" + portTurnAsString).Uri;
			//uri = new UriBuilder(adressTurnServer + ":80").Uri;
			//uri = new UriBuilder(adressTurnServer).Uri;
#if DEBUG
			//TestRequest();
			//RequestAuthentication();
#endif
		}
#if DEBUG
		public void TestTCPConnect()
		{
			Task.Factory.StartNew(() =>
			{
				TcpClient tcpClient = new TcpClient();
				IPAddress iPAddress = IPAddress.Parse(adressTurnServer);
				IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, portTurn);
				tcpClient.Connect(iPEndPoint);
				var stream = tcpClient.GetStream();
				var rawAuth = GenerateTurnPassword("randomusername");
				//stream.Write(rawAuth, 0, rawAuth.Length);
			});
		}
		public void TestRequest()
		{
			var handler = new HttpClientHandler();

			using (var httpClient = new HttpClient(handler))
			{
				//httpClient.BaseAddress = uri;

				using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
				{
					request.Content = new StringContent($"service=turn&username={username}");
					Console.WriteLine($"Send test request to {uri.ToString()}");
					var task = httpClient.SendAsync(request);
					var response = task.Result;
				}
			}
		}
#endif
		/// <summary>
		/// To retrieve a new set of credentials, the client makes a HTTP GET
		/// request, specifying TURN as the service to allocate credentials for,
		/// and optionally specifying a user id parameter.The purpose of the
		///	user id parameter is to simplify debugging on the TURN server, as
		///	well as provide the ability to control the number of credentials
		/// handed out for a specific user, if desired.The TURN credentials and
		///	their lifetime are returned as JSON, along with URIs that indicate
		/// how to connect to the server using the TURN protocol.<br/>
		/// <cref="https://datatracker.ietf.org/doc/html/draft-uberti-behave-turn-rest-00#section-2"/>
		/// </summary>
		public void RequestAuthentication()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			Console.WriteLine($"{DateTime.Now}	{method.DeclaringType.FullName}.{method.Name}() username {username} uri {uri}");

			var handler = new HttpClientHandler();

			using (var httpClient = new HttpClient(handler))
			{
				//httpClient.BaseAddress = uri;

				using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
				{
					request.Content = new StringContent($"service=turn&username={username}");

					Console.WriteLine($"Send test request to {uri.ToString()}");
					var task = httpClient.SendAsync(request);
					var response = task.Result;
				}
			}
		}
		/// <summary>
		/// The WebRTC client will perform a standard TURN allocation sequence
		/// using the long-term credentials mechanism specified in [RFC5389],
		/// Section 10.2, using the "username" value from the returned JSON for
		/// its USERNAME attribute, and the "password" value for the password
		/// input to the MESSAGE-INTEGRITY hash.<br/>
		/// <cref="https://datatracker.ietf.org/doc/html/draft-uberti-behave-turn-rest-00#section-4.1"/>
		/// </summary>
		private void AllocateLongTermCredentials()
		{
			throw new NotImplementedException();
		}

		private void CreateRequestAuthenticationData()
		{
			long time = ((DateTimeOffset)(DateTime.Now.AddDays(1))).ToUnixTimeSeconds();
		}
        /// <summary>
        /// https://stackoverflow.com/questions/35766382/coturn-how-to-use-turn-rest-api
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string[] GenerateTurnPassword(string username)
		{
			long ttl = 3600 * 6;
			var time = DateTimeOffset.Now.ToUnixTimeSeconds() + ttl;
			var newuser = time + ":" + username;
			byte[] key = Encoding.UTF8.GetBytes("north");
			HMACSHA1 hmacsha1 = new HMACSHA1(key);
			byte[] buffer = Encoding.UTF8.GetBytes(newuser);
			MemoryStream stream = new MemoryStream(buffer);
			var hashValue = hmacsha1.ComputeHash(stream);
			string[] arr = new string[2];
			arr[0] = Convert.ToBase64String(hashValue);
			arr[1] = newuser;
			return arr;
		}
	}
}

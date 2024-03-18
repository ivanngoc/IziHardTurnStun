using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Networking.IANA
{

	/// <summary>
	/// https://www.ietf.org/archive/id/draft-cotton-tsvwg-iana-ports-00.html#:~:text=4.4.&text=The%20Dynamic%20Ports%20range%20from,or%20by%20any%20other%20means.
	/// https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml
	/// https://www.rfc-editor.org/rfc/rfc6335.html
	/// Windows Check Port CLI
	/// netstat -anp | find "port number"
	/// netstat -anp | find "8080"
	/// разные команды
	/// netstat -p TCP	- показать установленные TCP соединения
	/// </summary>
	public class Ports
	{
		public int WellKnownRangeMin = 0;
		public int WellKnownRangeMax = 1023;

		public int RigisteredRangeMin = 1024;
		public int RigisteredRangeMax = 49151;

		public int PrivateRangeMin = 49152;
		public int PrivateRangeMax = 65535;

	}
	/// <summary>
	/// https://www.iana.org/assignments/protocol-numbers/protocol-numbers.xhtml
	/// </summary>
	public class ProtocolNumber
	{
		public const byte HOPOPT = 0;
		public const byte TCP = 6;
		public const byte UDP = 17;

		public readonly static Dictionary<int, EProtocolNumber> protocolTypeByValue;

		static ProtocolNumber()
		{
			protocolTypeByValue = new Dictionary<int, EProtocolNumber>()
			{
				[HOPOPT] = EProtocolNumber.HOPOPT,
				[TCP] = EProtocolNumber.TCP,
				[UDP] = EProtocolNumber.UDP,
			};
		}

		public static EProtocolNumber GetProtocol(byte value)
		{
			return protocolTypeByValue[value];
		}
	}

	public enum EProtocolNumber : byte
	{
		HOPOPT = ProtocolNumber.HOPOPT,
		TCP = ProtocolNumber.TCP,
		UDP = ProtocolNumber.UDP,
	}
}

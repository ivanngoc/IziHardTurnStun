using System.Buffers;

namespace System
{
	public static class ExtensionsArrayByteToBinaryString
	{
		public static string ToStringBinaryPerCharBitStreamLeftToRight(this byte[] val)
		{
			int size = val.Length;
			int countBits = size * 8;
			var array = ArrayPool<char>.Shared.Rent(countBits);
			Span<char> span = new Span<char>(array, 0, countBits);
			int offset = default;

			for (int i = 0; i < size; i++)
			{
				int b = val[i];

				for (int j = 0; j < 8; j++)
				{
					// case odd
					if ((b & 0b1000_0000) > 0)
					{
						span[offset] = '1';
					}
					else
					{
						span[offset] = '0';
					}
					b = b << 1;
					offset++;
				}
			}
			string s = new string(span);
			ArrayPool<char>.Shared.Return(array);
			return s;
		}
		/// <param name="val"></param>
		/// <returns></returns>
		public static string ToStringBinaryPerCharForHuman(this byte[] val)
		{
			int size = val.Length;
			int countBits = size * 8;
			var array = ArrayPool<char>.Shared.Rent(countBits);
			Span<char> span = new Span<char>(array, 0, countBits);

			int offset = countBits - 8;
			for (int i = 0; i < size; i++)
			{
				int b = val[i];

				for (int j = 0; j < 8; j++)
				{
					// case odd
					if ((b & 0b1000_0000) > 0)
					{
						span[offset] = '1';
					}
					else
					{
						span[offset] = '0';
					}
					b = b << 1;
					offset++;
				}
				// return to 8 chars writed and before next 8 chars
				offset -= 16;
			}
			string s = new string(span);
			ArrayPool<char>.Shared.Return(array);
			return s;
		}

		/// <param name="val"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string ToStringBinaryPerCharForHumanSeparateByte(this byte[] val, char separator)
		{
			int size = val.Length;
			int countBits = size * 8;
			int countChars = size * 8 + size - 1;

			var array = ArrayPool<char>.Shared.Rent(countChars);
			Span<char> span = new Span<char>(array, 0, countChars);

			int offset = countChars - 8;
			int iLast = size - 1;

			for (int i = 0; i < size; i++)
			{
				int b = val[i];

				for (int j = 0; j < 8; j++)
				{
					// case odd
					if ((b & 0b1000_0000) > 0)
					{
						span[offset] = '1';
					}
					else
					{
						span[offset] = '0';
					}
					b = b << 1;
					offset++;
				}
				if (i < iLast)
				{
					span[offset - 9] = separator;
					offset -= 17;
				}
			}
			string s = new string(span);
			ArrayPool<char>.Shared.Return(array);
			return s;
		}
		/// <param name="val"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string ToStringBinaryPerCharForHumanSeparateQuad(this byte[] val, char separator)
		{
			int size = val.Length;
			int countBits = size * 8;
			int countChars = size * 8 + (size * 2) - 1;

			var array = ArrayPool<char>.Shared.Rent(countChars);
			Span<char> span = new Span<char>(array, 0, countChars);

			int offset = countChars - 9;
			int iLast = size - 1;

			for (int i = 0; i < size; i++)
			{
				int b = val[i];

				for (int j = 0; j < 4; j++)
				{
					// case odd
					if ((b & 0b1000_0000) > 0)
					{
						span[offset] = '1';
					}
					else
					{
						span[offset] = '0';
					}
					b = b << 1;
					offset++;
				}

				span[offset] = separator;
				offset++;

				for (int j = 0; j < 4; j++)
				{
					// case odd
					if ((b & 0b1000_0000) > 0)
					{
						span[offset] = '1';
					}
					else
					{
						span[offset] = '0';
					}
					b = b << 1;
					offset++;
				}

				if (i < iLast)
				{
					span[offset - 10] = separator;
					offset -= 19;
					// return to 8 chars writed and before next 8 chars + separators
				}
			}
			string s = new string(span);
			ArrayPool<char>.Shared.Return(array);
			return s;
		}



	}
}
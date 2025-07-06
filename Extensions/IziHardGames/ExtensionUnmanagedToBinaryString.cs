using System.Buffers;

namespace System.BinaryString
{
	public static class ExtensionUnmanagedToBinaryString
	{
		#region Represent Binary "As Is" in Memory
		/// <summary>
		/// like array of bytes read as array of bits. If it was possible to read bit stream from left to right.<br/>
		/// Or like pointer move per bit in memory.<br/>
		/// Big Endian <br/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val"></param>
		/// <returns></returns>
		public static unsafe string ToStringBinaryPerCharBitStreamLeftToRight<T>(this T val) where T : unmanaged
		{
			int size = sizeof(T);
			int countBits = size * 8;
			var array = ArrayPool<char>.Shared.Rent(countBits);
			Span<char> span = new Span<char>(array, 0, countBits);
			byte* ptr = (byte*)&val;
			int offset = default;
			for (int i = 0; i < size; i++)
			{
				int b = *ptr;
				ptr++;

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
		/// <typeparam name="T"></typeparam>
		/// <param name="val"></param>
		/// <returns></returns>
		public static unsafe string ToStringBinaryPerCharBitStreamRightToLeft<T>(this T val) where T : unmanaged
		{
			int size = sizeof(T);
			int countBits = size * 8;
			var array = ArrayPool<char>.Shared.Rent(countBits);
			Span<char> span = new Span<char>(array, 0, countBits);
			byte* ptr = (byte*)&val;
			int offset = countBits;
			for (int i = 0; i < size; i++)
			{
				int b = *ptr;
				ptr++;

				for (int j = 0; j < 8; j++)
				{
					offset--;
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
				}
			}
			string s = new string(span);
			ArrayPool<char>.Shared.Return(array);
			return s;
		}
		#endregion

		#region Human Readable
		public static unsafe string ToStringBinaryPerCharForHuman<T>(this T val) where T : unmanaged
		{
			int size = sizeof(T);
			int countBits = size * 8;
			var array = ArrayPool<char>.Shared.Rent(countBits);
			Span<char> span = new Span<char>(array, 0, countBits);
			byte* ptr = (byte*)&val;
			int offset = countBits - 8;
			for (int i = 0; i < size; i++)
			{
				int b = *ptr;
				ptr++;
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
		public static unsafe string ToStringBinaryPerCharForHumanSeparateByte<T>(this T val, char separator) where T : unmanaged
		{
			int size = sizeof(T);
			int countBits = size * 8;
			int countChars = size * 8 + size - 1;

			var array = ArrayPool<char>.Shared.Rent(countChars);
			Span<char> span = new Span<char>(array, 0, countChars);

			byte* ptr = (byte*)&val;
			int offset = countChars - 8;
			int iLast = size - 1;

			for (int i = 0; i < size; i++)
			{
				int b = *ptr;
				ptr++;
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
		public static unsafe string ToStringBinaryPerCharForHumanSeparateQuad<T>(this T val, char separator) where T : unmanaged
		{
			int size = sizeof(T);
			int countBits = size * 8;
			int countChars = size * 8 + (size * 2) - 1;

			var array = ArrayPool<char>.Shared.Rent(countChars);
			Span<char> span = new Span<char>(array, 0, countChars);

			byte* ptr = (byte*)&val;
			int offset = countChars - 9;
			int iLast = size - 1;

			for (int i = 0; i < size; i++)
			{
				int b = *ptr;
				ptr++;

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
		/// <summary>
		/// For UInt16<br/>
		/// "0011101000001011" = 53340 Result  <br/>
		/// "1101000001011100" = 53340 Original Human readable<br/>
		/// Per Bit Mirror
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val"></param>
		/// <returns></returns>
		public static unsafe string ToStringBinaryPerCharForHumanMirror<T>(this T val) where T : unmanaged
		{
			int size = sizeof(T);
			int countBits = size * 8;
			var array = ArrayPool<char>.Shared.Rent(countBits);
			Span<char> span = new Span<char>(array, 0, countBits);
			byte* ptr = (byte*)&val;
			int offset = default;
			for (int i = 0; i < size; i++)
			{
				int b = *ptr;
				ptr++;

				for (int j = 0; j < 8; j++)
				{
					// case odd
					if ((b & 0b1) > 0)
					{
						span[offset] = '1';
					}
					else
					{
						span[offset] = '0';
					}
					b = b >> 1;
					offset++;
				}
			}
			string s = new string(span);
			ArrayPool<char>.Shared.Return(array);
			return s;
		}
		#endregion

#if BINARY_STRING_PRESET
		public static string ToStringBinaryRightToLeft<T>(this T val) where T : unmanaged
		{
			throw new NotImplementedException();
		}
#endif
	}
}
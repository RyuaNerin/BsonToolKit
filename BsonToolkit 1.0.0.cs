//////////////////////////////////////////////////////////////////////////
// DSON Toolkit 1.0.0
// Released: 2014.07.31
// http://github.com/RyuaNerin/DsonTookit
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ComputerBeacon.Json;

namespace ComputerBeacon.Bson
{
	public static class BsonExtension
	{
		public static byte[] ToBytes(this JsonObject jo)
		{
			byte[] arr;
			using (MemoryStream stream = new MemoryStream())
			{
				Byteifier.byteify(jo, stream);
				arr = stream.ToArray();
				stream.Dispose();
			}

			return arr;
		}
		public static byte[] ToBytes(this JsonArray ja)
		{
			byte[] arr;
			using (MemoryStream stream = new MemoryStream())
			{
				Byteifier.byteify(ja, stream);
				arr = stream.ToArray();
				stream.Dispose();
			}

			return arr;
		}
		public static void BsonWrite(this JsonObject jo, Stream outStream)
		{
			Byteifier.byteify(jo, outStream);
		}
		public static void BsonWrite(this JsonArray ja, Stream outStream)
		{
			Byteifier.byteify(ja, outStream);
		}
	}

	public static class BsonParser
	{
		public static JsonArray ParseArray(byte[] array)
		{
			JsonArray ja = BsonParser.Parse(array, true) as JsonArray;
			if (ja == null) throw new FormatException("BsonData represents JsonObject instead of JsonArray");
			return ja;
		}
		public static JsonArray ParseArray(Stream stream)
		{
			JsonArray ja = BsonParser.Parse(stream, true) as JsonArray;
			if (ja == null) throw new FormatException("BsonData represents JsonObject instead of JsonArray");
			return ja;
		}

		public static JsonObject ParseObject(byte[] array)
		{
			JsonObject jo = BsonParser.Parse(array, false) as JsonObject;
			if (jo == null) throw new FormatException("BsonData represents JsonArray instead of JsonObject");
			return jo;
		}
		public static JsonObject ParseObject(Stream stream)
		{
			JsonObject jo = BsonParser.Parse(stream, false) as JsonObject;
			if (jo == null) throw new FormatException("BsonData represents JsonArray instead of JsonObject");
			return jo;
		}

		public static object Parse(byte[] array)
		{
			return BsonParser.Parse(new MemoryStream(array), false);
		}
		public static object Parse(byte[] array, bool isArray)
		{
			return BsonParser.Parse(new MemoryStream(array), isArray);
		}
		public static object Parse(Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);
			return BsonParser.Parse(stream, false);
		}
		public static object Parse(Stream stream, bool isArray)
		{
			IJsonContainer obj;
			
			if (isArray)
				obj = new JsonArray();
			else
				obj = new JsonObject();

			byte[] buff;

			long docLength = stream.Position;

			string key;
			int tkey;

			buff = new byte[8];
			stream.Read(buff, 4, 4);

			docLength += BitConverter.ToInt32(buff, 4);

			do 
			{
				tkey = stream.ReadByte();

				switch (tkey)
				{
					// Double
					case 0x01:
						key = ReadCString(stream);
						stream.Read(buff, 0, 8);
						obj.InternalAdd(key, BitConverter.ToDouble(buff, 0));
						break;

					// string
					case 0x02:
						obj.InternalAdd(ReadCString(stream), ReadString(stream));
						break;

					// Objects
					case 0x03:
						obj.InternalAdd(ReadCString(stream), BsonParser.Parse(stream, false));
						break;

					// Array
					case 0x04:
						obj.InternalAdd(ReadCString(stream), BsonParser.Parse(stream, true));
						break;

					// Boolean
					case 0x08:
						key = ReadCString(stream);
						stream.Read(buff, 7, 1);
						obj.InternalAdd(key, BitConverter.ToBoolean(buff, 7));
						break;

					// Null
					case 0x0A:
						obj.InternalAdd(ReadCString(stream), null);
						break;

					// Int32
					case 0x10:
						key = ReadCString(stream);
						stream.Read(buff, 4, 4);
						obj.InternalAdd(key, BitConverter.ToInt32(buff, 4));
						break;

					// Int64
					case 0x12:
						key = ReadCString(stream);
						stream.Read(buff, 0, 8);
						obj.InternalAdd(key, BitConverter.ToInt64(buff, 0));
						break;

					default:
						break;
				}
			} while (stream.Position < docLength && tkey != 0x00);

			return obj;
		}

		private static string ReadString(Stream stream)
		{
			byte[] buff = new byte[4];
			stream.Read(buff, 0, 4);

			int len = BitConverter.ToInt32(buff, 0);

			buff = new byte[len];
			stream.Read(buff, 0, len);
			return Encoding.UTF8.GetString(buff, 0, len - 1);
		}
		private static string ReadCString(Stream stream)
		{
			List<byte> lst = new List<byte>();
			byte read;

			do
			{
				read = (byte)stream.ReadByte();
				lst.Add(read);
			} while (read != 0x00);

			return Encoding.UTF8.GetString(lst.ToArray(), 0, lst.Count - 1);
		}
	}

	class Byteifier
	{
		public static void byteify(JsonObject jo, Stream outStream)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				foreach (KeyValuePair<string, object> kvp in jo)
					Write(stream, kvp.Key, kvp.Value);

				stream.WriteByte(0x00);

				byte[] len = BitConverter.GetBytes((int)stream.Length + 4);
				outStream.Write(len, 0, 4);
				stream.WriteTo(outStream);

				stream.Dispose();
			}
		}

		public static void byteify(JsonArray ja, Stream outStream)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				for (int i = 0; i < ja.Count; i++)
					Write(stream, i.ToString(), ja[i]);

				stream.WriteByte(0x00);

				byte[] len = BitConverter.GetBytes((int)stream.Length + 4);
				outStream.Write(len, 0, 4);
				stream.WriteTo(outStream);

				stream.Dispose();
			}
		}
		private static void Write(Stream stream, string key, object obj)
		{
			if (obj == null)
			{
				stream.WriteByte(0x0A);
				WriteCString(stream, key);
			}
			else
			{
				if (obj is JsonObject)
				{
					stream.WriteByte(0x03);
					WriteCString(stream, key);
					byteify((JsonObject)obj, stream);

					return;
				}

				if (obj is JsonArray)
				{
					stream.WriteByte(0x04);
					WriteCString(stream, key);
					byteify((JsonArray)obj, stream);

					return;
				}

				if (obj is double ||
					obj is float)
				{
					stream.WriteByte(0x01);
					WriteCString(stream, key);
					WriteBytes(stream, BitConverter.GetBytes(Convert.ToDouble(obj)));

					return;
				}

				if (obj is string)
				{
					stream.WriteByte(0x02);
					WriteCString(stream, key);
					WriteString(stream, (string)obj);

					return;
				}

				if (obj is bool)
				{
					stream.WriteByte(0x08);
					WriteCString(stream, key);
					stream.WriteByte((bool)obj ? (byte)0x01 : (byte)0x02);

					return;
				}

				if (obj is sbyte ||
					obj is byte ||
					obj is short ||
					obj is ushort ||
					obj is int ||
					obj is uint)
				{
					stream.WriteByte(0x10);
					WriteCString(stream, key);
					WriteBytes(stream, BitConverter.GetBytes(unchecked((int)obj)));

					return;
				}

				if (obj is long ||
					obj is ulong)
				{
					stream.WriteByte(0x12);
					WriteCString(stream, key);
					WriteBytes(stream, BitConverter.GetBytes(unchecked((long)obj)));

					return;
				}

				if (obj == typeof(decimal))
					throw new FormatException("decimal is not supported in BsonToolkit 1.0.0");
			}
		}

		private static void WriteBytes(Stream stream, byte[] array)
		{
			stream.Write(array, 0, array.Length);
		}
		private static void WriteString(Stream stream, string key)
		{
			byte[] chars = Encoding.UTF8.GetBytes(key);
			byte[] len = BitConverter.GetBytes(chars.Length + 1);

			stream.Write(len, 0, 4);
			stream.Write(chars, 0, chars.Length);
			stream.WriteByte(0x00);
		}
		private static void WriteCString(Stream stream, string key)
		{
			byte[] chars = Encoding.UTF8.GetBytes(key);

			stream.Write(chars, 0, chars.Length);
			stream.WriteByte(0x00);
		}
	}
}

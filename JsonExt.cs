// By RyuaNerin

using System;

namespace ComputerBeacon.Json
{
	internal static class JsonExt
	{
		public static JsonObject GetJsonObject(this JsonObject jsonObject, string key)
		{
			return (JsonObject)jsonObject[key];
		}

		public static JsonArray GetJsonArray(this JsonObject jsonObject, string key)
		{
			return (JsonArray)jsonObject[key];
		}

		public static string GetString(this JsonObject jsonObject, string key)
		{
			return Convert.ToString(jsonObject[key]);
		}

		public static byte GetByte(this JsonObject jsonObject, string key)
		{
			return Convert.ToByte(jsonObject[key]);
		}
		public static sbyte GetSByte(this JsonObject jsonObject, string key)
		{
			return Convert.ToSByte(jsonObject[key]);
		}

		public static short GetInt16(this JsonObject jsonObject, string key)
		{
			return Convert.ToInt16(jsonObject[key]);
		}
		public static ushort GetUInt16(this JsonObject jsonObject, string key)
		{
			return Convert.ToUInt16(jsonObject[key]);
		}

		public static int GetInt(this JsonObject jsonObject, string key)
		{
			return Convert.ToInt32(jsonObject[key]);
		}
		public static uint GetUInt32(this JsonObject jsonObject, string key)
		{
			return Convert.ToUInt32(jsonObject[key]);
		}

		public static long GetInt64(this JsonObject jsonObject, string key)
		{
			return Convert.ToInt64(jsonObject[key]);
		}
		public static ulong GetUInt64(this JsonObject jsonObject, string key)
		{
			return Convert.ToUInt64(jsonObject[key]);
		}

		public static DateTime GetDateTime(this JsonObject jsonObject, string key)
		{
			return DateTime.Parse((string)jsonObject[key]);
		}
	}
}


namespace Synapse.General
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.IO;
    using NLog;
    using System.Globalization;
    using System.Net;
    using Newtonsoft.Json;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json.Linq;
    using System.Reflection;
    using System.IO.Compression;
    using System.Xml.Linq;
    using System.Collections.Concurrent;
    using System.Net.WebSockets;

    public static class Helpers
    {

        public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private const int DoubleRoundingPrecisionDigits = 8;

        #region write a file to disk

        public static void SaveToFile(this IEnumerable<object> items, string fileName, string header = "", bool append = false)
        {
            using StreamWriter file = new(fileName, append);
            if (!string.IsNullOrWhiteSpace(header))
                file.WriteLine(header);
            foreach (var item in items)
            {
                file.WriteLine(item.ToString());
            }
        }

        public static async Task SaveToFileAsync(this IEnumerable<object> items, string fileName, string header = "", bool append = false)
        {
            using StreamWriter file = new(fileName, append);
            if (!string.IsNullOrWhiteSpace(header))
                file.WriteLine(header);
            foreach (var item in items)
            {
                await file.WriteLineAsync(item.ToString());
            }
        }

        public static void SaveToFile<T>(this IEnumerable<T> items, string fileName, string header = "", bool append = false)
        {
            using StreamWriter file = new(fileName, append);
            if (!string.IsNullOrWhiteSpace(header))
                file.WriteLine(header);
            foreach (var item in items)
            {
                file.WriteLine(item?.ToString());
            }
        }

        public static void SaveToFile(object item, string fileName, string header = "", bool append = true)
        {
            using (StreamWriter file = new(fileName, append))
            {
                if (!string.IsNullOrWhiteSpace(header))
                    file.WriteLine(header);
                file.WriteLine(item.ToString());
            }
        }

        public static void SaveToFile<T>(T item, string fileName, string header = "", bool append = true)
        {
            using StreamWriter file = new(fileName, append);
            if (!string.IsNullOrWhiteSpace(header))
                file.WriteLine(header);
            file.WriteLine(item?.ToString());
        }

        #endregion

        #region loggin

        public static void ToError(this Logger logger, Exception ex)
        {
            logger.Error("{0}. {1}", ex.Message, ex.StackTrace);
        }

        public static void ToError(this Logger logger, Exception ex, string tag)
        {
            logger.Error($"{tag} / {ex.Message}, {ex.StackTrace}");
        }

        #endregion

        #region json

        public static T DeserializeObject<T>(this JsonSerializer serializer, string value)
        {

            try
            {
                var token = JToken.Parse(value);

                if (token is JArray)
                    token = token.SelectToken("error");
                else if (token is JObject)
                    token = token.SelectToken("error");

                if (token != null)
                    throw new Exception(token.ToString());

                using (var stringReader = new StringReader(value))
                {
                    using (var jsonTextReader = new JsonTextReader(stringReader))
                    {
                        return (T)serializer.Deserialize(jsonTextReader, typeof(T));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            //return default(T);

        }

        public static JToken TryParseToJToken(this string str)
        {
            var input = str.Trim();

            try
            {
                return JToken.Parse(input);
            }
            catch (Exception) //some other exception
            {
                return false;
            }

        }

        public static bool IsJson(this string str)
        {
            var input = str.Trim();

            if ((input.StartsWith("{") && input.EndsWith("}")) || //For object
                (input.StartsWith("[") && input.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(input);
                    return true;
                }
                catch (Exception) //some other exception
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        #endregion

        #region datetime
       
        /// <summary>
        /// Convert from UnixTimestamp (seconds) to DateTime type
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime UnixTimeSecondsToDateTime(this long seconds)
        {
           return DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;         
        }

        /// <summary>
        /// Convert from UnixTimestamp (milliseconds) to DateTime type
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DateTime UnixTimeMillisecondsToDateTime(this long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
        }

        /// <summary>
        /// Convert from DateTime to UnixTimestamp (seconds)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
           return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Convert from DateTime to UnixTimestamp (milliseconds)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Convert from DateTime to UnixTimestamp (part seconds)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static double ToUnixTimePartSeconds(this DateTime dateTime)
        {
            return dateTime.Subtract(DateTime.UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// Convert from UnixTimestamp (part seconds) to DateTime type
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime UnixTimeToDateTimeFromPartSeconds(this double seconds)
        {
            return DateTime.UnixEpoch.AddSeconds(seconds);
        }

        /// <summary>
        /// Convert from UnixTimestamp (tiks) to DateTime type
        /// </summary>
        /// <param name="tiks"></param>
        /// <returns></returns>
        public static DateTime UnixTimeToDateTimeFromTicks(this long tiks)
        {
            return DateTime.UnixEpoch.AddTicks(tiks);
        }

        /// <summary>
        /// Convert from TimeSpan to OLE automation value.
        /// </summary>
        /// <param name="ts">TimeSpan</param>
        /// <returns>OLE automation value</returns>
        public static double ToOADate(this TimeSpan ts)
        {
            return (DateTime.FromOADate(0) + ts).ToOADate();
        }

        /// <summary>
        /// Convert from OLE automation value to TimeSpan.
        /// </summary>
        /// <param name="oats">OLE automation value</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan FromOADate(double oats)
        {
            return DateTime.FromOADate(oats) - DateTime.FromOADate(0);
        }

        #endregion

        #region price formating

        public static double Normalize(this double value)
        {
            return Math.Round(value, DoubleRoundingPrecisionDigits, MidpointRounding.AwayFromZero);
        }

        public static string ToStringNormalized(this double value)
        {
            return value.ToString("0." + new string('#', DoubleRoundingPrecisionDigits), InvariantCulture);
        }

        public static double PriceRound(this double value, double step)
        {
            return step * Math.Floor(value / step);
        }

        public static double AmountRound(this double value, double step)
        {
            return step * Math.Floor(value / step);
        }

        public static int Decimals(this double value)
        {
            string str = value.ToString("F12").TrimEnd('0');

            string[] temp = str.Split('.', ',');

            if (temp.Count() > 1)
                return temp[1].Length;
            else
                return 0;
        }

        /// <summary>
        /// Calculates decimals based on the first non-zero digit. This is not a "strict" way of calculating the number of digits after the decimal point.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetDecimals(this double value)
        {

            if (double.IsNaN(value)) return - 1;

            string s = value.ToString("F10", InvariantCulture).TrimEnd('0');

            int dot = s.IndexOf('.');
            if (dot == -1) return 0;

            int decimals = 0;

            for(var i = dot + 1; i < s.Length; i++)
            {
                decimals++;
                if (s[i] != '0')
                    break;
            }

            return decimals;
             

        }

        #endregion     

        #region XAML

        public static string GetPropertyValues<T>(this T obj) where T : class
        {

            Type t = obj.GetType();

            PropertyInfo[] props = t.GetProperties();

            var sb = new StringBuilder();

            foreach (var p in props)
                sb.Append(string.Format("{0}:{1}, ", p.Name, p.GetValue(obj)));

            return sb.ToString();

        }

        public static IEnumerable<string> GetPropertyNames<T>(this T obj) where T : class
        {
            Type t = obj.GetType();
            PropertyInfo[] props = t.GetProperties();
            return props.Select(p => p.Name);
        }

        #endregion

        #region REST

        public static string ToParamString(this Dictionary<string, string> param)
        {
            if (param == null) return "";

            StringBuilder b = new();
            foreach (var item in param)
                b.Append(string.Format("&{0}={1}", item.Key, WebUtility.UrlEncode(item.Value)));

            try { return b.ToString().Substring(1); }
            catch (Exception) { return ""; }
        }

        public static string GetResponseString(this HttpWebRequest request)
        {
            using var response = request.GetResponse();
            using var stream = response.GetResponseStream() ?? throw new NullReferenceException("The HttpWebRequest's response stream cannot be empty.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        #endregion

        public static string[] ReadAllLines(this FileInfo fileinfo)
        {
            if (!File.Exists(fileinfo.FullName))
                return File.ReadAllLines(fileinfo.FullName);
            else
                return null;
        }

        public static string Decompress(this byte[] baseBytes)
        {
            using var decompressedStream = new MemoryStream();
            using var compressedStream = new MemoryStream(baseBytes);
            using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
            deflateStream.CopyTo(decompressedStream);
            decompressedStream.Position = 0;
            using var streamReader = new StreamReader(decompressedStream);
            return streamReader.ReadToEnd();
        }

        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {

            while (!queue.IsEmpty)
            {
                queue.TryDequeue(out T item);
            }

        }

        public static int? FindIndex<T>(this T[] array, int startIndex, Func<T, bool> predicate)
        {
            for (int i = startIndex; i < array.Length; i++)
            {
                if (predicate(array[i]))
                    return i;
            }
            return null;
        }


    }
}


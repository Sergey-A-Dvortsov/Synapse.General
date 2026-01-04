
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
            using (StreamWriter file = new(fileName, append))
            {
                if (!string.IsNullOrWhiteSpace(header))
                    file.WriteLine(header);
                foreach (var item in items)
                {
                    file.WriteLine(item.ToString());
                }
            }
        }

        public static async Task SaveToFileAsync(this IEnumerable<object> items, string fileName, string header = "", bool append = false)
        {
            using (StreamWriter file = new(fileName, append))
            {
                if (!string.IsNullOrWhiteSpace(header))
                    file.WriteLine(header);
                foreach (var item in items)
                {
                    await file.WriteLineAsync(item.ToString());
                }
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

        #endregion

        #region loggin

        public static void ToError(this Logger logger, Exception ex)
        {
            logger.Error("{0}. {1}", ex.Message, ex.StackTrace);
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
       
        public static DateTime UnixTimeSecondsToDateTime(this long seconds)
        {
           return DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;         
        }

        public static DateTime UnixTimeMillisecondsToDateTime(this long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
        }

        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
           return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
        }

        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
        }

        public static double ToUnixTimePartSeconds(this DateTime dateTime)
        {
            return dateTime.Subtract(DateTime.UnixEpoch).TotalSeconds;
        }

        public static DateTime UnixTimeToDateTimeFromPartSeconds(this double seconds)
        {
            return DateTime.UnixEpoch.AddSeconds(seconds);
        }

         public static DateTime UnixTimeToDateTimeFromTicks(this long tiks)
        {
            return DateTime.UnixEpoch.AddTicks(tiks);
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
            string str = value.ToString("F12");

            while (true)
            {
                if (str.Last() == '0')
                    str = str.Remove(str.Length - 1, 1);
                else
                    break;
            }

            string[] temp = str.Split('.', ',');

            if (temp.Count() > 1)
                return temp[1].Length;
            else
                return 0;
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

        public static Sides Invert(this Sides side)
        {
            return side == Sides.Sell ? Sides.Buy : Sides.Sell;
        }

        public static ulong CreateUserId()
        {
            return (ulong)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
        }

        public static ulong GetUnicId(this ref DateTime lasttime, ref int lastcnt)
        {
            var nowtime = DateTime.Now;

            if (Math.Round((nowtime - lasttime).TotalMilliseconds, 0) > 0)
                lastcnt = 0;
            else
                lastcnt += 1;
            var strid = string.Format("{0}{1:000}", DateTime.Now.ToString("yyMMddHHmmssfff"), lastcnt);
            lasttime = nowtime;

            return ulong.Parse(strid);

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

        private static int GetMonth(char smb)
        {
            //A-01;B-02;C-03;D-04;E-05;F-06;G-07;H-08;I-09;J-10;K-11;L-12;
            switch (smb)
            {
                case 'A':
                    return 1;
                case 'B':
                    return 2;
                case 'C':
                    return 3;
                case 'D':
                    return 4;
                case 'E':
                    return 5;
                case 'F':
                    return 6;
                case 'G':
                    return 7;
                case 'H':
                    return 8;
                case 'I':
                    return 9;
                case 'J':
                    return 10;
                case 'K':
                    return 11;
                case 'L':
                    return 12;
                default:
                    return 99;
            }

        }

        private static int GetYear(char smb)
        {
            //год A - 0; B - 1; C - 2; ; D - 3; E - 4; ; F - 5; G - 6; H - 7; K - 8; L - 9; 
            switch (smb)
            {
                case 'A':
                    return 0;
                case 'B':
                    return 1;
                case 'C':
                    return 2;
                case 'D':
                    return 3;
                case 'E':
                    return 4;
                case 'F':
                    return 5;
                case 'G':
                    return 6;
                case 'H':
                    return 7;
                case 'K':
                    return 8;
                case 'L':
                    return 9;
                default:
                    return 99;
            }

        }

        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {

            while (!queue.IsEmpty)
            {
                queue.TryDequeue(out T item);
            }

        }

    }
}


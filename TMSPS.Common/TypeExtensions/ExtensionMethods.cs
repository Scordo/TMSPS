using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using TMSPS.Core.Common;

namespace System
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrTimmedEmpty(this string text)
        {
            return text == null || text.Trim().Length == 0;
        }

        /// <summary>
        /// Extends the datatype string with the ability to convert the current instance into a sha1 hash
        /// </summary>
        /// <param name="text">The text to hash. If the text is null, the method will return null</param>
        /// <returns></returns>
        public static string ToHash(this string text)
        {
            if (text == null)
                return null;

            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(text));

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

		public static T Dequeue<T>(this Queue<T> queue, Predicate<T> predicate, object lockObject)
		{
			lock (lockObject)
			{
				if (queue.Count == 0)
					return default(T);

				T peekValue = queue.Peek();

				return predicate(peekValue) ? queue.Dequeue() : default(T);
			}
		}

        public static string InnerXML(this XElement element)
        {
            StringBuilder result = new StringBuilder();

            foreach (XNode node in element.Nodes())
                result.Append(node.ToString());

            return result.ToString();
        }

        /// <summary>
        /// Converts an Enumerable to an xml-representation that can be passed as a list to a stored procedure
        /// </summary>
        /// <param name="objList">The list of objects.</param>
        /// <returns>An xml representing a list of objects</returns>
        public static string ToXmlListString<T>(this IEnumerable<T> objList)
        {
            if (objList == null)
                return null;

            StringBuilder objXMLBuilder = new StringBuilder();

            foreach (T obj in objList)
            {
                objXMLBuilder.AppendFormat("<i>{0}</i>", obj);
            }

            if (objXMLBuilder.Length != 0)
            {
                objXMLBuilder.Insert(0, "<l>");
                objXMLBuilder.Append("</l>");
            }

            string strResult = objXMLBuilder.ToString();

            return strResult.Trim().Length == 0 ? null : strResult;
        }

        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return dictionary == null ? null : new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        public static string[] ToArray(this IDictionary<string, string> dictionary)
        {
            if (dictionary == null)
                return null;

            List<string> values = new List<string>();

            foreach (var pair in dictionary)
            {
                values.Add(pair.Key);
                values.Add(pair.Value);
            }

            return values.ToArray();
        }

        public static void ForEach<T>(this IEnumerable<T> instances, Action<T, int> action)
        {
            int i = 0;

            foreach (T instance in instances)
            {
                action(instance, i);
                i++;
            }
        }
    }
}
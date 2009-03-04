using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;

namespace System
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrTimmedEmpty(this string text)
        {
            return text == null || text.Trim().Length == 0;
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
    }
}
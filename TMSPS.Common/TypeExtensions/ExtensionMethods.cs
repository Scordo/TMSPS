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
    }
}
using System;
using System.Text;

namespace Dynatrace.OpenKit.Util.Json.Writer
{
    internal class JsonValueWriter
    {
        private StringBuilder stringBuilder;

        internal JsonValueWriter()
        {
            stringBuilder = new StringBuilder();
        }

        /// <summary>
        /// Appending characters for opening an array in a JSON string
        /// </summary>
        internal void OpenArray()
        {
            stringBuilder.Append("[");
        }

        /// <summary>
        /// Appending characters for closing an array in a JSON string
        /// </summary>
        internal void CloseArray()
        {
            stringBuilder.Append("]");
        }

        /// <summary>
        /// Appending characters for opening an object in a JSON string
        /// </summary>
        internal void OpenObject()
        {
            stringBuilder.Append("{");
        }

        /// <summary>
        /// Appending characters for closing an object in a JSON string
        /// </summary>
        internal void CloseObject()
        {
            stringBuilder.Append("}");
        }

        /// <summary>
        /// Appending characters for seperating arrays, objects and values in a JSON string
        /// </summary>
        internal void InsertElementSeperator()
        {
            stringBuilder.Append(",");
        }

        /// <summary>
        /// Appending characters for seperating a key value pair in a JSON string
        /// </summary>
        internal void InsertKeyValueSeperator()
        {
            stringBuilder.Append(":");
        }

        /// <summary>
        /// Appending characters for a key in a JSON string
        /// </summary>
        /// <param name="key">JSON key</param>
        internal void InsertKey(string key)
        {
            stringBuilder.Append("\"");
            stringBuilder.Append(key);
            stringBuilder.Append("\"");
        }

        /// <summary>
        /// Appending characters for a string value in a JSON string
        /// </summary>
        /// <param name="value"></param>
        internal void InsertStringValue(string value)
        {
            stringBuilder.Append("\"");
            stringBuilder.Append(value);
            stringBuilder.Append("\"");
        }

        /// <summary>
        /// Appending characters for value which is not a string in a JSON string
        /// </summary>
        /// <param name="value"></param>
        internal void InsertValue(string value)
        {
            stringBuilder.Append(value);
        }

        /// <summary>
        /// Returning the whole JSON string
        /// </summary>
        /// <returns>JSON string</returns>
        public override String ToString()
        {
            return stringBuilder.ToString();
        }
    }
}

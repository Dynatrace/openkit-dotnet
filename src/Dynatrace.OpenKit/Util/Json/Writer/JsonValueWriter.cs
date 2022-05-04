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
            stringBuilder.Append(EscapeString(key));
            stringBuilder.Append("\"");
        }

        /// <summary>
        /// Appending characters for a string value in a JSON string
        /// </summary>
        /// <param name="value"></param>
        internal void InsertStringValue(string value)
        {
            stringBuilder.Append("\"");
            stringBuilder.Append(EscapeString(value));
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

        /// <summary>
        /// Escaping the string used in JSON obj
        /// </summary>
        /// <param name="value">string value which should be escaped</param>
        /// <returns>Escaped string value</returns>
        internal static string EscapeString(string value)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '/': sb.Append("\\/"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    default:
                        if ('\x00' <= value[i] && value[i] <= '\x1f')
                        {
                            sb.Append("\\u");
                            sb.Append(((int)value[i]).ToString("x04"));
                        }
                        else
                        {
                            sb.Append(value[i]);
                        }
                        break;
                }
            }

            return sb.ToString();
        }
    }
}

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Util.Json.Objects;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Objects
{
    internal class EventPayloadBuilder 
    {

        /// <summary>
        /// <see cref="ILogger"/> for tracing log messages.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Dictionary containing attributes for sendEvent API
        /// </summary>
        private readonly Dictionary<string, JsonValue> attributes;

        public EventPayloadBuilder(Dictionary<string, JsonValue> attributes, ILogger logger)
        {
            this.logger = logger;

            if(attributes == null)
            {
                this.attributes = new Dictionary<string, JsonValue>();
            }
            else
            {
                this.attributes = new Dictionary<string, JsonValue>(attributes);
            }
        }

        public EventPayloadBuilder AddOverridableAttribute(string key, JsonValue value)
        {
            if (value != null && !attributes.ContainsKey(key))
            {
                attributes[key] = value;
            }

            return this;
        }

        public EventPayloadBuilder AddNonOverridableAttribute(string key, JsonValue value)
        {
            if (value != null)
            {
                if (attributes.ContainsKey(key))
                {
                    logger.Warn($"EventPayloadBuilder AddNonOverrideableAttribute: ${key} is reserved for internal values!");
                }
               
                attributes[key] = value;
            }

            return this;
        }

        public string Build()
        {
            return JsonObjectValue.FromDictionary(attributes).ToString();
        }

        /// <summary>
        /// Removes reservered internal attributes from the provided attributes
        /// </summary>
        internal EventPayloadBuilder CleanReservedInternalAttributes()
        {
            foreach (KeyValuePair<string, JsonValue> entry in new Dictionary<string, JsonValue>(attributes))
            {
                if (entry.Key == "dt" || entry.Key.StartsWith("dt."))
                {
                    logger.Warn($"EventPayloadBuilder CleanReservedInternalAttributes: ${entry.Key} is reserved for internal values!");
                    attributes.Remove(entry.Key);
                }
            }

            return this;
        }


    }
}

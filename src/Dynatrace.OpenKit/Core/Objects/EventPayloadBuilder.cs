using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Util.Json.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        private readonly Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();

        public EventPayloadBuilder(Dictionary<string, JsonValue> attributes, ILogger logger)
        {
            this.logger = logger;

            InitializeInternalAttributes(attributes);
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
        /// Initialize the internal attribute dictionary and filter out the reserved internal keys already
        /// </summary>
        /// <param name="extAttributes">External attributes coming from the API</param>
        private void InitializeInternalAttributes(Dictionary<string, JsonValue> extAttributes)
        {
            if(extAttributes != null)
            {
                foreach (KeyValuePair<string, JsonValue> entry in extAttributes)
                {
                    if (entry.Key == "dt" || entry.Key.StartsWith("dt."))
                    {
                        logger.Warn($"EventPayloadBuilder InitializeInternalAttributes: ${entry.Key} is reserved for internal values!");
                    }
                    else
                    {
                        this.attributes.Add(entry.Key, entry.Value);
                    }
                }
            }
        }
    }
}

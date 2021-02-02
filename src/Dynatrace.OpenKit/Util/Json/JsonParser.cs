//
// Copyright 2018-2021 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Collections.Generic;
using Dynatrace.OpenKit.Util.Json.Lexer;
using Dynatrace.OpenKit.Util.Json.Objects;
using Dynatrace.OpenKit.Util.Json.Parser;

namespace Dynatrace.OpenKit.Util.Json
{
    /// <summary>
    ///     JSON parser class for parsing JSON input strings.
    /// </summary>
    public class JsonParser
    {
        /// <summary>
        ///     Error message used when encountering unterminated JSON arrays.
        /// </summary>
        private const string UNTERMINATED_JSON_ARRAY_ERROR = "Unterminated JSON array";

        /// <summary>
        ///     Error message used when encountering unterminated JSON objects.
        /// </summary>
        private const string UNTERMINATED_JSON_OBJECT_ERROR = "Unterminated JSON object";

        /// <summary>
        ///     Lexical analyzer
        /// </summary>
        private readonly JsonLexer lexer;

        /// <summary>
        ///     Stack storing the parser state. This is required to parse nested objects
        /// </summary>
        private readonly LinkedList<JsonParserState> stateStack = new LinkedList<JsonParserState>();

        /// <summary>
        ///     Stack storing JSON values (keep in mind that there are nested values)
        /// </summary>
        private readonly LinkedList<JsonValueContainer> valueContainerStack = new LinkedList<JsonValueContainer>();

        /// <summary>
        ///     Parsed JSON value object
        /// </summary>
        private JsonValue parsedValue;

        /// <summary>
        ///     Current state of the parser
        /// </summary>
        private JsonParserState state = JsonParserState.INIT;

        /// <summary>
        ///     Constructor taking the JSON input string.
        /// </summary>
        /// <param name="input">JSON input string</param>
        public JsonParser(string input) : this(new JsonLexer(input))
        {
        }

        /// <summary>
        ///     Internal constructor taking the lexical analyzer.
        ///     <para>
        ///         This constructor can be used by test, when mocking the lexer is needed.
        ///     </para>
        /// </summary>
        /// <param name="lexer">lexical analyzer</param>
        protected JsonParser(JsonLexer lexer)
        {
            this.lexer = lexer;
        }

        /// <summary>
        ///     Property for retrieving the current parser state.
        ///     <para>
        ///         This property is only intended for unit tests to properly retrieve the current state.
        ///     </para>
        /// </summary>
        public JsonParserState State => state;

        /// <summary>
        ///     Parses the JSON string passed in the constructor and returns the parsed JSON value object.
        /// </summary>
        /// <returns>the parsed JSON value object</returns>
        /// <exception cref="JsonParserException">in case an error whole parsing the string is encountered.</exception>
        public JsonValue Parse()
        {
            if (state == JsonParserState.END)
            {
                return parsedValue;
            }

            if (state == JsonParserState.ERROR)
            {
                throw new JsonParserException("JSON parser is in erroneous state");
            }

            try
            {
                parsedValue = DoParse();
            }
            catch (JsonLexerException e)
            {
                state = JsonParserState.ERROR;
                throw new JsonParserException("Caught exception from lexical analysis", e);
            }

            return parsedValue;
        }

        /// <summary>
        ///     Parses the JSON string passed in the constructor and returns the parsed JSON value object.
        /// </summary>
        /// <returns>the parsed JSON value object.</returns>
        /// <exception cref="JsonParserException">if there is an error during lexical analysis of the JSON string.</exception>
        private JsonValue DoParse()
        {
            JsonToken token;
            do
            {
                token = lexer.NextToken();
                switch (state)
                {
                    case JsonParserState.INIT:
                        ParseInitState(token);
                        break;
                    case JsonParserState.IN_ARRAY_START:
                        ParseInArrayStartState(token);
                        break;
                    case JsonParserState.IN_ARRAY_VALUE:
                        ParseInArrayValueState(token);
                        break;
                    case JsonParserState.IN_ARRAY_DELIMITER:
                        ParseInArrayDelimiterState(token);
                        break;
                    case JsonParserState.IN_OBJECT_START:
                        ParseInObjectStartState(token);
                        break;
                    case JsonParserState.IN_OBJECT_KEY:
                        ParseInObjectKeyState(token);
                        break;
                    case JsonParserState.IN_OBJECT_COLON:
                        ParseInObjectColonState(token);
                        break;
                    case JsonParserState.IN_OBJECT_VALUE:
                        ParseInObjectValueState(token);
                        break;
                    case JsonParserState.IN_OBJECT_DELIMITER:
                        ParseInObjectDelimiterState(token);
                        break;
                    case JsonParserState.END:
                        ParseEndState(token);
                        break;
                    case JsonParserState.ERROR:
                        // this should never be reached, since whenever there is a transition into error state, an
                        // exception is thrown right afterwards. This is just a precaution
                        throw new JsonParserException(UnexpectedTokenErrorMessage(token, "in error state"));
                    default:
                        // precaution in case a new literal is added to the parser state enum
                        throw new JsonParserException(InternalParserErrorMessage(state, "Unexpected JsonParserState"));
                }
            } while (token != null);

            EnsureValueContainerStackIsNotEmpty();

            return valueContainerStack.First.Value.JsonValue;
        }

        /// <summary>
        ///     Parses the given token in init state.
        ///     <para>
        ///         This state is the state right after starting parsing the JSON string.
        ///         Valid and expected tokens in this state are simple value tokens or start of a compound value.
        ///     </para>
        /// </summary>
        /// <param name="token">the token to parse</param>
        /// <exception cref="JsonParserException">in case parsing fails.</exception>
        private void ParseInitState(JsonToken token)
        {
            EnsureTokenIsNotNull(token, "No JSON object could be decoded");

            switch (token.TokenType)
            {
                case JsonTokenType.LITERAL_NULL: // fallthrough
                case JsonTokenType.LITERAL_BOOLEAN: // fallthrough
                case JsonTokenType.VALUE_STRING: // fallthrough
                case JsonTokenType.VALUE_NUMBER: // fallthrough
                    valueContainerStack.AddFirst(new JsonValueContainer(TokenToSimpleJsonValue(token)));
                    state = JsonParserState.END;
                    break;
                case JsonTokenType.LEFT_SQUARE_BRACKET:
                    var jsonValueList = new LinkedList<JsonValue>();
                    valueContainerStack.AddFirst(new JsonValueContainer(JsonArrayValue.FromList(jsonValueList),
                        jsonValueList));
                    state = JsonParserState.IN_ARRAY_START;
                    break;
                case JsonTokenType.LEFT_BRACE:
                    var jsonObjectDict = new Dictionary<string, JsonValue>();
                    valueContainerStack.AddFirst(new JsonValueContainer(JsonObjectValue.FromDictionary(jsonObjectDict),
                        jsonObjectDict));
                    state = JsonParserState.IN_OBJECT_START;
                    break;
                default:
                    state = JsonParserState.ERROR;
                    throw new JsonParserException(UnexpectedTokenErrorMessage(token, "at start of input"));
            }
        }

        /// <summary>
        ///     Parse token in state when an array has been started.
        /// </summary>
        /// <param name="token">the token to parse</param>
        /// <exception cref="JsonParserException">in case parsing fails</exception>
        private void ParseInArrayStartState(JsonToken token)
        {
            EnsureTokenIsNotNull(token, UNTERMINATED_JSON_ARRAY_ERROR);

            switch (token.TokenType)
            {
                case JsonTokenType.LITERAL_NULL: // fallthrough
                case JsonTokenType.LITERAL_BOOLEAN: // fallthrough
                case JsonTokenType.VALUE_STRING: // fallthrough
                case JsonTokenType.VALUE_NUMBER:
                    EnsureTopLevelElementIsAJsonArray();
                    valueContainerStack.First.Value.BackingList.Add(TokenToSimpleJsonValue(token));
                    state = JsonParserState.IN_ARRAY_VALUE;
                    break;
                case JsonTokenType.LEFT_SQUARE_BRACKET:
                    // start of nested array as first element in the current array
                    ParseStartOfNestedArray();
                    break;
                case JsonTokenType.LEFT_BRACE:
                    // start of nested object as first element in the current array
                    ParseStartOfNestedObject();
                    break;
                case JsonTokenType.RIGHT_SQUARE_BRACKET:
                    CloseCompositeJsonValueAndRestoreState();
                    break;
                default:
                    state = JsonParserState.ERROR;
                    throw new JsonParserException(UnexpectedTokenErrorMessage(token, "at beginning of array"));
            }
        }

        /// <summary>
        ///     Parses a token in an array and an array value has already been parsed previously.
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="JsonParserException"></exception>
        private void ParseInArrayValueState(JsonToken token)
        {
            EnsureTokenIsNotNull(token, UNTERMINATED_JSON_ARRAY_ERROR);

            switch (token.TokenType)
            {
                case JsonTokenType.COMMA:
                    state = JsonParserState.IN_ARRAY_DELIMITER;
                    break;
                case JsonTokenType.RIGHT_SQUARE_BRACKET:
                    CloseCompositeJsonValueAndRestoreState();
                    break;
                default:
                    state = JsonParserState.ERROR;
                    throw new JsonParserException(UnexpectedTokenErrorMessage(token,
                        "in array after value has been parsed"));
            }
        }

        /// <summary>
        ///     Parses a token in an array after a delimiter has been encountered previously
        /// </summary>
        /// <param name="token">the token to parse</param>
        /// <exception cref="JsonParserException">in case parsing fails</exception>
        private void ParseInArrayDelimiterState(JsonToken token)
        {
            EnsureTokenIsNotNull(token, UNTERMINATED_JSON_ARRAY_ERROR);

            switch (token.TokenType)
            {
                case JsonTokenType.LITERAL_NULL: // fallthrough
                case JsonTokenType.LITERAL_BOOLEAN: // fallthrough
                case JsonTokenType.VALUE_STRING: // fallthrough
                case JsonTokenType.VALUE_NUMBER:
                    EnsureTopLevelElementIsAJsonArray();
                    valueContainerStack.First.Value.BackingList.Add(TokenToSimpleJsonValue(token));
                    state = JsonParserState.IN_ARRAY_VALUE;
                    break;
                case JsonTokenType.LEFT_SQUARE_BRACKET:
                    // start of a nested array as element in the current array
                    ParseStartOfNestedArray();
                    break;
                case JsonTokenType.LEFT_BRACE:
                    // start of a nested object as element in the current array
                    ParseStartOfNestedObject();
                    break;
                default:
                    state = JsonParserState.ERROR;
                    throw new JsonParserException(UnexpectedTokenErrorMessage(token, "in array after delimiter"));
            }
        }

        /// <summary>
        ///     Utility method to parse the start of a nested object.
        ///     <para>
        ///         This method is called when the left brace token is encountered.
        ///     </para>
        /// </summary>
        private void ParseStartOfNestedObject()
        {
            stateStack.AddFirst(JsonParserState.IN_ARRAY_VALUE);
            var jsonObjectDict = new Dictionary<string, JsonValue>();
            valueContainerStack.AddFirst(new JsonValueContainer(JsonObjectValue.FromDictionary(jsonObjectDict),
                jsonObjectDict));
            state = JsonParserState.IN_OBJECT_START;
        }

        /// <summary>
        ///     Utility method to parse the start of a nested array.
        ///     <para>
        ///         This method is called when the left square bracket token is encountered.
        ///     </para>
        /// </summary>
        private void ParseStartOfNestedArray()
        {
            stateStack.AddFirst(JsonParserState.IN_ARRAY_VALUE);
            var jsonValueList = new LinkedList<JsonValue>();
            valueContainerStack.AddFirst(new JsonValueContainer(JsonArrayValue.FromList(jsonValueList), jsonValueList));
            state = JsonParserState.IN_ARRAY_START;
        }

        /// <summary>
        ///     Parse a token right after a JSON object has ben started.
        /// </summary>
        /// <param name="token">the token to parse</param>
        /// <exception cref="JsonParserException">in case parsing fails</exception>
        private void ParseInObjectStartState(JsonToken token)
        {
            EnsureTokenIsNotNull(token, UNTERMINATED_JSON_OBJECT_ERROR);
            EnsureTopLevelElementIsAJsonObject();

            switch (token.TokenType)
            {
                case JsonTokenType.RIGHT_BRACE:
                    // object is closed right after it was started
                    CloseCompositeJsonValueAndRestoreState();
                    break;
                case JsonTokenType.VALUE_STRING:
                    valueContainerStack.First.Value.LastParsedObjectKey = token.Value;
                    state = JsonParserState.IN_OBJECT_KEY;
                    break;
                default:
                    state = JsonParserState.ERROR;
                    throw new JsonParserException(UnexpectedTokenErrorMessage(token,
                        "encountered - object key expected"));
            }
        }

        /// <summary>
        ///     Parses a token right after a JSON key token was parsed.
        /// </summary>
        /// <param name="token">the token to parse</param>
        /// <exception cref="JsonParserException">in case parsing fails</exception>
        private void ParseInObjectKeyState(JsonToken token)
        {
            EnsureTokenIsNotNull(token, UNTERMINATED_JSON_OBJECT_ERROR);

            switch (token.TokenType)
            {
                case JsonTokenType.COLON:
                    state = JsonParserState.IN_OBJECT_COLON;
                    break;
                default:
                    state = JsonParserState.ERROR;
                    throw new JsonParserException(UnexpectedTokenErrorMessage(token,
                        "encountered - key-value delimiter expected"));
            }
        }

        /// <summary>
        ///     Parses a token right after a JSON key-value delimiter (":") was parsed.
        /// </summary>
        /// <param name="token">the token to parse.</param>
        /// <exception cref="JsonParserException">in case parsing fails</exception>
        private void ParseInObjectColonState(JsonToken token)
        {
            EnsureTokenIsNotNull(token, UNTERMINATED_JSON_OBJECT_ERROR);
            EnsureTopLevelElementIsAJsonObject();

            switch (token.TokenType)
            {
                case JsonTokenType.VALUE_NUMBER: // fallthrough
                case JsonTokenType.VALUE_STRING: // fallthrough
                case JsonTokenType.LITERAL_BOOLEAN: // fallthrough
                case JsonTokenType.LITERAL_NULL: // fallthrough
                    // simple JSON value as object value
                    valueContainerStack.First.Value.LastParsedObjectValue = TokenToSimpleJsonValue(token);
                    state = JsonParserState.IN_OBJECT_VALUE;
                    break;
                case JsonTokenType.LEFT_BRACE:
                    // value is an object
                    var jsonObjectDict = new Dictionary<string, JsonValue>();
                    valueContainerStack.AddFirst(new JsonValueContainer(JsonObjectValue.FromDictionary(jsonObjectDict),
                        jsonObjectDict));
                    stateStack.AddFirst(JsonParserState.IN_OBJECT_VALUE);
                    state = JsonParserState.IN_OBJECT_START;
                    break;
                case JsonTokenType.LEFT_SQUARE_BRACKET:
                    // value is an array
                    var jsonValueList = new LinkedList<JsonValue>();
                    valueContainerStack.AddFirst(new JsonValueContainer(JsonArrayValue.FromList(jsonValueList),
                        jsonValueList));
                    stateStack.AddFirst(JsonParserState.IN_OBJECT_VALUE);
                    state = JsonParserState.IN_ARRAY_START;
                    break;
                default:
                    // any other token
                    throw new JsonParserException(
                        UnexpectedTokenErrorMessage(token, "after key-value pair encountered"));
            }
        }

        /// <summary>
        ///     Parses a token right after a JSON object value was parsed.
        ///     <para>
        ///         Note for now: An object can have more than one key-value pairs, but the value must be a simple type.
        ///     </para>
        /// </summary>
        /// <param name="token">the token to parse</param>
        /// <exception cref="JsonParserException">in case parsing fails</exception>
        private void ParseInObjectValueState(JsonToken token)
        {
            EnsureTokenIsNotNull(token, UNTERMINATED_JSON_OBJECT_ERROR);
            EnsureTopLevelElementIsAJsonObject();

            switch (token.TokenType)
            {
                case JsonTokenType.RIGHT_BRACE:
                    // object is closed right after some value
                    // push last parsed key/value into the dictionary
                    EnsureKeyValuePairWasParsed();
                    var key = valueContainerStack.First.Value.LastParsedObjectKey;
                    var value = valueContainerStack.First.Value.LastParsedObjectValue;
                    valueContainerStack.First.Value.BackingDict[key] = value;
                    CloseCompositeJsonValueAndRestoreState();
                    break;
                case JsonTokenType.COMMA:
                    // expecting more entries in the current object, push existing and make state transition
                    PutLastParsedKeyValuePairIntoObject();
                    state = JsonParserState.IN_OBJECT_DELIMITER;
                    break;
                default:
                    // any other token
                    state = JsonParserState.ERROR;
                    throw new JsonParserException(
                        UnexpectedTokenErrorMessage(token, "after key-value pair encountered"));
            }
        }

        /// <summary>
        ///     Parses a token right after a delimiter has been parsed.
        ///     <para>
        ///         Note for now: not supported yet.
        ///     </para>
        /// </summary>
        /// <param name="token">the token to parse</param>
        /// <exception cref="JsonParserException">in case parsing fails.</exception>
        private void ParseInObjectDelimiterState(JsonToken token)
        {
            EnsureTokenIsNotNull(token, UNTERMINATED_JSON_OBJECT_ERROR);
            EnsureTopLevelElementIsAJsonObject();

            switch (token.TokenType)
            {
                case JsonTokenType.VALUE_STRING:
                    valueContainerStack.First.Value.LastParsedObjectKey = token.Value;
                    state = JsonParserState.IN_OBJECT_KEY;
                    break;
                default:
                    state = JsonParserState.ERROR;
                    throw new JsonParserException(UnexpectedTokenErrorMessage(token,
                        "encountered - object key expected"));
            }
        }

        /// <summary>
        ///     Parses a token in end state
        /// </summary>
        /// <param name="token">the token to parse</param>
        /// <exception cref="JsonParserException">in case parsing fails.</exception>
        private void ParseEndState(JsonToken token)
        {
            if (token == null)
            {
                // end of input. as expected in regular terminal state
                return;
            }

            state = JsonParserState.ERROR;
            throw new JsonParserException(UnexpectedTokenErrorMessage(token, "at end of input"));
        }

        /// <summary>
        ///     Helper method to remove the top level JSON value from the value stack and also the state.
        /// </summary>
        /// <exception cref="JsonParserException">in case of an exception</exception>
        private void CloseCompositeJsonValueAndRestoreState()
        {
            EnsureValueContainerStackIsNotEmpty();

            if (valueContainerStack.Count != stateStack.Count + 1)
            {
                throw new JsonParserException(InternalParserErrorMessage(state,
                    "valueContainerStack and stateStack size mismatch"));
            }

            if (valueContainerStack.Count == 1)
            {
                // the outermost array is terminated, do not remove anything from the stack
                state = JsonParserState.END;
                return;
            }

            var currentValue = valueContainerStack.First.Value.JsonValue;
            valueContainerStack.RemoveFirst();

            // ensure that there is a new top level element which is a composite value (object or array)
            EnsureValueContainerStackIsNotEmpty();
            var topLevelElement = valueContainerStack.First.Value;
            if (topLevelElement.JsonValue.ValueType == JsonValueType.ARRAY)
            {
                if (topLevelElement.BackingList == null)
                {
                    // precaution to ensure we did not do something wrong
                    throw new JsonParserException(InternalParserErrorMessage(state, "backing list is null"));
                }

                topLevelElement.BackingList.Add(currentValue);
            }
            else if (topLevelElement.JsonValue.ValueType == JsonValueType.OBJECT)
            {
                topLevelElement.LastParsedObjectValue = currentValue;
            }
            else
            {
                throw new JsonParserException(InternalParserErrorMessage(state, "not a composite top level object"));
            }

            state = stateStack.First.Value;
            stateStack.RemoveFirst();
        }

        /// <summary>
        ///     Puts the last parsed key value pair into the top level stack element.
        ///     <para>
        ///         Some sanity checks are performed to ensure consistency
        ///     </para>
        /// </summary>
        private void PutLastParsedKeyValuePairIntoObject()
        {
            EnsureKeyValuePairWasParsed();
            EnsureTopLevelElementIsAJsonObject();

            var topLevelElement = valueContainerStack.First.Value;
            var key = topLevelElement.LastParsedObjectKey;
            var value = topLevelElement.LastParsedObjectValue;

            topLevelElement.BackingDict[key] = value;
            topLevelElement.LastParsedObjectKey = null;
            topLevelElement.LastParsedObjectValue = null;
        }

        /// <summary>
        ///     Helper method for converting a simple JSON token to a JSON value.
        /// </summary>
        /// <param name="token">the token to convert to a JSON value</param>
        /// <returns>the converted JSON value</returns>
        private static JsonValue TokenToSimpleJsonValue(JsonToken token)
        {
            switch (token.TokenType)
            {
                case JsonTokenType.LITERAL_NULL:
                    return JsonNullValue.Null;
                case JsonTokenType.LITERAL_BOOLEAN:
                    return JsonBooleanValue.FromLiteral(token.Value);
                case JsonTokenType.VALUE_STRING:
                    return JsonStringValue.FromString(token.Value);
                case JsonTokenType.VALUE_NUMBER:
                    return JsonNumberValue.FromNumberLiteral(token.Value);
                default:
                    throw new JsonParserException($"Internal parser error: Unexpected JSON token \"{token}\"");
            }
        }

        /// <summary>
        ///     Ensures that the given <code>token</code> is not <code>null</code>
        /// </summary>
        /// <param name="token">the token to check for null</param>
        /// <param name="exceptionMessage">the message passed to <see cref="JsonParserException" /></param>
        /// <exception cref="JsonParserException">in case the given token is <code>null</code></exception>
        private void EnsureTokenIsNotNull(JsonToken token, string exceptionMessage)
        {
            if (token != null)
            {
                return;
            }

            state = JsonParserState.ERROR;
            throw new JsonParserException(exceptionMessage);
        }

        /// <summary>
        ///     Ensures that the value container stack's top element is ok such that a JSON array can be parsed.
        /// </summary>
        private void EnsureTopLevelElementIsAJsonArray()
        {
            EnsureValueContainerStackIsNotEmpty();

            var topLevelElement = valueContainerStack.First.Value;
            if (topLevelElement.JsonValue.ValueType != JsonValueType.ARRAY)
            {
                // sanity check, cannot happen, unless there is a programming error
                throw new JsonParserException(
                    InternalParserErrorMessage(state, "top level element is not a JSON array"));
            }

            if (topLevelElement.BackingList == null)
            {
                throw new JsonParserException(InternalParserErrorMessage(state, "backing list is null"));
            }
        }

        /// <summary>
        ///     Ensures that the value container stack's top element is OK such that a JSON object can be parsed.
        /// </summary>
        private void EnsureTopLevelElementIsAJsonObject()
        {
            EnsureValueContainerStackIsNotEmpty();

            var topLevelElement = valueContainerStack.First.Value;
            if (topLevelElement.JsonValue.ValueType != JsonValueType.OBJECT)
            {
                // sanity check, cannot happen unless there is a programming error
                throw new JsonParserException(InternalParserErrorMessage(state,
                    "top level element is not a JSON object"));
            }

            if (topLevelElement.BackingDict == null)
            {
                // sanity check, cannot happen, unless there is a programming error
                throw new JsonParserException(InternalParserErrorMessage(state, "backing map is null"));
            }
        }

        /// <summary>
        ///     Ensure that a key-value pair was parsed previously.
        /// </summary>
        private void EnsureKeyValuePairWasParsed()
        {
            EnsureValueContainerStackIsNotEmpty();

            var topLevelElement = valueContainerStack.First.Value;
            if (topLevelElement.LastParsedObjectKey == null)
            {
                // sanity check, cannot happen, unless there is a programming error
                throw new JsonParserException(InternalParserErrorMessage(state, "LastParsedObjectKey is null"));
            }

            if (topLevelElement.LastParsedObjectValue == null)
            {
                // sanity check, cannot happen, unless there is a programming error
                throw new JsonParserException(InternalParserErrorMessage(state, "LastParsedObjectValue is null"));
            }
        }

        /// <summary>
        ///     Helper method to ensure that value container stack is not empty.
        /// </summary>
        /// <exception cref="JsonParserException">in case <see cref="valueContainerStack" /> is empty</exception>
        private void EnsureValueContainerStackIsNotEmpty()
        {
            if (valueContainerStack.Count == 0)
            {
                throw new JsonParserException(InternalParserErrorMessage(state, "valueContainerStack is empty"));
            }
        }

        /// <summary>
        ///     Helper method for creating internal parser error text.
        /// </summary>
        /// <param name="parserState">current parser state</param>
        /// <param name="suffix">the suffix to append to the message</param>
        /// <returns></returns>
        private static string InternalParserErrorMessage(JsonParserState parserState, string suffix)
        {
            return $"Internal parser error: [state={parserState}] {suffix}";
        }

        /// <summary>
        ///     Helper method for creating unexpected token error text.
        /// </summary>
        /// <param name="token">the unexpected token</param>
        /// <param name="suffix">the suffix to append to the message</param>
        /// <returns></returns>
        private static string UnexpectedTokenErrorMessage(JsonToken token, string suffix)
        {
            return $"Unexpected token \"{token}\" {suffix}";
        }

        /// <summary>
        ///     Helper class storing the <see cref="JsonValue" /> and the appropriate backing container class, if it is a
        ///     composite object.
        /// </summary>
        private sealed class JsonValueContainer
        {
            /// <summary>
            ///     Backing dictionary which is non-null if and only if <code>jsonValue</code> is a <see cref="JsonObjectValue" />
            /// </summary>
            internal readonly IDictionary<string, JsonValue> BackingDict;

            /// <summary>
            ///     Backing list which is non-null if and only if <code>jsonValue</code> is a <see cref="JsonArrayValue" />
            /// </summary>
            internal readonly ICollection<JsonValue> BackingList;

            /// <summary>
            ///     The JSON value to store
            /// </summary>
            internal readonly JsonValue JsonValue;

            /// <summary>
            ///     Field to store the last parsed key of an object
            /// </summary>
            internal string LastParsedObjectKey;

            /// <summary>
            ///     Field to store the last parsed value of an object
            /// </summary>
            internal JsonValue LastParsedObjectValue;

            /// <summary>
            ///     Constructs a <see cref="JsonValueContainer" /> wit a JSON value
            ///     <para>
            ///         Any backing container classes are set to null, therefore only use this constructor for simple values.
            ///     </para>
            /// </summary>
            /// <param name="jsonValue">a simple <see cref="JsonValue" /> to initialize this container</param>
            internal JsonValueContainer(JsonValue jsonValue)
            {
                JsonValue = jsonValue;
                BackingList = null;
                BackingDict = null;
            }

            /// <summary>
            ///     Constructs a <see cref="JsonValueContainer" /> with a JSON array value.
            /// </summary>
            /// <param name="jsonArrayValue">the JSON array value</param>
            /// <param name="backingList">the backing list for the JSON array value.</param>
            internal JsonValueContainer(JsonArrayValue jsonArrayValue, ICollection<JsonValue> backingList)
            {
                JsonValue = jsonArrayValue;
                BackingList = backingList;
                BackingDict = null;
            }

            /// <summary>
            ///     Constructs a <see cref="JsonValueContainer" /> with a JSON object value.
            /// </summary>
            /// <param name="jsonObjectValue">the JSON object value.</param>
            /// <param name="backingDict">the backing map for the JSON object value.</param>
            internal JsonValueContainer(JsonObjectValue jsonObjectValue, IDictionary<string, JsonValue> backingDict)
            {
                JsonValue = jsonObjectValue;
                BackingList = null;
                BackingDict = backingDict;
            }
        }
    }
}
//
// Copyright 2018-2020 Dynatrace LLC
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

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Dynatrace.OpenKit.Core.Util
{
    internal class CrashFormatter
    {
        internal const string StackFramePrefix = "   at ";
        internal  const string InFileLineNum = "in {0}:line {1}";
        internal const string NewLine = "\n";

        private readonly Exception exception;

        internal CrashFormatter(Exception exception)
        {
            this.exception = exception;
        }

        internal string Name => exception.GetType().ToString();

        internal string Reason => exception.Message;

        internal string StackTrace => GetInvariantStackTrace();

        /// <summary>
        /// Utility method providing a culture invariant stack trace string.
        /// </summary>
        /// <remarks>
        /// Since <see cref="Exception.StackTrace"/> will translate certains
        /// words depending on the UI culture, an invariant version is required.
        /// The implementation is actually taken from the MS reference code, without
        /// the culture dependent features.
        /// </remarks>
        /// <returns>Culture invariant stack trace string</returns>
        private string GetInvariantStackTrace()
        {
#if NETSTANDARD1_1
            // required classes were introduced with .NET Standard 2.0
            return exception.StackTrace;
#else
            var stackTrace = new StackTrace(exception, true);
            var stackFrames = stackTrace.GetFrames();
            if (stackFrames == null || stackFrames.Length == 0)
            {
                return string.Empty;
            }

            var displayFilenames = true;   // we'll try, but demand may fail
            var firstFrame = true;
            var stackTraceBuilder = new StringBuilder();

            foreach(var stackFrame in stackFrames)
            {
                // ensure the current frame has a method
                var method = stackFrame.GetMethod();
                if (method == null)
                {
                    continue;
                }

                // append a newline for every stack frame, except the first one
                if (firstFrame)
                {
                    firstFrame = false;
                }
                else
                {
                    stackTraceBuilder.Append(NewLine);
                }

                // prefix for stack frame
                stackTraceBuilder.Append(StackFramePrefix);

                // append type name + method name
                var type = method.DeclaringType;
                if (type != null)
                {
                    stackTraceBuilder.Append(type.FullName.Replace('+', '.')).Append('.');
                }
                stackTraceBuilder.Append(method.Name);

                // deal with the generic portion of the method
                if (method is MethodInfo methodInfo && methodInfo.IsGenericMethod)
                {
                    stackTraceBuilder.Append("[");
                    var genericArguments = methodInfo.GetGenericArguments();
                    var firstGenericArgument = true;

                    foreach (var genericArgument in genericArguments)
                    {
                        if (!firstGenericArgument)
                        {
                            stackTraceBuilder.Append(",");
                        }
                        else
                        {
                            firstGenericArgument = false;
                        }

                        stackTraceBuilder.Append(genericArgument.Name);
                    }

                    stackTraceBuilder.Append("]");
                }

                // append method arguments
                stackTraceBuilder.Append("(");
                var parameters = method.GetParameters();
                var firstParameter = true;

                foreach (var parameter in parameters)
                {
                    if (!firstParameter)
                    {
                        stackTraceBuilder.Append(", ");
                    }
                    else
                    {
                        firstParameter = false;
                    }

                    stackTraceBuilder.Append(parameter.ParameterType?.Name ?? "<UnknownType>")
                        .Append(" ")
                        .Append(parameter.Name);
                }

                stackTraceBuilder.Append(")");

                // check if we can add a location
                if (!displayFilenames || stackFrame.GetILOffset() == -1)
                {
                    // don't  display filename - continue with next frame
                    continue;
                }

                string filename = null;
                try
                {
                    filename = stackFrame.GetFileName();
                }
                catch (Exception)
                {
                    // If the demand for displaying filenames fails, then it won't
                    // succeed later in the loop.  Avoid repeated exceptions by not trying again.
                    displayFilenames = false;
                }

                if (filename != null)
                {
                    // tack on " in c:\tmp\MyFile.cs:line 5"
                    stackTraceBuilder.Append(' ');
                    stackTraceBuilder.AppendFormat(CultureInfo.InvariantCulture, InFileLineNum, filename, stackFrame.GetFileLineNumber());
                }
            }

            return stackTraceBuilder.ToString();

#endif // !NETSTANDARD1_1
        }

    }
}

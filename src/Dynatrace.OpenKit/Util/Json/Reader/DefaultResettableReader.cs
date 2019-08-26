//
// Copyright 2018-2019 Dynatrace LLC
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
using System.IO;

namespace Dynatrace.OpenKit.Util.Json.Reader
{
    /// <summary>
    ///     Implements a reader which reads single characters from an underlying <see cref="TextReader"/>. A circular
    ///     buffer allows for <see cref="Mark">marking</see> and <see cref="Reset">resetting</see> resetting the reader
    ///     to this marked position.
    /// </summary>
    public class DefaultResettableReader : IResettableReader
    {
        /// <summary>
        ///     The underlying reader from which the actual characters are read.
        /// </summary>
        private readonly TextReader reader;

        /// <summary>
        ///     A circular buffer holding the last read characters from the underlying <see cref="reader"/>.
        /// </summary>
        private char[] buffer;

        /// <summary>
        ///     Specifies the current size of the <see cref="buffer"/>.
        /// </summary>
        private int bufferSize;

        /// <summary>
        ///     The index indicating the number of characters read by the <see cref="reader"/>
        /// </summary>
        private int readerIndex;

        /// <summary>
        ///     The absolute index indicating the position of the character to read next (either from the
        ///     <see cref="reader"/> or from the <see cref="buffer"/>)
        /// </summary>
        private int currentReadIndex;

        /// <summary>
        ///     The marked position to which this reader will be reset to when calling <see cref="Reset"/>.
        /// </summary>
        private int markedReaderIndex = -1;

        public DefaultResettableReader(string input) : this(new StringReader(input))
        {
        }

        public DefaultResettableReader(TextReader reader)
        {
            this.reader = reader;
        }

        public int Read()
        {
            if (CanReadFromBuffer())
            {
                var readIndex = ToRelativeBufferIndex(currentReadIndex++);
                return buffer[readIndex];
            }

            var chr = reader.Read();

            if (CanWriteToBuffer())
            {
                var writeIndex = ToRelativeBufferIndex(readerIndex);
                buffer[writeIndex] = (char) chr;
            }

            readerIndex++;
            currentReadIndex = readerIndex;

            return chr;
        }

        /// <summary>
        ///     Indicates whether reading from the buffer is possible or not.
        /// </summary>
        private bool CanReadFromBuffer()
        {
            if (markedReaderIndex < 0)
            {
                // buffer not initialized yet
                return false;
            }

            return currentReadIndex < readerIndex;
        }

        /// <summary>
        ///     Indicates whether writing to the buffer is possible or not.
        /// </summary>
        private bool CanWriteToBuffer()
        {
            return markedReaderIndex >= 0; // only when buffer was initialized.
        }

        /// <summary>
        ///     Calculates the relative index inside the <see cref="buffer"/> from the given absolute index of
        ///     the <see cref="reader"/>.
        /// </summary>
        /// <param name="absoluteBufferIndex">the absolute index to be converted to a relative index</param>
        /// <returns>the relative index inside the <see cref="buffer"/></returns>
        private int ToRelativeBufferIndex(int absoluteBufferIndex)
        {
            if (buffer == null)
            {
                // reader not marked yet
                return 0;
            }

            return ToRelativeBufferIndex(absoluteBufferIndex, buffer.Length);
        }

        /// <summary>
        ///     Calculates the relative index from the given absolute index.
        ///     <para>
        ///         Basically the index inside the buffer is the modulo of the buffer's size.
        ///     </para>
        /// </summary>
        /// <param name="absoluteBufferIndex">the absolute index to be converted in to a relative index</param>
        /// <param name="numBufferElements">the number of elements the buffer can hold</param>
        /// <returns></returns>
        private int ToRelativeBufferIndex(int absoluteBufferIndex, int numBufferElements)
        {
            return absoluteBufferIndex % numBufferElements;
        }

        public void Mark(int readAheadLimit)
        {
            if (readAheadLimit < 0)
            {
                throw new ArgumentException("readAheadLimit < 0");
            }

            InitializeOrResizeBuffer(readAheadLimit);

            markedReaderIndex = currentReadIndex;
        }

        /// <summary>
        ///     Initializes or increases buffer array with / to the given buffer size.
        ///     <para>
        ///         Calling this method will not decrease the size of the underlying array, in case the new buffer size
        ///         is smaller than the current size of the buffer.
        ///     </para>
        /// </summary>
        /// <param name="newBufferSize">the size to which the buffer will be resized to</param>
        private void InitializeOrResizeBuffer(int newBufferSize)
        {
            if (buffer == null)
            {
                // first time initialization
                buffer = new char[newBufferSize];
                bufferSize = newBufferSize;
                return;
            }

            if (newBufferSize <= buffer.Length)
            {
                // enough space available
                bufferSize = newBufferSize;
                return;
            }

            // resize the buffer
            var resizedBuffer = CreateResizedBufferCopy(newBufferSize);

            buffer = resizedBuffer;
            bufferSize = newBufferSize;
        }

        /// <summary>
        ///     Creates copy of the current <see cref="buffer"/> and resizes it to the given buffer size. The content
        ///     of the buffer is copies so that the position of <see cref="currentReadIndex"/> is kept the same
        /// </summary>
        /// <param name="newBufferSize">the size of the new buffer</param>
        /// <returns>a resized copy of the existing buffer</returns>
        private char[] CreateResizedBufferCopy(int newBufferSize)
        {
            var resizedBuffer = new char[newBufferSize];
            var relativeBufferIndex = ToRelativeBufferIndex(currentReadIndex);

            // calculate the new relative index inside the resized buffer and copy the content of the current buffer
            // to this index.
            var relativeResizedBufferIndex = ToRelativeBufferIndex(currentReadIndex, newBufferSize);

            // copy everything from current index (a) to current buffer end
            // buffer:         | c | d | a | b |
            // resized buffer: |   |   | a | b |   |
            var numCharactersToBufferEnd = buffer.Length - relativeBufferIndex;
            Array.Copy(buffer, relativeBufferIndex, resizedBuffer, relativeResizedBufferIndex, numCharactersToBufferEnd);

            // fill up the resized buffer to its end
            // buffer:         | c | d | a | b |
            // resized buffer: |   |   | a | b | c |
            var numCharactersToResizedBufferEnd = Math.Min(relativeBufferIndex, resizedBuffer.Length - buffer.Length);
            Array.Copy(buffer, 0, resizedBuffer, relativeResizedBufferIndex + numCharactersToBufferEnd, numCharactersToResizedBufferEnd);

            // copy the rest of the wrapped characters
            // buffer:         | c | d | a | b |
            // resized buffer: | d |   | a | b | c |
            Array.Copy(buffer, numCharactersToBufferEnd, resizedBuffer, 0,
                relativeBufferIndex - numCharactersToResizedBufferEnd);

            return resizedBuffer;
        }

        /// <summary>
        ///     Resets this reader to the previously <see cref="Mark">marked</see> position.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     In case no position was marked yet or the number of characters to reset exceeds the previously specified
        ///     look ahead limit when the reset position was <see cref="Mark">marked</see>.
        /// </exception>
        public void Reset()
        {
            if (markedReaderIndex < 0)
            {
                throw new InvalidOperationException("No position has been marked");
            }

            var numCharactersToReset = currentReadIndex - markedReaderIndex;
            if (numCharactersToReset > bufferSize)
            {
                throw new InvalidOperationException(
                    $"Cannot reset beyond {bufferSize} positions. Tried to reset {numCharactersToReset} positions");
            }

            currentReadIndex = markedReaderIndex;
        }

        public void Close()
        {
            reader.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                reader.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
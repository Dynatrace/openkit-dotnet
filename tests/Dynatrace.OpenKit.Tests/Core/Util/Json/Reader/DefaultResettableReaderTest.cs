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

using System;
using Dynatrace.OpenKit.Util.Json.Reader;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util.Json.Reader
{
    public class DefaultResettableReaderTest
    {
        [Test]
        public void ReadSingleCharacter()
        {
            // given
            var target = new DefaultResettableReader("Hello");

            // when
            var obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('H'));
        }

        [Test]
        public void ReadAllCharacters()
        {
            // given
            const string input = "Hello";
            var target = new DefaultResettableReader(input);

            foreach (var chr in input)
            {
                // when
                var obtained = target.Read();

                // then
                Assert.That(obtained, Is.EqualTo(chr));
            }
        }

        [Test]
        public void ReadAfterMarkAndResettingSingleCharacter()
        {
            // given
            var target = new DefaultResettableReader("abc");

            // when
            target.Mark(1);
            var obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('a'));

            // and when
            target.Reset();
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('a'));

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('b'));

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('c'));

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo(-1));
        }

        [Test]
        public void ReadAfterMarkAndResettingTwoCharactersWithEvenReadOffset()
        {
            // given
            const string input = "abc";
            var target = new DefaultResettableReader(input);

            // when
            target.Mark(2);
            var obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('a'));

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('b'));

            // and when
            target.Reset();

            foreach (var chr in input)
            {
                // when
                obtained = target.Read();

                // then
                Assert.That(obtained, Is.EqualTo(chr));
            }

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo(-1));
        }

        [Test]
        public void ReadAfterMarkAndResettingTwoCharactersWithOddReadOffset()
        {
            // given
            const string input = "abcd";
            var target = new DefaultResettableReader(input);

            // when
            var obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('a'));

            // and when
            target.Mark(2);
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('b'));

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('c'));

            // and when
            target.Reset();

            for (var i = 1; i < input.Length; i++)
            {
                // when
                obtained = target.Read();

                // then
                Assert.That(obtained, Is.EqualTo(input[i]));
            }

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo(-1));
        }

        [Test]
        public void MarkWithLowerReadAheadLimitThanPreviousMark()
        {
            // given
            const string input = "abcd";
            var target = new DefaultResettableReader(input);

            // when
            target.Mark(2);

            // then
            for (var i = 0; i < 2; i++)
            {
                var chr = target.Read();
                Assert.That(chr, Is.EqualTo(input[i]));
            }

            // and when
            target.Reset();

            for (var i = 0; i < 3; i++)
            {
                // and when
                target.Mark(1);
                var obtained = target.Read();

                // then
                Assert.That(obtained, Is.EqualTo(input[i]));

                // and when
                target.Reset();
                obtained = target.Read();

                // then
                Assert.That(obtained, Is.EqualTo(input[i]));
            }

            // and when reading to end, then
            for (var i = 3; i < input.Length; i++)
            {
                var obtained = target.Read();
                Assert.That(obtained, Is.EqualTo(input[i]));
            }

            // and finally when
            var lastReadChr = target.Read();

            // then
            Assert.That(lastReadChr, Is.EqualTo(-1));
        }

        [Test]
        public void MarkWithHigherReadAheadLimitThanPreviousMarkOnEvenPosition()
        {
            // given
            const string input = "abcde";
            var target = new DefaultResettableReader(input);

            // when
            target.Mark(1);
            var obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('a'));

            // and when reading from reset reader at even position
            target.Reset();
            target.Mark(2);
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('a'));

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('b'));

            // and when
            target.Reset();

            // then
            foreach (var c in input)
            {
                obtained = target.Read();
                Assert.That(obtained, Is.EqualTo(c));
            }

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo(-1));
        }

        [Test]
        public void MarkWithHigherReadAheadLimitThanPreviousMarkOnOddPosition()
        {
            // given
            const string input = "abcde";
            var target = new DefaultResettableReader(input);

            // when
            target.Mark(1);
            var obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('a'));

            // and when
            target.Mark(1);
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('b'));

            // and when reading from reset reader at odd position
            target.Reset();
            target.Mark(2);
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('b'));

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('c'));

            // and when
            target.Reset();

            // then
            for (var i = 1; i < input.Length; i++)
            {
                obtained = target.Read();
                Assert.That(obtained, Is.EqualTo(input[i]));
            }

            // and when
            obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo(-1));
        }

        [Test]
        public void MarkAndReadAfterNotReadingAllPreviouslyMarkedCharacters()
        {
            // given
            const string input = "abcdef";
            var target = new DefaultResettableReader(input);

            // when
            target.Mark(2);
            var obtained = target.Read();

            // then
            Assert.That(obtained, Is.EqualTo('a'));

            // and when
            target.Mark(3);

            // then
            for (var i = 1; i < 4; i++)
            {
                obtained = target.Read();
                Assert.That(obtained, Is.EqualTo(input[i]));
            }

            // and when
            target.Reset();

            // then
            for (var i = 1; i < input.Length; i++)
            {
                obtained = target.Read();
                Assert.That(obtained, Is.EqualTo(input[i]));
            }

            obtained = target.Read();
            Assert.That(obtained, Is.EqualTo(-1));
        }

        [Test]
        public void ResetAfterReadAheadLimitExceededThrowsException()
        {
            // given
            var target = new DefaultResettableReader("abcd");

            // when
            target.Mark(1);

            target.Read();
            target.Read();

            // then
            var ex = Assert.Throws<InvalidOperationException>(() => target.Reset());
            Assert.That(ex.Message, Is.EqualTo("Cannot reset beyond 1 positions. Tried to reset 2 positions"));
        }

        [Test]
        public void ResetWhenReadAheadLimitExceededAfterMarkAndResetThrowsException()
        {
            // given
            const string input = "abcdefgh";
            var target = new DefaultResettableReader(input);

            // when
            target.Mark(5);

            // then
            for (var i = 0; i < 5; i++)
            {
                var chr = target.Read();
                Assert.That(chr, Is.EqualTo(input[i]));
            }

            // and when
            target.Reset();
            target.Mark(3);

            // then
            for (var i = 0; i < 4; i++)
            {
                var chr = target.Read();
                Assert.That(chr, Is.EqualTo(input[i]));
            }

            var ex = Assert.Throws<InvalidOperationException>(() => target.Reset());
            Assert.That(ex.Message, Is.EqualTo("Cannot reset beyond 3 positions. Tried to reset 4 positions"));
        }

        [Test]
        public void ResetOnUnmarkedReaderThrowsAnException()
        {
            // given
            var target = new DefaultResettableReader("abcd");

            // when, then
            var ex = Assert.Throws<InvalidOperationException>(() => target.Reset());
            Assert.That(ex.Message, Is.EqualTo("No position has been marked"));
        }

        [Test]
        public void MarkWithNegativeLookAheadLimitThrowsException()
        {
            // given
            var target = new DefaultResettableReader("abcd");

            // when, then
            var ex = Assert.Throws<ArgumentException>(() => target.Mark(-1));
            Assert.That(ex.Message, Is.EqualTo("readAheadLimit < 0"));
        }
    }
}
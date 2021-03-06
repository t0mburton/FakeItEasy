namespace FakeItEasy.Tests.Core
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy.Core;
    using FakeItEasy.Tests.TestHelpers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class FakeAsserterTests
    {
        private List<IFakeObjectCall> calls;
        private CallWriter callWriter;

        [SetUp]
        public void Setup()
        {
            this.calls = new List<IFakeObjectCall>();
            this.callWriter = A.Fake<CallWriter>();
        }

        [Test]
        public void AssertWasCalled_should_pass_when_the_repeatPredicate_returns_true_for_the_number_of_matching_calls()
        {
            this.StubCalls(2);

            var asserter = this.CreateAsserter();

            asserter.AssertWasCalled(x => true, string.Empty, x => x == 2, string.Empty);
        }

        [Test]
        public void AssertWasCalled_should_fail_when_the_repeatPredicate_returns_false_fro_the_number_of_matching_calls()
        {
            this.StubCalls(2);

            var asserter = this.CreateAsserter();

            var exception = Record.Exception(() => asserter.AssertWasCalled(x => true, string.Empty, x => false, string.Empty));
            exception.Should().BeAnExceptionOfType<ExpectationException>();
        }

        [Test]
        public void AssertWasCalled_should_pass_the_number_of_matching_calls_to_the_repeatPredicate()
        {
            int? numberPassedToRepeatPredicate = null;

            this.StubCalls(4);

            var asserter = this.CreateAsserter();

            asserter.AssertWasCalled(x => this.calls.IndexOf(x) == 0, string.Empty, x => { numberPassedToRepeatPredicate = x; return true; }, string.Empty);

            numberPassedToRepeatPredicate.Should().Be(1);
        }

        [Test]
        public void Exception_message_should_start_with_call_specification()
        {
            var asserter = this.CreateAsserter();

            var message = this.GetExceptionMessage(() =>
                asserter.AssertWasCalled(x => true, @"IFoo.Bar(1)", x => false, string.Empty));
            var expectedMessage =
@"

  Assertion failed for the following call:
    IFoo.Bar(1)";

            message.Should().StartWith(expectedMessage);
        }

        [Test]
        public void Exception_message_should_write_repeat_expectation()
        {
            this.StubCalls(2);

            var asserter = this.CreateAsserter();

            var message = this.GetExceptionMessage(() =>
                asserter.AssertWasCalled(x => false, string.Empty, x => x == 2, "#2 times"));

            var expectedMessage =
@"
  Expected to find it #2 times but found it #0 times among the calls:";

            message.Should().Contain(expectedMessage);
        }

        [Test]
        public void Exception_message_should_call_the_call_writer_to_append_calls_list()
        {
            this.StubCalls(2);

            var asserter = this.CreateAsserter();

            this.GetExceptionMessage(() => asserter.AssertWasCalled(x => false, string.Empty, x => false, string.Empty));

            A.CallTo(() => this.callWriter.WriteCalls(A<IEnumerable<IFakeObjectCall>>.That.IsThisSequence(this.calls), A<IOutputWriter>._)).MustHaveHappened();
        }

        [Test]
        public void Exception_message_should_write_that_no_calls_were_made_when_calls_is_empty()
        {
            this.calls.Clear();

            var asserter = this.CreateAsserter();

            var message = this.GetExceptionMessage(() =>
                asserter.AssertWasCalled(x => false, string.Empty, x => x == 2, "#2 times"));

            var expectedMessage =
@"
  Expected to find it #2 times but no calls were made to the fake object.";

            message.Should().Contain(expectedMessage);
        }

        [Test]
        public void Exception_message_should_end_with_blank_line()
        {
            var asserter = this.CreateAsserter();

            var message = this.GetExceptionMessage(() =>
                asserter.AssertWasCalled(x => false, string.Empty, x => false, string.Empty));

            message.Should().EndWith(string.Concat(Environment.NewLine, Environment.NewLine));
        }

        [Test]
        public void Exception_message_should_start_with_blank_line()
        {
            var asserter = this.CreateAsserter();

            var message = this.GetExceptionMessage(() =>
                asserter.AssertWasCalled(x => false, string.Empty, x => false, string.Empty));

            message.Should().StartWith(Environment.NewLine);
        }

        private FakeAsserter CreateAsserter()
        {
            return new FakeAsserter(this.calls, this.callWriter);
        }

        private void StubCalls(int numberOfCalls)
        {
            for (int i = 0; i < numberOfCalls; i++)
            {
                this.calls.Add(A.Fake<IFakeObjectCall>());
            }
        }

        private string GetExceptionMessage(Action failingAssertion)
        {
            var exception = Record.Exception(failingAssertion);
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ExpectationException>();
            return exception.Message;
        }
    }
}

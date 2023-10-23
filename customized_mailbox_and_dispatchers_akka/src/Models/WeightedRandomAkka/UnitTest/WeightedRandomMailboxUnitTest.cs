using Xunit;
using Akka.Actor;
using System;
using customized_mailbox_and_dispatchers_akka.src.Models.WeightedRandomAkka;
using Akka.Dispatch.MessageQueues;
using Akka.Event;
using Moq;
using Akka.Actor.Internal;

namespace WeightedRandomAkka.Tests
{
    public class WeightedRandomMailboxTests
    {
        private readonly DummyMessageQueue _messageQueue = new DummyMessageQueue();
        private readonly WeightedRandomMailbox _mailbox;

        public WeightedRandomMailboxTests()
        {
            _mailbox = new WeightedRandomMailbox(_messageQueue);
        }

        /// <summary>
        /// Test to check the enqueue functionality of the mailbox.
        /// </summary>
        [Fact]
        public void Enqueue_ShouldAddMessageToQueue()
        {
            var envelope = new Envelope(new TestWeightedMessage(10), ActorRefs.NoSender);

            _mailbox.Enqueue(envelope);

            Assert.True(_mailbox.HasMessages);
        }

        /// <summary>
        /// Test to check the dequeue functionality of the mailbox based on weights.
        /// </summary>
        [Fact]
        public void Dequeue_ShouldReturnMessageBasedOnWeight()
        {
            var envelope1 = new Envelope(new TestWeightedMessage(10), ActorRefs.NoSender);
            var envelope2 = new Envelope(new TestWeightedMessage(90), ActorRefs.NoSender);

            int countHighWeight = 0;
            for (int i = 0; i < 100; i++)
            {
                _mailbox.Enqueue(envelope1);
                _mailbox.Enqueue(envelope2);
                if (_mailbox.TryDequeue(out var dequeuedEnvelope) && dequeuedEnvelope.Message is TestWeightedMessage message && message.Weight == 90)
                {
                    countHighWeight++;
                }
                _mailbox.CleanUp();
            }

            Assert.True(countHighWeight > 50); // Assuming the weighted random is fair, we should get the higher weighted message more often.
        }

        /// <summary>
        /// Test to check the cleanup functionality of the mailbox.
        /// </summary>
        [Fact]
        public void CleanUp_ShouldClearAllMessages()
        {
            var envelope = new Envelope(new TestWeightedMessage(10), ActorRefs.NoSender);

            _mailbox.Enqueue(envelope);
            _mailbox.CleanUp(null, false);

            Assert.False(_mailbox.HasMessages);
        }

        public class TestWeightedMessage : IWeightedMessage
        {
            public double Weight { get; }

            public TestWeightedMessage(double weight)
            {
                Weight = weight;
            }
        }

        public class DummyMessageQueue : IMessageQueue
        {
            // A list to hold envelopes for the dummy implementation
            private readonly List<Envelope> _envelopes = new List<Envelope>();

            // Enqueue a message onto the queue
            public void Enqueue(IActorRef receiver, Envelope envelope)
            {
                _envelopes.Add(envelope);
            }

            // Try to dequeue a message from the queue
            public bool TryDequeue(out Envelope envelope)
            {
                if (_envelopes.Count > 0)
                {
                    envelope = _envelopes[0];
                    _envelopes.RemoveAt(0);
                    return true;
                }
                envelope = default(Envelope);
                return false;
            }

            // Check if the queue has any messages
            public bool HasMessages => _envelopes.Count > 0;

            // Get the number of messages in the queue
            public int Count => _envelopes.Count;
            public void CleanUp(IActorRef actor, IMessageQueue deadLetters)
            {
                foreach (var envelope in _envelopes)
                {
                    deadLetters.Enqueue(actor, envelope);
                }
                _envelopes.Clear();
            }

        }
    }
}

using Akka.Actor;
using Akka.Dispatch;
using Akka.Dispatch.MessageQueues;
using System;
using System.Collections.Generic;
using System.Linq;

namespace customized_mailbox_and_dispatchers_akka.src.Models.WeightedRandomAkka
{
    public class WeightedRandomMailbox : Mailbox
    {
        private readonly Random _random = new Random();
        private readonly LinkedList<Envelope> _queue = new LinkedList<Envelope>();

        public WeightedRandomMailbox(IMessageQueue messageQueue) : base(messageQueue)
        {
        }

        public void EnqueueFirst(in Envelope envelope)
        {
            _queue.AddFirst(envelope);
        }

        public void Enqueue(in Envelope envelope)
        {
            _queue.AddLast(envelope);
        }

        public bool HasMessages => _queue.Count > 0;

        public bool TryDequeue(out Envelope envelope)
        {
            if (_queue.Count > 0) 
            {
                envelope = Dequeue(); 
                return true;
            }
            envelope = default(Envelope);
            return false;
        }

        protected Envelope Dequeue()
        {
            if (_queue.Count == 0) return default(Envelope);

            var totalWeight = _queue.OfType<Envelope>().Where(e => e.Message is IWeightedMessage).Sum(e => ((IWeightedMessage)e.Message).Weight);
            var randomWeight = _random.NextDouble() * totalWeight;

            double cumulativeWeight = 0;
            foreach (var envelope in _queue)
            {
                if (envelope.Message is IWeightedMessage weightedMessage)
                {
                    cumulativeWeight += weightedMessage.Weight;
                    if (cumulativeWeight >= randomWeight)
                    {
                        _queue.Remove(envelope);
                        return envelope;
                    }
                }
            }

            return default(Envelope);
        }

        public void CleanUp(ActorCell owner, bool aboutToTerminate)
        {
            if (owner?.System?.DeadLetters != null) {
                foreach (var envelope in _queue)
                {
                    owner.System.DeadLetters.Tell(envelope);
                }
            }
            _queue.Clear();
        }
    }

}

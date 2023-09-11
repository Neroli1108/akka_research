using Akka.Actor;

namespace customized_mailbox_and_dispatchers_akka.src.Models.PriorityBasedAkka.PriorityQueueEnvelope
{
    public class PriorityQueueEnvelope
    {
        public Envelope Envelope { get; set; }
        public int Priority { get; set; }
    }
    public class PriorityQueue
    {
        private List<PriorityQueueEnvelope> _data = new List<PriorityQueueEnvelope>();

        public void Enqueue(Envelope item, int priority)
        {
            _data.Add(new PriorityQueueEnvelope { Envelope = item, Priority = priority });
            _data = _data.OrderBy(d => d.Priority).ToList();
        }

        public Envelope Dequeue()
        {
            var frontItem = _data.First();
            _data.RemoveAt(0);
            return frontItem.Envelope;
        }

        public bool Any() => _data.Any();
        public int Count => _data.Count;

    }
}

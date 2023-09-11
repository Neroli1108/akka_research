namespace customized_mailbox_and_dispatchers_akka.src.models.random_akka.TimestampedEnvelope
{
    public class TimeBucket
    {
        private readonly TimeSpan _windowSize;
        private List<TimestampedEnvelope> _messages = new List<TimestampedEnvelope>();

        public TimeBucket(TimeSpan windowSize)
        {
            _windowSize = windowSize;
        }

        public void AddMessage(TimestampedEnvelope message)
        {
            _messages.Add(message);
            _messages.RemoveAll(m => (DateTime.Now - m.ReceivedAt) > _windowSize);
        }

        public TimestampedEnvelope GetRandomMessage()
        {
            var randomIndex = new Random().Next(_messages.Count);
            var selectedMessage = _messages[randomIndex];
            _messages.RemoveAt(randomIndex);
            return selectedMessage;
        }

        public bool IsEmpty => !_messages.Any();
    }
}

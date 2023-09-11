using Akka.Actor;

namespace customized_mailbox_and_dispatchers_akka.src.models.random_akka.TimestampedEnvelope
{
    public class TimestampedEnvelope
    {
        public Envelope Envelope { get; }
        public DateTime ReceivedAt { get; }

        public TimestampedEnvelope(Envelope envelope, DateTime receivedAt)
        {
            Envelope = envelope;
            ReceivedAt = receivedAt;
        }
    }

}

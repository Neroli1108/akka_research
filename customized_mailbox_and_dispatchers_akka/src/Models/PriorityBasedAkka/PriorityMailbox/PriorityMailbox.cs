using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch.MessageQueues;
using Akka.Dispatch;
using customized_mailbox_and_dispatchers_akka.src.Models.PriorityBasedAkka.PriorityQueueEnvelope;
using customized_mailbox_and_dispatchers_akka.src.Models.PriorityBasedAkka.Interface;


namespace customized_mailbox_and_dispatchers_akka.src.Models.PriorityBasedAkka.PriorityMailbox
{
    public class PriorityMailbox : MailboxType, IMessageQueue
    {
        private readonly PriorityQueue _queue = new PriorityQueue();

        public PriorityMailbox(Settings settings, Config config) : base(settings, config)
        {
        }

        public bool HasMessages => _queue.Any();
        public int Count => _queue.Count;

        public override IMessageQueue Create(IActorRef owner, ActorSystem system)
        {
            return this;
        }

        public void Enqueue(IActorRef receiver, Envelope envelope)
        {
            if (envelope.Message is IPriorityMessage priorityMsg)
            {
                _queue.Enqueue(envelope, priorityMsg.Priority);
            }
            else
            {
                _queue.Enqueue(envelope, int.MaxValue);  // Default priority
            }
        }

        public bool TryDequeue(out Envelope? envelope)
        {
            if (_queue.Any())
            {
                envelope = _queue.Dequeue();
                return true;
            }
            envelope = null;
            return false;
        }

        public void CleanUp(IActorRef owner, IMessageQueue deadletters)
        {
            while (_queue.Any())
            {
                deadletters.Enqueue(owner, _queue.Dequeue());
            }
        }
    }
}

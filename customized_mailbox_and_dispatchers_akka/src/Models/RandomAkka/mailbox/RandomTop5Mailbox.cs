using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using Akka.Dispatch.MessageQueues;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;

public class RandomTop5Mailbox : MailboxType, IMessageQueue
{
    public RandomTop5Mailbox(Settings settings, Config config) : base(settings, config)
    {
    }

    public bool HasMessages => _messages.Any();
    public int Count => _messages.Count;

    public override IMessageQueue Create(IActorRef owner, ActorSystem system)
    {
        // Returns this instance as the message queue
        return this;
    }

    public void Enqueue(IActorRef receiver, Envelope envelope)
    {
        // Record the receive order and timestamp
        LogMessageReceipt(envelope, DateTime.UtcNow);

        _messages.Add(envelope);
    }

    public bool TryDequeue(out Envelope envelope)
    {
        envelope = default(Envelope);

        if (!_messages.Any())
            return false;

        int topLimit = Math.Min(5, _messages.Count);
        var randomIndex = new Random().Next(topLimit);
        envelope = _messages[randomIndex];
        _messages.RemoveAt(randomIndex);

        // Record the dequeue timestamp
        LogMessageDequeue(envelope, DateTime.UtcNow);

        return true;
    }

    private readonly List<Envelope> _messages = new List<Envelope>();

    private void LogMessageReceipt(Envelope envelope, DateTime timestamp)
    {
        // Implement logging logic here, e.g., system.Log.Info(...)
    }

    private void LogMessageDequeue(Envelope envelope, DateTime timestamp)
    {
        // Implement logging logic here
    }

    public void CleanUp(IActorRef owner, IMessageQueue deadLetters)
    {
        foreach (var message in _messages)
        {
            deadLetters.Enqueue(owner, message);
        }
        _messages.Clear();
    }
}

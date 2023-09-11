using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using Akka.Dispatch.MessageQueues;
using Akka.Event;
using Akka.Util;
using System;
using System.Collections.Generic;
using System.Linq;

public class RandomTimeBucketMailbox : MailboxType, IMessageQueue
{
    public RandomTimeBucketMailbox(Settings settings, Config config) : base(settings, config)
    {
    }

    public bool HasMessages => _buckets.Any(bucket => bucket.Messages.Any());
    public int Count => _buckets.Sum(bucket => bucket.Messages.Count);

    public override IMessageQueue Create(IActorRef owner, ActorSystem system)
    {
        // Returns this instance as the message queue
        return this;
    }
    public void CleanUp(IActorRef owner, IMessageQueue deadLetters)
    {
        var removedEnvelopes = new List<Envelope>();
        var now = DateTime.UtcNow;

        // Find all the buckets whose messages are beyond the sliding window
        foreach (var bucket in _buckets.Where(bucket => (now - bucket.LastReceived).TotalSeconds > SlidingWindowSeconds))
        {
            removedEnvelopes.AddRange(bucket.Messages);
            bucket.Messages.Clear();
        }

        // Remove all buckets that are empty
        _buckets.RemoveAll(bucket => !bucket.Messages.Any());

        // Send all removed envelopes to deadLetters
        foreach (var envelope in removedEnvelopes)
        {
            deadLetters.Enqueue(owner, envelope);
        }
    }



    public void Enqueue(IActorRef receiver, Envelope envelope)
    {
        var now = DateTime.UtcNow;

        // Record the receive order and timestamp
        LogMessageReceipt(envelope, now);

        // Add to the appropriate bucket
        var suitableBucket = _buckets.FirstOrDefault(b => (now - b.FirstReceived).TotalSeconds <= SlidingWindowSeconds);

        if (suitableBucket == null)
        {
            suitableBucket = new MessageBucket { FirstReceived = now };
            _buckets.Add(suitableBucket);
        }

        suitableBucket.Messages.Add(envelope);
        suitableBucket.LastReceived = now;

        // Ensure buckets are valid after addition
        ValidateBuckets();
    }

    public bool TryDequeue(out Envelope envelope)
    {
        envelope = default(Envelope);

        if (!_buckets.Any())
            return false;

        var firstBucket = _buckets.First();

        if (!firstBucket.Messages.Any())
            return false;

        // Randomly select a message from the first bucket
        var randomIndex = new Random().Next(firstBucket.Messages.Count);
        envelope = firstBucket.Messages[randomIndex];
        firstBucket.Messages.RemoveAt(randomIndex);

        // Record the dequeue timestamp
        LogMessageDequeue(envelope, DateTime.UtcNow);

        // If bucket is empty, remove it
        if (!firstBucket.Messages.Any())
            _buckets.RemoveAt(0);

        return true;
    }


    private const int SlidingWindowSeconds = 10; // Define as needed
    private readonly List<MessageBucket> _buckets = new List<MessageBucket>();

    private void LogMessageReceipt(Envelope envelope, DateTime timestamp)
    {
        // Implement logging logic here, e.g., system.Log.Info(...)
    }

    private void LogMessageDequeue(Envelope envelope, DateTime timestamp)
    {
        // Implement logging logic here
    }

    private void ValidateBuckets()
    {
        // Implement logic to split or merge buckets as needed based on the sliding window
    }

    private class MessageBucket
    {
        public DateTime FirstReceived { get; set; }
        public DateTime LastReceived { get; set; }
        public List<Envelope> Messages { get; } = new List<Envelope>();
    }
}

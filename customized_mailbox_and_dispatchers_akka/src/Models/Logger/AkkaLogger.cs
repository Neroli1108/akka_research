using Akka.Actor;
using System;
using System.Collections.Generic;

public class LogMessage
{
    public LogMessage(IActorRef actor, string message)
    {
        Actor = actor;
        Message = message;
        Timestamp = DateTime.UtcNow;
    }

    public IActorRef Actor { get; }
    public string Message { get; }
    public DateTime Timestamp { get; }
}

public class LoggerActor : ReceiveActor
{
    private Dictionary<IActorRef, List<LogMessage>> _logsByActor = new Dictionary<IActorRef, List<LogMessage>>();

    public LoggerActor()
    {
        Receive<LogMessage>(logMessage =>
        {
            if (!_logsByActor.ContainsKey(logMessage.Actor))
            {
                _logsByActor[logMessage.Actor] = new List<LogMessage>();
            }

            _logsByActor[logMessage.Actor].Add(logMessage);
            Console.WriteLine($"[{logMessage.Timestamp}] Actor {logMessage.Actor.Path.Name}: {logMessage.Message}");
        });
    }
}

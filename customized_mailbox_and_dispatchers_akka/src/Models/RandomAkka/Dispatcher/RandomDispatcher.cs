using System.Collections.Generic;
using System;

public class RandomTop5Dispatcher : TaskDispatcher
{
	public RandomTop5Dispatcher(string id, string throughput, string throughputDeadlineTime, string mailboxRequirement, string configPath) : base(id, throughput, throughputDeadlineTime, mailboxRequirement, configPath)
	{
	}

	protected override IActorRef SelectNextActor(IList<IActorRef> schedule)
	{
		int topLimit = Math.Min(5, schedule.Count);
		return schedule[new Random().Next(topLimit)];
	}
}

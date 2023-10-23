using Xunit;
using Akka.Actor;
using Akka.Dispatch;
using Moq;
using Akka.Configuration;
using customized_mailbox_and_dispatchers_akka.src.Models.RandomAkka;
using customized_mailbox_and_dispatchers_akka.src.Models.PriorityBasedAkka.Interface;
using customized_mailbox_and_dispatchers_akka.src.Models.PriorityBasedAkka.PriorityMailbox;

namespace customized_mailbox_and_dispatchers_akka.src.Models.RandomAkka.UnitTest
{
    public class PriorityMailboxTests : IDisposable
    {
        private readonly Mock<IActorRef> _actorRefMock;
        private readonly ActorSystem _actorSystem;
        private readonly Config _config;  // Use a real config

        public PriorityMailboxTests()
        {
            _actorRefMock = new Mock<IActorRef>();
            _actorSystem = ActorSystem.Create("testSystem");
            _config = ConfigurationFactory.ParseString("");  // You can provide specific configuration string if needed
        }

        [Fact]
        public void Enqueue_ShouldPrioritize_IPriorityMessage()
        {
            // Arrange
            var settings = new Settings(_actorSystem, _config);  // Use the real config
            var mailbox = new PriorityMailbox(settings, _config);

            var normalMessage = new object();
            var priorityMessage = new Mock<IPriorityMessage>();
            priorityMessage.Setup(p => p.Priority).Returns(1);

            // Act
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(normalMessage, _actorRefMock.Object));
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(priorityMessage.Object, _actorRefMock.Object));

            mailbox.TryDequeue(out Envelope firstDequeued);
            mailbox.TryDequeue(out Envelope secondDequeued);

            // Assert
            Assert.IsAssignableFrom<IPriorityMessage>(firstDequeued.Message);
            Assert.Equal(normalMessage, secondDequeued.Message);
        }

        public void Dispose()
        {
            _actorSystem.Terminate().Wait();
        }
    }
}

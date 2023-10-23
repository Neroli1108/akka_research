using Xunit;
using Akka.Actor;
using Akka.Dispatch;
using customized_mailbox_and_dispatchers_akka.src.Models;
using Moq;
using Akka.Configuration;

namespace WeightedRandomAkka.Tests
{
    public class RandomTop5MailboxTests : IDisposable
    {
        private readonly Mock<IActorRef> _actorRefMock;
        private readonly ActorSystem _actorSystem;
        private readonly Config _config;

        public RandomTop5MailboxTests()
        {
            _actorRefMock = new Mock<IActorRef>();
            _actorSystem = ActorSystem.Create("TestActorSystem");
            _config = ConfigurationFactory.Empty;  // Creating an empty Akka configuration
        }

        public void Dispose()
        {
            _actorSystem.Terminate().Wait();
        }

        private Settings CreateRealSettings()
        {
            return new Settings(_actorSystem, _config);
        }

        [Fact]
        public void HasMessages_ShouldReturnFalse_WhenNoMessages()
        {
            var settings = CreateRealSettings();
            var mailbox = new RandomTop5Mailbox(settings, _config);
            Assert.False(mailbox.HasMessages);
        }

        [Fact]
        public void HasMessages_ShouldReturnTrue_WhenMessagesExist()
        {
            var settings = CreateRealSettings();
            var mailbox = new RandomTop5Mailbox(settings, _config);
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(new object(), _actorRefMock.Object));
            Assert.True(mailbox.HasMessages);
        }

        [Fact]
        public void Count_ShouldReturnCorrectMessageCount()
        {
            var settings = CreateRealSettings();
            var mailbox = new RandomTop5Mailbox(settings, _config);
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(new object(), _actorRefMock.Object));
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(new object(), _actorRefMock.Object));
            Assert.Equal(2, mailbox.Count);
        }

        [Fact]
        public void TryDequeue_ShouldReturnMessage_WhenMessagesExist()
        {
            var settings = CreateRealSettings();
            var mailbox = new RandomTop5Mailbox(settings, _config);
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(new object(), _actorRefMock.Object));

            bool result = mailbox.TryDequeue(out Envelope dequeuedEnvelope);

            Assert.True(result);
            Assert.NotNull(dequeuedEnvelope.Message);
        }
    }
}

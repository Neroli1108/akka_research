using Xunit;
using Akka.Actor;
using Akka.Dispatch;
using Moq;
using Akka.Configuration;

namespace customized_mailbox_and_dispatchers_akka.src.Models.RandomAkka.UnitTest
{
    public class RandomTimeBucketMailboxTests
    {
        private readonly Mock<IActorRef> _actorRefMock;
        private readonly ActorSystem _actorSystem; // Using real instance

        public RandomTimeBucketMailboxTests()
        {
            _actorRefMock = new Mock<IActorRef>();
            _actorSystem = ActorSystem.Create("TestSystem"); // Create a real actor system for testing
        }

        private Settings CreateSettings()
        {
            var config = ConfigurationFactory.Default(); // Use a default configuration
            return new Settings(_actorSystem, config); // Pass the real actor system and default config
        }

        [Fact]
        public void HasMessages_ShouldReturnFalse_WhenNoMessages()
        {
            var settings = CreateSettings();
            var mailbox = new RandomTimeBucketMailbox(settings, ConfigurationFactory.Default());
            Assert.False(mailbox.HasMessages);
        }

        [Fact]
        public void HasMessages_ShouldReturnTrue_WhenMessagesExist()
        {
            var settings = CreateSettings();
            var mailbox = new RandomTimeBucketMailbox(settings, ConfigurationFactory.Default());
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(new object(), _actorRefMock.Object));
            Assert.True(mailbox.HasMessages);
        }

        [Fact]
        public void Count_ShouldReturnCorrectMessageCount()
        {
            var settings = CreateSettings();
            var mailbox = new RandomTimeBucketMailbox(settings, ConfigurationFactory.Default());
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(new object(), _actorRefMock.Object));
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(new object(), _actorRefMock.Object));
            Assert.Equal(2, mailbox.Count);
        }

        [Fact]
        public void TryDequeue_ShouldReturnMessage_WhenMessagesExist()
        {
            var settings = CreateSettings();
            var mailbox = new RandomTimeBucketMailbox(settings, ConfigurationFactory.Default());
            mailbox.Enqueue(_actorRefMock.Object, new Envelope(new object(), _actorRefMock.Object));

            bool result = mailbox.TryDequeue(out Envelope dequeuedEnvelope);

            Assert.True(result);
            Assert.NotNull(dequeuedEnvelope.Message);
        }
    }
}

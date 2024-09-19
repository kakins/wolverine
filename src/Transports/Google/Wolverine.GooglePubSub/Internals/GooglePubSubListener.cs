using Google.Cloud.PubSub.V1;
using Wolverine.Transports;

namespace Wolverine.GooglePubSub.Internals
{
    internal class GooglePubSubListener : IListener, IDisposable
    {
        public Uri Address { get; private set; }

        private readonly SubscriberClient _subscriber;
        private readonly GooglePubSubTopicSubscription _subscription;
        private readonly IReceiver _receiver;
        private readonly Task _runner;
        private CancellationTokenSource _cancellation = new();

        public GooglePubSubListener(GooglePubSubTopicSubscription subscription, IReceiver receiver, SubscriberClient subscriber)
        {
            Address = subscription.Uri;

            _subscriber = subscriber;
            _subscription = subscription;
            _receiver = receiver;

            _runner = Task.Run(async () =>
            {
                await _subscriber.StartAsync(ProcessMessageAsync);
            }, _cancellation.Token);
        }

        private async Task<SubscriberClient.Reply> ProcessMessageAsync(PubsubMessage message, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received message {message.MessageId} published at {message.PublishTime.ToDateTime()}");
            Console.WriteLine($"Text: '{message.Data.ToStringUtf8()}'");

            var envelope = _subscription.Mapper.CreateEnvelope(message);

            if (envelope.IsPing())
            {
                return SubscriberClient.Reply.Ack;
            }

            // TODO: This works with inline endpoint mode for the subscription
            // in that it waits for the handler to finish executing before ack'ing
            // For durability or buffered listeners we may need a different approach

            await _receiver.ReceivedAsync(this, envelope);

            return SubscriberClient.Reply.Ack;
        }

        public ValueTask CompleteAsync(Envelope envelope)
        {
            // Do nothing here, it's already ack'd in the SubscriberClient delegate
            return ValueTask.CompletedTask;
        }

        public ValueTask DeferAsync(Envelope envelope)
        {
            // TODO: figure out how to defer messages 
            throw new NotImplementedException();
        }

        public async ValueTask DisposeAsync()
        {
            await _subscriber.DisposeAsync();
        }

        public void Dispose()
        {
            _subscriber.DisposeAsync();
            _runner.Dispose();
        }

        public async ValueTask StopAsync()
        {
            _cancellation.Cancel();
            await _runner; // necessary?
            await _subscriber.StopAsync(_cancellation.Token);
        }
    }
}

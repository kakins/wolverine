using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using JasperFx.Core;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Testcontainers.PubSub;
using Wolverine.Tracking;

namespace Wolverine.GooglePubSub.Tests;

public class end_to_end : IAsyncLifetime
{
    private IHost _host;
    private PubSubContainer _pubSubContainer;

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:8085");
        Environment.SetEnvironmentVariable("PUBSUB_PROJECT_ID", "test-project");

        _pubSubContainer = new PubSubBuilder()
            .WithImage("gcr.io/google.com/cloudsdktool/google-cloud-cli:emulators")
            .WithEnvironment("PUBSUB_PROJECT_ID", "test-project")
            .WithPortBinding(8085)
            .Build();

        await _pubSubContainer.StartAsync();

        var publisherService = await new PublisherServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        }.BuildAsync();
        var topicName = new TopicName("my-project", "my-topic");
        var topic = publisherService.CreateTopic(topicName);

        var subscriberService = await new SubscriberServiceApiClientBuilder()
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction
        }
        .BuildAsync();
        var subscriptionName = new SubscriptionName("my-project", "my-subscription");
        subscriberService.CreateSubscription(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);


        _host = await Host.CreateDefaultBuilder()
            .UseWolverine(opts =>
            {
                opts.UseGooglePubSub("test-project").AutoProvision();

                // TODO: Configure testing
                opts.PublishMessage<TestMessage>()
                    .ToGooglePubSubTopic("test-message")
                    .ConfigureTopic(publisher =>
                    {
                        publisher.EmulatorDetection = EmulatorDetection.EmulatorOrProduction;
                        publisher.TopicName = topicName;
                    });

                opts.ListenToGooglePubSubTopicSubscription("test-project", "test-subscription", subscriber =>
                {
                    // ...
                    subscriber.EmulatorDetection = EmulatorDetection.EmulatorOrProduction;
                    subscriber.SubscriptionName = subscriptionName;
                })
                .FromTopic("test-message");
                //.FromTopic("test-message", publisher =>
                //{
                //    // Optionally alter how the topic/publisher is created
                //    // ...
                //});

            }).StartAsync();
    }

    [Fact]
    public async Task send_and_receive_a_single_message()
    {
        var message = new TestMessage("testing");

        var session = await _host.TrackActivity()
            .IncludeExternalTransports()
            .Timeout(5.Minutes())
            .SendMessageAndWaitAsync(message);

        session.Received.SingleMessage<TestMessage>()
            .Name.ShouldBe(message.Name);
    }

    [Fact]
    public async Task Test1()
    {
        // First create a topic.
        var publisherService = await new PublisherServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        }.BuildAsync();
        var topicName = new TopicName("1234", "my-topic");
        var topic = publisherService.CreateTopic(topicName);

        // Subscribe to the topic.
        var subscriberService = await new SubscriberServiceApiClientBuilder()
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction
        }
        .BuildAsync();
        var subscriptionName = new SubscriptionName("1234", "my-subscription");
        subscriberService.CreateSubscription(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);

        // Publish message
        var publisher = await new PublisherClientBuilder()
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            TopicName = topicName,
        }
        .BuildAsync();
        string messageId = await publisher.PublishAsync("Hello, Pubsub");
        await publisher.ShutdownAsync(TimeSpan.FromSeconds(15));


        // Listen for messages
        var subscriber = await new SubscriberClientBuilder()
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            SubscriptionName = subscriptionName
        }
        .BuildAsync();
        var receivedMessages = new List<PubsubMessage>();
        await subscriber.StartAsync((msg, cancellationToken) =>
        {
            receivedMessages.Add(msg);
            Console.WriteLine($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
            Console.WriteLine($"Text: '{msg.Data.ToStringUtf8()}'");
            // Stop this subscriber after one message is received.
            // This is non-blocking, and the returned Task may be awaited.
            subscriber.StopAsync(TimeSpan.FromSeconds(15));
            // Return Reply.Ack to indicate this message has been handled.
            return Task.FromResult(SubscriberClient.Reply.Ack);
        });

        // Tidy up by deleting the subscription and the topic.
        subscriberService.DeleteSubscription(subscriptionName);
        publisherService.DeleteTopic(topicName);
    }

    public async Task DisposeAsync()
    {
    }

    public record TestMessage(string Name);

    public static class PubSubMessageHandler
    {
        public static void Handle(TestMessage message) 
        { 
        }
    }
}
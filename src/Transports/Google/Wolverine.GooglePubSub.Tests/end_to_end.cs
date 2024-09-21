using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using JasperFx.Core;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Testcontainers.PubSub;
using Wolverine.Configuration;
using Wolverine.GooglePubSub.Internals;
using Wolverine.Tracking;

namespace Wolverine.GooglePubSub.Tests;

public class EndToEndFixture : IAsyncLifetime
{
    public PubSubContainer PubSubContainer { get; private set; }
    public IHost Host { get; set; }

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:8085");
        Environment.SetEnvironmentVariable("PUBSUB_PROJECT_ID", "test-project");

        PubSubContainer = new PubSubBuilder()
            .WithImage("gcr.io/google.com/cloudsdktool/google-cloud-cli:emulators")
            .WithEnvironment("PUBSUB_PROJECT_ID", "test-project")
            .WithPortBinding(8085)
            .Build();

        await PubSubContainer.StartAsync();

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

        Host = await Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseWolverine(opts =>
            {
                opts.UseGooglePubSub("test-project").AutoProvision();

                // TODO: Configure testing
                opts.PublishMessage<TestMessage>()
                    .ToGooglePubSubTopic(
                        "test-message",
                        topic =>
                        {
                            // topic config
                        })
                    .ConfigureClient(publisher =>
                    {
                        publisher.EmulatorDetection = EmulatorDetection.EmulatorOrProduction;
                        publisher.TopicName = topicName;
                    });

                opts.ListenToGooglePubSubTopicSubscription(
                    "test-project", 
                    "test-subscription", 
                    subscriber =>
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

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

public class end_to_end : IClassFixture<EndToEndFixture>
{
    private readonly EndToEndFixture _fixture;

    public end_to_end(EndToEndFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task send_and_receive_a_single_message()
    {
        var message = new TestMessage("testing");

        var session = await _fixture.Host.TrackActivity()
            .IncludeExternalTransports()
            .Timeout(1.Minutes())
            .SendMessageAndWaitAsync(message);

        session.Received.SingleMessage<TestMessage>()
            .Name.ShouldBe(message.Name);
    }

    [Fact]
    public async Task builds_system_topics_by_default()
    {
        var transport = _fixture.Host.GetRuntime().Options.Transports.GetOrCreate<GooglePubSubTransport>();
        var endpoints = transport
            .Endpoints()
            .Where(x => x.Role == EndpointRole.System)
            .OfType<GooglePubSubTopic>()
            .ToArray();

        endpoints.ShouldContain(x => x.TopicName.TopicId.StartsWith("wolverine.response."));
        endpoints.ShouldContain(x => x.TopicName.TopicId.StartsWith("wolverine.retries."));
    }

    [Fact]
    public async Task builds_system_subscriptions_by_default()
    {
        var transport = _fixture.Host.GetRuntime().Options.Transports.GetOrCreate<GooglePubSubTransport>();
        var endpoints = transport
            .Endpoints()
            .Where(x => x.Role == EndpointRole.System)
            .OfType<GooglePubSubTopicSubscription>()
            .ToArray();

        endpoints.ShouldContain(x => x.Id.StartsWith("wolverine.response."));
        endpoints.ShouldContain(x => x.Id.StartsWith("wolverine.retries."));
    }

    [Fact]
    public async Task disables_system_topic_and_subscriptions()
    {

    }

    [Fact]
    public async Task send_and_receive_multiple_messages_concurreently()
    {
        Func<IMessageContext, Task> sendMany = async bus =>
        {
            await bus.SendAsync(new TestMessage("Red"));
            await bus.SendAsync(new TestMessage("Green"));
            await bus.SendAsync(new TestMessage("Refactor"));
        };

        var session = await _fixture.Host.TrackActivity()
            .IncludeExternalTransports()
            .Timeout(1.Minutes())
            .ExecuteAndWaitAsync(sendMany);

        session.Received.MessagesOf<TestMessage>()
            .Select(x => x.Name)
            .ToArray()
            .ShouldBeEquivalentTo(new[] { "Red", "Green", "Refactor" });
    }

    [Fact]
    public async Task send_and_receive_multiple_messages_in_order()
    {
        //var sessionConfig = _fixture.Host.TrackActivity()
        //    .IncludeExternalTransports()
        //    .Timeout(1.Minutes());

        //var session = await sessionConfig.ExecuteAndWaitAsync(ctx => ctx.SendAsync(new TestMessage("Red")));
        //session = await sessionConfig.ExecuteAndWaitAsync(ctx => ctx.SendAsync(new TestMessage("Green")));
        //session = await sessionConfig.ExecuteAndWaitAsync(ctx => ctx.SendAsync(new TestMessage("Refactor")));

        //session.Received.MessagesOf<TestMessage>()
        //    .Select(x => x.Name)
        //    .ToArray()
        //    .ShouldBeEquivalentTo(new[] { "Red", "Green", "Refactor" });
    }

    //[Fact]
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


    public static class PubSubMessageHandler
    {
        public static void Handle(TestMessage message) 
        { 
        }
    }
}

public record TestMessage(string Name);

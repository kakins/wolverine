namespace Wolverine.GooglePubSub
{
    public interface IGooglePubSubListeningEndpoint
    {
        /// <summary>
        ///     The maximum number of messages to receive in a single batch when listening
        ///     in either buffered or durable modes. The default is 20.
        /// </summary>
        public int MaximumMessagesToReceive { get; set; }

        /// <summary>
        ///     The duration for which the listener waits for a message to arrive in the
        ///     queue before returning. If a message is available, the call returns sooner than this time.
        ///     If no messages are available and the wait time expires, the call returns successfully
        ///     with an empty list of messages. Default is 5 seconds.
        /// </summary>
        public TimeSpan MaximumWaitTime { get; set; }
    }
}

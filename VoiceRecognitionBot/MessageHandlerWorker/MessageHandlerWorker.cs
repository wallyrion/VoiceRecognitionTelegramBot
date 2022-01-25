using System.Threading.Channels;

namespace VoiceRecognitionBot.MessageHandlerWorker;

public class MessageHandlerWorker<TMessage> : IMessageHandlerWorker<TMessage>
{
    private readonly Dictionary<int, Worker> _workers;
    private readonly Func<TMessage, string> _getUniqueIdFunc;
    private readonly int _workersNumber;
    private readonly ILogger _logger;

    public MessageHandlerWorker(
        Func<TMessage, Task> action,
        Func<TMessage, string> getUniqueIdFunc,
        CancellationToken token,
        int workersNumber,
        ILogger logger
    )
    {
        _workersNumber = workersNumber;
        _getUniqueIdFunc = getUniqueIdFunc;
        _workers = new Dictionary<int, Worker>(workersNumber);
        _logger = logger;

        for (int i = 0; i < workersNumber; i++)
        {
            _workers[i] = new Worker(action, token, logger);
        }
    }

    public void Process(TMessage item)
    {
        // load each worker evenly

        try
        {
            if (item == null)
            {
                _logger.LogWarning("Tried to pass null item into the message worker handler");
                return;
            }

            string uniqueId = _getUniqueIdFunc(item);

            if (uniqueId == string.Empty)
            {
                _logger.LogWarning("MessageHandlerWorker unique key can not be empty or null for type: {MessageType}, message: {@Message}",
                    item.GetType().FullName, item);

                return;
            }

            int workerId = Math.Abs(uniqueId.GetHashCode() % _workersNumber);
            if (!_workers[workerId].AddToQueueForProcessing(item))
            {
                _logger.LogWarning("MessageHandlerWorker unable to add message to worker queue for type: {MessageType}, message: {@Message}", 
                    item.GetType().FullName, item);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MessageHandlerWorker: Error while addding message to worker queue for type: {MessageType}, message: {@Message}", 
                item?.GetType().FullName, item);
        }
    }


    private class Worker
    {
        private readonly Channel<TMessage> _channel = Channel.CreateUnbounded<TMessage>(new UnboundedChannelOptions { SingleWriter = false, SingleReader = true });

        public Worker(Func<TMessage, Task> action, CancellationToken token, ILogger logger)
        {
            Task.Run(() => ProcessHandling(action, token, logger), token);
        }

        private async Task ProcessHandling(Func<TMessage, Task> action, CancellationToken token, ILogger logger)
        {
            while (await _channel.Reader.WaitToReadAsync(token))
            {
                TMessage item = await _channel.Reader.ReadAsync(token);

                try
                {
                    await action(item);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while processing message handling: {HandlerFunc}, message: {@Message}", 
                        action.GetType().FullName, item);
                }
            }
        }

        public bool AddToQueueForProcessing(TMessage item)
        {
            return _channel.Writer.TryWrite(item);
        }
    }
}
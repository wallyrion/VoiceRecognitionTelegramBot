/*namespace WeatherExample.MessageHandlerWorker;

public class MessageHandlerWorkerCreator : IMessageHandlerWorkerCreator
{
    private readonly CancellationToken _cancellationToken;
    private readonly int _workersNumber;
    private readonly ILogger _logger;

    public MessageHandlerWorkerCreator(int workersNumber, ILogger logger)
    {
        _workersNumber = workersNumber;
        _logger = logger;
    }

    public IMessageHandlerWorker<TMessage> GetHandler<TMessage>(
        Func<TMessage, Task> action,
        Func<TMessage, string> getUniqueIdFunc,
        int? workersNumber = null
    )
    {
        return new MessageHandlerWorker<TMessage>(action, getUniqueIdFunc, workersNumber ?? _workersNumber, _logger);
    }
}*/
namespace VoiceRecognitionBot.MessageHandlerWorker;

public interface IMessageHandlerWorkerCreator
{
    IMessageHandlerWorker<TMessage> GetHandler<TMessage>(
        Func<TMessage, Task> action,
        Func<TMessage, string> getUniqueIdFunc,
        int? workersCount = null);
}
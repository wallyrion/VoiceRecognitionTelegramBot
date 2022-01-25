namespace VoiceRecognitionBot.MessageHandlerWorker;

public interface IMessageHandlerWorker<TMessage>
{
    public void Process(TMessage message);
}
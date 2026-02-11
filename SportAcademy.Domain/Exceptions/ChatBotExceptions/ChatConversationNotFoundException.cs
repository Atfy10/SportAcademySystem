namespace SportAcademy.Domain.Exceptions;

public class ChatConversationNotFoundException : Exception
{
    public ChatConversationNotFoundException(string id)
        : base($"Chat conversation with id '{id}' was not found.")
    {
    }
}

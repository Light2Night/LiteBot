using Discord;
using Discord.WebSocket;

namespace LiteBot.StableDiffusion.UserRequests;

public class ResetPropertyRequest : UserRequest
{
    protected PropertyAccessor propertyAccessor;

    public ResetPropertyRequest(SocketMessage socketMessage, PropertyAccessor propertyAccessor) : base(socketMessage)
    {
        this.propertyAccessor = propertyAccessor;
    }

    public override void Exucute()
    {
        propertyAccessor.SetDefaultValues(socketMessage.Author.Id);

        MessageReference messageReference = new MessageReference(socketMessage.Id, socketMessage.Channel.Id);
        SendMessage("Default values are set", messageReference);
    }
}
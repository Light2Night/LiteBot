namespace CS_DiscordBot.Exceptions;

public class IsNotCommandException : Exception
{
    public IsNotCommandException() : this("Is not command cxception") { }
    public IsNotCommandException(string message) : base(message) { }
}

namespace LiteBot.Exceptions;

public class UnknownCommandException : Exception
{
    public UnknownCommandException() : this("Unknown command cxception") { }
    public UnknownCommandException(string message) : base(message) { }
}
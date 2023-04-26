using DiscordBot.StableDiffusionUserRequests;

namespace DiscordBot.StableDiffusion;

public class StableDiffusionQueue
{
    protected Queue<Task> queue = new();
    protected object queueLocker = new();

    public StableDiffusionQueue() { }

    public void Enqueue(UserRequest request)
    {
        Task task = new Task(() => ExecuteAction(request.Exucute));

        lock (queueLocker)
        {
            queue.Enqueue(task);

            if (queue.Count == 1)
            {
                queue.Peek().Start();
            }
        }
    }

    protected void ExecuteAction(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            //new EventHandler<Exception>(this, e);
        }
        finally
        {
            DequeueAndStartNext();
        }
    }

    protected void DequeueAndStartNext()
    {
        lock (queueLocker)
        {
            queue.Dequeue();
            if (queue.Count > 0)
                queue.Peek().Start();
        }
    }
}
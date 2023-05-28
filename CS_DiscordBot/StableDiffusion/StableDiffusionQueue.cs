using LiteBot.StableDiffusion.UserRequests;

namespace LiteBot.StableDiffusion;

public class StableDiffusionQueue {
	protected Queue<Task> queue = new();
	protected object queueLocker = new();

	public event EventHandler<Exception>? ExceptionCatched;

	public StableDiffusionQueue() { }

	public void Enqueue(UserRequest request) {
		Task task = new Task(() => ExecuteAction(request.Exucute));

		lock (queueLocker) {
			queue.Enqueue(task);

			if (queue.Count == 1) {
				queue.Peek().Start();
			}
		}
	}

	protected void ExecuteAction(Action action) {
		try {
			action();
		}
		catch (Exception ex) {
			ExceptionCatched?.Invoke(this, ex);
		}
		finally {
			DequeueAndStartNext();
		}
	}

	protected void DequeueAndStartNext() {
		lock (queueLocker) {
			queue.Dequeue();
			if (queue.Count > 0)
				queue.Peek().Start();
		}
	}
}
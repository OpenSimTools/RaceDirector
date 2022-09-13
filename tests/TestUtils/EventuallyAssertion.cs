namespace TestUtils;

public class EventuallyAssertion
{
    private readonly Action _action;
    private string _errorMessage = "Not satisfied within timeout";

    public static EventuallyAssertion Eventually(Action action) => new(action);

    private EventuallyAssertion(Action action)
    {
        _action = action;
    }

    public EventuallyAssertion OrError(string message)
    {
        _errorMessage = message;
        return this;
    }

    public void Within(TimeSpan timeout) => Within(timeout, timeout / 20);

    public void Within(TimeSpan timeout, TimeSpan interval)
    {
        var end = DateTime.Now.Add(timeout);
        while (true)
        {
            try
            {
                _action();
                break;
            }
            catch (Exception e)
            {
                if (DateTime.Now > end)
                    throw new TimeoutException(_errorMessage, e);
                Thread.Sleep(interval);
            }
        }
    }
}
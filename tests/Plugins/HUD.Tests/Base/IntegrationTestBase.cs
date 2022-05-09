using System;
using System.Threading;
using Xunit.Categories;

namespace HUD.Tests.Base
{
    [IntegrationTest]
    public abstract class IntegrationTestBase
    {
        protected static TimeSpan Timeout { get => TimeSpan.FromMilliseconds(500); }
        protected static TimeSpan PollingInterval { get => TimeSpan.FromMilliseconds(50); }

        protected void Eventually(Action f) => Eventually(f, "Not satisfied within timeout");

        protected void Eventually(Action f, string message)
        {
            var end = DateTime.Now.Add(Timeout);
            while (true)
            {
                try
                {
                    f();
                    break;
                }
                catch (Exception e)
                {
                    if (DateTime.Now > end)
                        throw new TimeoutException(message, e);
                    else
                        Thread.Sleep(PollingInterval);
                }
            }
        }
    }
}

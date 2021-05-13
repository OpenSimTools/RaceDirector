using System;
using System.Threading;
using Xunit.Categories;

namespace HUD.Tests.Base
{
    [IntegrationTest]
    public abstract class IntegrationTestBase
    {
        protected TimeSpan Timeout { get => TimeSpan.FromMilliseconds(500); }
        protected TimeSpan PollingInterval { get => TimeSpan.FromMilliseconds(50); }

        protected void Eventually(Action f)
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
                        throw new TimeoutException("Not satisfied within timeout", e);
                    else
                        Thread.Sleep(PollingInterval);
                }
            }
        }
    }
}

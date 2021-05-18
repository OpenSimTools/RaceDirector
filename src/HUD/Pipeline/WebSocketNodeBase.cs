using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Plugin.HUD.Pipeline
{
    /// <summary>
    /// Starts and stops WebSocket servers based on trigger input.
    /// Broadcasts to all WebSocket clients on data input.
    /// </summary>
    /// <typeparam name="TTrigger">Trigger type</typeparam>
    /// <typeparam name="TData">Data type</typeparam>
    public abstract class WebSocketNodeBase<TTrigger, TData> : IDisposable
    {
        protected readonly ITargetBlock<TTrigger> TriggerTarget;
        protected readonly ITargetBlock<TData> DataTarget;

        protected WebSocketNodeBase(IEnumerable<IWsServer<TData>> servers)
        {
            TriggerTarget = new ActionBlock<TTrigger>(trigger => {
                if (ServerShouldRun(trigger))
                    foreach (var s in servers) s.Start();
                else
                    foreach (var s in servers) s.Stop();
            });
            DataTarget = new ActionBlock<TData>(data => {
                foreach (var s in servers) s.Multicast(data);
            });
        }

        /// <summary>
        /// Returns if the trigger data should start ot stop the servers.
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        protected abstract bool ServerShouldRun(TTrigger trigger);

        public virtual void Dispose()
        {
            TriggerTarget.Complete();
            DataTarget.Complete();
        }
    }
}

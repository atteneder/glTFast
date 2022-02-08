// Source: https://social.msdn.microsoft.com/Forums/en-US/163ef755-ff7b-4ea5-b226-bbe8ef5f4796/is-there-a-pattern-for-calling-an-async-method-synchronously?forum=async

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GLTFast.Utils {

    static class AsyncHelpers
    {
        /// <summary>
        /// Executes an async Task&lt;T&gt; method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task&lt;T&gt; method to execute</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var sync = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(sync);
            sync.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    sync.InnerException = e;
                    throw;
                }
                finally
                {
                    sync.EndMessageLoop();
                }
            }, null);
            sync.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Executes an async Task&lt;T&gt; method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task&lt;T&gt; method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var sync = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(sync);
            T ret = default(T);
            sync.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    sync.InnerException = e;
                    throw;
                }
                finally
                {
                    sync.EndMessageLoop();
                }
            }, null);
            sync.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        class ExclusiveSynchronizationContext : SynchronizationContext
        {
            bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exception
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}
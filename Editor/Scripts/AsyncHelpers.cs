// Source: https://social.msdn.microsoft.com/Forums/en-US/163ef755-ff7b-4ea5-b226-bbe8ef5f4796/is-there-a-pattern-for-calling-an-async-method-synchronously?forum=async

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GLTFast.Utils
{

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
            // ReSharper disable once AsyncVoidLambda
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
            // ReSharper disable once AsyncVoidLambda
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
            bool m_Done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent m_WorkItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> m_Items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (m_Items)
                {
                    m_Items.Enqueue(Tuple.Create(d, state));
                }
                m_WorkItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => m_Done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!m_Done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (m_Items)
                    {
                        if (m_Items.Count > 0)
                        {
                            task = m_Items.Dequeue();
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
                        m_WorkItemsWaiting.WaitOne();
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

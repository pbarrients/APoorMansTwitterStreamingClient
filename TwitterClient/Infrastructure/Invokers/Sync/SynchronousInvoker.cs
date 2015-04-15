using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterClient.Infrastructure.Invokers.Sync
{
    public interface ISynchronousInvoker
    {
        T GetResultSynchronously<T>(Task<T> task);
        void ExecuteSynchronously(Task task);
    }

    public class SynchronousInvoker : ISynchronousInvoker
    {
        public T GetResultSynchronously<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }

        public void ExecuteSynchronously(Task task)
        {
            task.Wait();
        }
    }
}

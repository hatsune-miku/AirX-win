using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Extension
{
    public static class TaskLogOnError
    {
        public static void LogOnError(this Task task)
        {
            _ = task.ContinueWith(it =>
            {
                Debug.WriteLineIf(it.IsFaulted, it.Exception);
            }, TaskScheduler.Default);
        }
    }
}

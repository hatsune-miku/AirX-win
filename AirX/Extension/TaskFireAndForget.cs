using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Extension
{
    public static class TaskFireAndForget
    {
        /// <summary>
        /// 给Task的扩展方法，赋予放任一个Task在后台运行而对其结果不关心的能力。
        /// </summary>
        public static void FireAndForget(this Task task)
        {
            _ = task.ContinueWith(it =>
            {
                Debug.WriteLineIf(it.IsFaulted, it.Exception);
            }, TaskScheduler.Default);
        }
    }
}

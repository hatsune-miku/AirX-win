using AirX.ViewModel;
using Lombok.NET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirX.Worker
{
    /// 负责写入文件的后台worker
    class FilePartWorker
    {
        /// 文件块的队列
        private BlockingCollection<FilePartWorkload> queue = new();

        /// 用于在UI线程执行代码
        private SynchronizationContext context = SynchronizationContext.Current;

        /// 工作线程
        private Thread worker;

        public FilePartWorker()
        {

        }

        /// 添加一份工作内容，即：哪个文件、写哪里、写什么
        public void PostWorkload(FilePartWorkload workload)
        {
            queue.Add(workload);
        }

        /// 开始不断等待工作的到来、处理工作
        public void Run()
        {
            while (!queue.IsCompleted)
            {
                try
                {
                    var workload = queue.Take();
                    HandleSingleWorkload(workload);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public void Start()
        {
            /// 判断是否已经在工作了
            if (worker != null)
            {
                throw new InvalidOperationException("Worker already started.");
            }
            worker = new Thread(Run);
            worker.Start();
        }


        /// 处理单件任务
        private void HandleSingleWorkload(FilePartWorkload workload)
        {
            NewFileViewModel remoteViewModel;
            try
            {
                /// 判断对应文件是否不存在
                remoteViewModel = GlobalViewModel.Instance.ReceiveFiles[workload.FileId];
            }
            catch (Exception)
            {
                Debug.WriteLine("Unexpected file incoming: fileid=" + workload.FileId + ", length=" + workload.Length);
                return;
            }

            var file = remoteViewModel.ReceivingFile;

            /// 判断这个文件用户还要不要
            if (file.Status == AirXBridge.FileStatus.CancelledByReceiver)
            {
                /// 不要了的情况
                Debug.WriteLine("File cancelled!");

                /// WritingStream是流式写入的，所以用完要记得关闭
                file.WritingStream?.Close();

                /// 从全局状态中移除这个文件
                GlobalViewModel.Instance.ReceiveFiles.Remove(workload.FileId);
                return;
            }

            // In progress.
            file.Status = AirXBridge.FileStatus.InProgress;
            if (file.WritingStream == null)
            {
                file.Status = AirXBridge.FileStatus.Error;
                Debug.WriteLine("File not accepted!");
                return;
            }

            // Update UI.
            context.Post(_ =>
            {
                file.DisplayProgress = file.Progress;
                file.DisplayStatus = AirXBridge.FileStatus.InProgress;
            }, null);

            /// 跳到文件对应位置，写入对应数据
            file.WritingStream.Seek((long)workload.Offset, SeekOrigin.Begin);
            file.WritingStream.Write(workload.Data, 0, (int)workload.Length);

            /// 更新总进度
            file.Progress += workload.Length;

            if (file.Progress == file.TotalSize)
            {
                /// 总进度达到文件总大小，视为传输+保存成功
                context.Post(_ =>
                {
                    file.DisplayProgress = file.Progress;

                    /// 更新状态
                    file.DisplayStatus = AirXBridge.FileStatus.Completed;
                }, null);

                /// 关闭水龙头
                file.WritingStream.Close();
                file.WritingStream = null;

                file.Status = AirXBridge.FileStatus.Completed;
                Debug.WriteLine("File recv completed!");
            }
        }
    }

    public partial class FilePartWorkload
    {
        public byte FileId { get; set; }
        public UInt64 Offset { get; set; }
        public UInt64 Length { get; set; }
        public byte[] Data { get; set; }


        public FilePartWorkload(byte fileId, ulong offset, ulong length, byte[] data)
        {
            FileId = fileId;
            Offset = offset;
            Length = length;
            Data = data;
        }
    }
}

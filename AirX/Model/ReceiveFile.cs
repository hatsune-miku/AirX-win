using AirX.Bridge;
using Lombok.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Model
{
    public class ReceiveFile
    {
        public string Filename { get; set; }
        public FileStream WritingStream { get; set; }
        public string FullPath { get; set; }
        public ulong Progress { get; set; }
        public ulong TotalSize { get; set; }
        public int FileId { get; set; }
        public AirXBridge.FileStatus Status { get; set; }
        public Peer From { get; set; }

        public static readonly ReceiveFile Sample = new()
        {
            Filename = "sample.pdf",
            WritingStream = null,
            FullPath = "",
            Progress = (ulong) (100 * 1024 * 0.65),
            TotalSize = 100 * 1024,
            FileId = -1,
            Status = AirXBridge.FileStatus.InProgress,
            From = Peer.Sample,
        };
    }
}

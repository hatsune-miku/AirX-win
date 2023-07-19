using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Model
{
    /// <summary>
    /// 正发送中的文件
    /// </summary>
    public class SendFile
    {
        public string Filename { get; set; }                /** 文件名 */
        public string FullPath { get; set; }                /** 文件在我方机器的绝对路径 */
        public ulong Progress { get; set; }                 /** 已发送的字节数 */
        public ulong TotalSize { get; set; }                /** 文件总字节数 */
        public int FileId { get; set; }                     /** 接收方规定的FileId */
        public AirXBridge.FileStatus Status { get; set; }   /** 文件的发送状态 */
    }
}

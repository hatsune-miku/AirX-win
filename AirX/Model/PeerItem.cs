using AirX.Pages;
using AirX.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Model
{
    /// <summary>
    /// Peer的包装类，用于在UI中显示Peer的信息。
    /// 好像存在的意义并不大，不知道当时咋写的
    /// </summary>
    public class PeerItem
    {
        public Peer Value { get; set; }

        public PeerItem(Peer peer)
        {
            Value = peer;
        }

        public string GetDescription()
        {
            return $"{Value.IpAddress}:{Value.Port}";
        }
    }
}

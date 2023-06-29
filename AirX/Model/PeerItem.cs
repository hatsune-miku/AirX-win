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

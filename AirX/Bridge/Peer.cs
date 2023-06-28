using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Bridge
{
    public class Peer
    {
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

        public static Peer Parse(string s)
        {
            var part1 = s.Split("@");
            var part2 = part1[1].Split(":");
            return new Peer
            {
                Hostname = part1[0],
                IpAddress = part2[0],
                Port = int.Parse(part2[1]),
            };
        }

        public static bool TryParse(string s, out Peer peer)
        {
            try
            {
                peer = Parse(s);
                return true;
            }
            catch (Exception)
            {
                peer = null;
                return false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Model
{
    /// 一个Peer，代表局域网中，另一个使用AirX的靓仔。
    /// 自己不是自己的Peer。
    public class Peer
    {
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

        /// 定义一个随时可取用的sample peer，预览时候用
        public static readonly Peer Sample = new()
        {
            Hostname = "chance",
            IpAddress = "10.0.0.6",
            Port = 12345,
        };

        /// 定义格式为 `Chance@10.0.0.1:9819`
        public override string ToString()
        {
            return $"{Hostname}@{IpAddress}:{Port}";
        }

        /// 上面`description`的逆向操作，从String解析出Peer对象
        /// Peer format: <hostname>@<host>:<port>
        /// <summary>
        /// Peer string format: <hostname>@<ip>:<port>
        /// </summary>
        public static Peer Parse(string s)
        {
            // Incomplete peer string?
            /// 有时候description会简化成不带`@`的形式，这里特殊处理一下
            if (!s.Contains("@"))
            {
                s = "<empty>@" + s;
            }

            /// part1 = ["Chance", "10.0.0.1:9819"]
            var part1 = s.Split("@");

            /// part2 = ["10.0.0.1", "9819"]
            var part2 = part1[1].Split(":");

            return new Peer
            {
                Hostname = part1[0],
                IpAddress = part2[0],
                Port = int.Parse(part2[1]),
            };
        }

        /// <summary>
        /// 与Peer不同，TryParse不会一言不合就抛出异常，而是如果解析失败，返回false。
        /// </summary>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerApiLib
{
    public record Address : IAddress
    {
        public string IP { get; set; }
        public int? Port { get; set; }

        public Address(string ip, int port)
        {
            IP = ip;
            Port = port;
        }

        public Address(string ip)
        {
            IP = ip;
        }

        public string Get()
        {
            if(Port != null)
            {
                return $"{IP}:{Port}";
            }
            else
            {
                return IP;
            }
            
        }
    }
}

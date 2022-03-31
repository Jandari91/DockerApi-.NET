using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerApiLib
{
    public interface IAddress
    {
        string IP { get; set; }
        int? Port { get; set; }

        string Get();
    }
}

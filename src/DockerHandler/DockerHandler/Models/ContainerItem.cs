using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerHandler.Models
{
    public class ContainerItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public ContainerItem(string id, string name, string state)
        {
            Id = id;
            Name = name;
            State = state;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerApiLib
{

    public class ContainerInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? State { get; set; }

        public ContainerInfo(string id, string name, string state)
        {
            if (id == null || string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            if (name == null || string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (state == null || string.IsNullOrEmpty(state))
                throw new ArgumentNullException("state");

            Id = id;
            Name = name;
            State = state;
        }

        public ContainerInfo(string id, string name)
        {
            if (id == null || string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            if (name == null || string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Id = id;
            Name = name;
        }
    }
    
}

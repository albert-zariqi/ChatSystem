using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Coordinator.App.Configurations
{
    public class RabbitMqConfiguration
    {
        public string HostName { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }
}

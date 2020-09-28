using System.Net;

namespace Quotes_Receiver.Models
{
    public class ReceiveConfig
    {
        public int ReceiveDelayMs { get; set; }
        public IPAddress MulticastGroup { get; set; }
        public int MulticastPort { get; set; }
    }
}
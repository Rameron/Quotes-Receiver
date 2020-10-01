using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Quotes_Receiver.Models;

namespace Quotes_Receiver
{
    public class ValuesReceiver
    {
        private const int RECONNECT_DELAY_MS = 5000;

        private readonly ReceiveConfig _receiveConfig;

        private int _lastDelaySecond = -1;

        private UdpClient _udpClient;

        public ValuesReceiver(ReceiveConfig receiveConfig)
        {
            _receiveConfig = receiveConfig;
            ReceivedPackets = new ConcurrentQueue<byte[]>();
        }

        public ConcurrentQueue<byte[]> ReceivedPackets { get; }


        ~ValuesReceiver()
        {
            StopReceiving();
        }

        public void StartReceiving()
        {
            InitUdpClient();

            var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                ReceivedPackets.Enqueue(ReceiveValue(ref ipEndPoint));
                ExecuteDelay();
            }
        }

        private void StopReceiving()
        {
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient.Dispose();
            }
        }

        private void InitUdpClient()
        {
            _udpClient = new UdpClient(_receiveConfig.MulticastPort);
            _udpClient.JoinMulticastGroup(_receiveConfig.MulticastGroup);
        }

        private byte[] ReceiveValue(ref IPEndPoint ipEndPoint)
        {
            try
            {
                return _udpClient.Receive(ref ipEndPoint);
            }
            catch (SocketException e)
            {
                Console.WriteLine(
                    $"Failed to receive data through UDP client: {e.Message}.\nOperation will repeat in {RECONNECT_DELAY_MS / 1000} seconds...");
                Thread.Sleep(RECONNECT_DELAY_MS);
                return ReceiveValue(ref ipEndPoint);
            }
        }

        private void ExecuteDelay()
        {
            if (_lastDelaySecond != DateTime.Now.Second)
            {
                Thread.Sleep(_receiveConfig.ReceiveDelayMs);
                _lastDelaySecond = DateTime.Now.Second;
            }
        }
    }
}
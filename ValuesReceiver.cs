using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        private long _firstPacketCounter = -1L;
        private int _lastDelaySecond = -1;
        private long _lastPacketCounter = -1L;

        private UdpClient _udpClient;

        public ValuesReceiver(ReceiveConfig receiveConfig)
        {
            _receiveConfig = receiveConfig;
            ReceivedValues = new ConcurrentDictionary<double, int>();
        }

        public ConcurrentDictionary<double, int> ReceivedValues { get; }

        public long LostPackets { get; private set; }


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
                var receivedData = ReceiveValue(ref ipEndPoint);
                if (receivedData.Length != 16)
                {
                    continue;
                }

                CalculateLostPackets(receivedData);
                AddReceivedValue(receivedData);
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
                Console.WriteLine($"Failed to receive data through UDP client: {e.Message}.\nOperation will repeat in {RECONNECT_DELAY_MS / 1000} seconds...");
                Thread.Sleep(RECONNECT_DELAY_MS);
                return ReceiveValue(ref ipEndPoint);
            }
        }

        private void CalculateLostPackets(IEnumerable<byte> inputBytes)
        {
            var currentPacketCounter = BitConverter.ToInt64(inputBytes.Take(8).ToArray());
            if (_firstPacketCounter == -1L)
            {
                _firstPacketCounter = currentPacketCounter;
            }
            else if (_lastPacketCounter == -1L)
            {
                LostPackets += currentPacketCounter - _firstPacketCounter - 1;
            }
            else
            {
                LostPackets += currentPacketCounter - _lastPacketCounter - 1;
            }

            _lastPacketCounter = currentPacketCounter;
        }

        private void AddReceivedValue(IEnumerable<byte> inputBytes)
        {
            var receivedValue = BitConverter.ToDouble(inputBytes.Skip(8).Take(8).ToArray());
            ReceivedValues.AddOrUpdate(receivedValue, 1, (k, v) => v + 1);
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
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Quotes_Receiver
{
    public class PacketsProcessor
    {
        private readonly ValuesReceiver _valuesReceiver;
        private long _firstPacketCounter = -1L;
        private long _lastPacketCounter = -1L;

        public PacketsProcessor(ValuesReceiver valuesReceiver)
        {
            _valuesReceiver = valuesReceiver;
            ReceivedValues = new ConcurrentDictionary<double, int>();
        }

        public long LostPackets { get; private set; }
        public ConcurrentDictionary<double, int> ReceivedValues { get; }

        public void StartProcessing()
        {
            while (true)
            {
                if (_valuesReceiver.ReceivedPackets.TryDequeue(out var newReceivedPacket))
                {
                    if (newReceivedPacket.Length != 16)
                    {
                        continue;
                    }

                    var packetCounter = BitConverter.ToInt64(newReceivedPacket.Take(8).ToArray());
                    CalculateLostPackets(packetCounter);

                    var receivedValue = BitConverter.ToDouble(newReceivedPacket.Skip(8).Take(8).ToArray());
                    AddReceivedValue(receivedValue);
                }
            }
        }

        private void CalculateLostPackets(long packetCounter)
        {
            if (_firstPacketCounter == -1L)
            {
                _firstPacketCounter = packetCounter;
            }
            else if (_lastPacketCounter == -1L)
            {
                LostPackets += packetCounter - _firstPacketCounter - 1;
            }
            else
            {
                LostPackets += packetCounter - _lastPacketCounter - 1;
            }

            _lastPacketCounter = packetCounter;
        }

        private void AddReceivedValue(double receivedValue)
        {
            ReceivedValues.AddOrUpdate(receivedValue, 1, (k, v) => v + 1);
        }
    }
}
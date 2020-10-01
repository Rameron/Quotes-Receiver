using System;
using System.Linq;
using System.Threading.Tasks;

namespace Quotes_Receiver
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var configReader = new ConfigReader();
                var receiveConfig = configReader.ReadReceiveConfig();
                Console.WriteLine("Receive configuration successfully loaded!");

                var valuesReceiver = new ValuesReceiver(receiveConfig);
                Task.Factory.StartNew(valuesReceiver.StartReceiving, TaskCreationOptions.LongRunning);
                Console.WriteLine("Values receiver successfully started!");

                var packetsProcessor = new PacketsProcessor(valuesReceiver);
                Task.Factory.StartNew(packetsProcessor.StartProcessing, TaskCreationOptions.LongRunning);
                Console.WriteLine("Packets processor successfully started!");

                var characteristicsCalculator = new CharacteristicsCalculator(packetsProcessor);
                Task.Factory.StartNew(characteristicsCalculator.CalculateCharacteristics, TaskCreationOptions.LongRunning);
                Console.WriteLine("Characteristics calculator successfully started!");

                Console.WriteLine();
                Console.WriteLine("Press 'Enter' for show current statistics.");
                Console.WriteLine("Press 'CTRL+C' for close application.");
                Console.WriteLine();

                while (true)
                {
                    while (Console.ReadKey().Key != ConsoleKey.Enter) { }

                    Console.WriteLine(characteristicsCalculator.ValuesCharacteristics);
                    Console.WriteLine($"Lost Packets: {packetsProcessor.LostPackets}");
                    Console.WriteLine($"Received Packets: {packetsProcessor.ReceivedValues.Sum(x => x.Value)}");
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key for exit...");
            Console.ReadKey();
        }
    }
}
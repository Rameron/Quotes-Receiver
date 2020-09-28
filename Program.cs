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

                var characteristicsCalculator = new CharacteristicsCalculator(valuesReceiver);
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
                    Console.WriteLine($"Lost Packets: {valuesReceiver.LostPackets}");
                    Console.WriteLine($"Received Packets: {valuesReceiver.ReceivedValues.Sum(p => p.Value)}");
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
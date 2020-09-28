using System;
using System.Collections.Generic;
using System.Linq;
using Quotes_Receiver.Models;

namespace Quotes_Receiver
{
    public class CharacteristicsCalculator
    {
        private const int ROUND_DIGITS = 4;

        private readonly ValuesReceiver _valuesReceiver;

        public ValuesCharacteristics ValuesCharacteristics { get; }

        public CharacteristicsCalculator(ValuesReceiver valuesReceiver)
        {
            ValuesCharacteristics = new ValuesCharacteristics();
            _valuesReceiver = valuesReceiver;
        }

        public void CalculateCharacteristics()
        {
            while (true)
            {
                var inputDictionary = new Dictionary<double, int>(_valuesReceiver.ReceivedValues);
                if (!inputDictionary.Any())
                {
                    continue;
                }

                ValuesCharacteristics.Average = Math.Round(GetAverage(inputDictionary), ROUND_DIGITS);
                ValuesCharacteristics.StandardDeviation =
                    Math.Round(GetStandardDeviation(inputDictionary, ValuesCharacteristics.Average), ROUND_DIGITS);
                ValuesCharacteristics.Median = Math.Round(GetMedian(inputDictionary), ROUND_DIGITS);
                ValuesCharacteristics.Mode = Math.Round(GetMode(inputDictionary), ROUND_DIGITS);
            }
        }

        private static double GetAverage(Dictionary<double, int> inputDictionary)
        {
            var valuesCount = CalculateValuesCount(inputDictionary);
            return inputDictionary.Select(p => p.Key * p.Value).Sum() / valuesCount;
        }

        private static double GetStandardDeviation(Dictionary<double, int> inputDictionary, double averageValue)
        {
            var valuesCount = CalculateValuesCount(inputDictionary);
            var averagesSum = inputDictionary.Sum(p => Math.Pow(p.Key - averageValue, 2) * p.Value);
            return Math.Sqrt(averagesSum / (valuesCount - 1));
        }

        private static double GetMedian(Dictionary<double, int> inputDictionary)
        {
            var orderedValues = inputDictionary.OrderBy(p => p.Key).ToList();
            var midValue = (orderedValues.Count - 1) / 2.0;
            return (orderedValues[(int) midValue].Key + orderedValues[(int) (midValue + 0.5)].Key) / 2;
        }

        private static double GetMode(Dictionary<double, int> inputDictionary)
        {
            var maxCount = inputDictionary.Max(p => p.Value);
            return inputDictionary.FirstOrDefault(p => p.Value == maxCount).Key;
        }

        private static int CalculateValuesCount(Dictionary<double, int> inputDictionary)
        {
            return inputDictionary.Sum(p => p.Value);
        }
    }
}
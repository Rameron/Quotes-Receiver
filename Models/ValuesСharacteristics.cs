namespace Quotes_Receiver.Models
{
    public class ValuesCharacteristics
    {
        public double Average { get; set; }
        public double StandardDeviation { get; set; }
        public double Mode { get; set; }
        public double Median { get; set; }

        public override string ToString()
        {
            return $"Average: {Average}\nStandard Deviation: {StandardDeviation}\nMode: {Mode}\nMedian: {Median}";
        }
    }
}
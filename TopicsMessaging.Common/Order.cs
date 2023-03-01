namespace TopicsMessaging.Common
{
    public class Order
    {
        public string Name { get; set; }
        public DateTime OrderDate { get; set; }
        public int Items { get; set; }
        public double Value { get; set; }
        public string Priority { get; set; }
        public string Region { get; set; }
        public bool HasLoyaltyCard { get; set; }

        public override string ToString()
        {
            return $"{Name} \t Itm: {Items} \t {Value} \t {Region} \t Loyal: {HasLoyaltyCard}";
        }
    }
}
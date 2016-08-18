namespace AtomicChessPuzzles.Models
{
    public class Counter
    {
        public string ID { get; set; }
        public int Next { get; set; }

        public Counter(string id, int next)
        {
            ID = id;
            Next = next;
        }
    }
}

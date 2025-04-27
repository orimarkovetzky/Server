using System.Data.SqlClient;

namespace FlowServer.Models
{
    public class MachineCard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string Status { get; set; }
        public string CurrentProduct { get; set; }
        public string NextProduct { get; set; }
        public int TimeRemaining { get; set; }
        public bool IsDelayed { get; set; } = false;

        public MachineCard(int id, string name, int type, string status, string currentProduct, string nextProduct, int timeRemaining, bool isDelayed)
        {
            Id = id;
            Name = name;
            Type = type;
            Status = status;
            CurrentProduct = currentProduct;
            NextProduct = nextProduct;
            TimeRemaining = timeRemaining;
            IsDelayed = isDelayed;
        }
        public MachineCard() { }

    }
}

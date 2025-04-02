namespace FlowServer.Models
{
    public class Task
    {
        public Batch batch { get; set; }
        public Machine machine { get; set; }
        public DateTime estStartTime { get; set; }
        public DateTime estEndTime { get; set; }
        public DateTime actStartTime { get; set; }
        public DateTime actEndTime { get; set; }
    }
}

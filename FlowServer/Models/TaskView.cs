namespace FlowServer.Models
{
    public class TaskView
    {
        public int BatchId { get; set; }
        public int MachineId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public DateTime StartTimeEst { get; set; }
        public DateTime EndTimeEst { get; set; }
        public string Status { get; set; }

        public TaskView(int batchId, int machineId, DateTime startTime, DateTime endTime, string status)
        {
            BatchId = batchId;
            MachineId = machineId;
            StartTime = startTime;
            EndTime = endTime;
            Status = status;
        }
        public TaskView() { } // default constructor

    }

}

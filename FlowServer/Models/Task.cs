using FlowServer.DBServices;
namespace FlowServer.Models
{
    public class Task
    {
        public Batch batch { get; set; }
        public Machine machine { get; set; }
        public DateTime startTimeEst { get; set; }
        public DateTime endTimeEst { get; set; }
        public DateTime actStartTime { get; set; }
        public DateTime actEndTime { get; set; }
        public string status { get; set; }

        public Task(Batch batch, Machine machine, DateTime estStartTime, DateTime estEndTime, DateTime actStartTime, DateTime actEndTime, string status)
        {
            this.batch = batch;
            this.machine = machine;
            this.startTimeEst = estStartTime;
            this.endTimeEst = estEndTime;
            this.actStartTime = actStartTime;
            this.actEndTime = actEndTime;
            this.status = status;
        }

        public Task(Batch batch, Machine machine, DateTime estStartTime, DateTime estEndTime,string status)
        {
            this.batch = batch;
            this.machine = machine;
            this.startTimeEst = estStartTime;
            this.endTimeEst = estEndTime;
            this.status = status;
        }
       
        public Task() { }

        public static int UpdateTaskStatus(int batchId,int machineId, string newStatus)
        {
            TaskDBServices dbs = new TaskDBServices();
            return dbs.ChangeTaskStatus(batchId,machineId, newStatus);
        }

        public static int ScheduleTask(int batchId, int machineId, DateTime startTimeEst, DateTime endTimeEst)
        {
            TaskDBServices dbServices = new TaskDBServices();
            return dbServices.ScheduleTask(batchId, machineId, startTimeEst, endTimeEst);
        }
    }
}

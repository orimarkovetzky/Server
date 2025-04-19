using FlowServer.DBServices;
namespace FlowServer.Models
{
    public class Task
    {
        public int batchId { get; set; }
        public int machineId { get; set; }
        public DateTime startTimeEst { get; set; }
        public DateTime endTimeEst { get; set; }
        public DateTime actStartTime { get; set; }
        public DateTime actEndTime { get; set; }
        public string status { get; set; }
        public DateTime? EndTimeEst { get; internal set; }

        public Task(int batchId, int machineId, DateTime estStartTime, DateTime estEndTime, DateTime actStartTime, DateTime actEndTime, string status)
        {
            this.batchId = batchId;
            this.machineId = machineId;
            this.startTimeEst = estStartTime;
            this.endTimeEst = estEndTime;
            this.actStartTime = actStartTime;
            this.actEndTime = actEndTime;
            this.status = status;
        }

        public Task(int batchId, int machineId, DateTime estStartTime, DateTime estEndTime,string status)
        {
            this.batchId = batchId;
            this.machineId = machineId;
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

        public static int CreateTask(int batchId, int machineId, int userId)
        {
            TaskDBServices dbServices = new TaskDBServices();
            return dbServices.CreateTask(batchId, machineId, userId);
        }
        public static int ScheduleTask(int batchId, int machineId,int userId ,DateTime startTimeEst, DateTime endTimeEst)
        {
            CreateTask(batchId, machineId, userId);
            TaskDBServices dbServices = new TaskDBServices();
            return dbServices.ScheduleTask(batchId, machineId,userId ,startTimeEst, endTimeEst);
        }
    }
}

using System.Reflection.PortableExecutable;
using System;
using FlowServer.DBServices;
namespace FlowServer.Models
{
    public class Task
    {
        public int id { get; set; } // id of the task
        public int batchId { get; set; }
        public int machineId { get; set; }
        public DateTime startTimeEst { get; set; }
        public DateTime actStartTime { get; set; }
        public DateTime actEndTime { get; set; }
        public string status { get; set; }
        public DateTime? endTimeEst { get; internal set; }
        public int units { get; set; }
        public string name { get; set; } // name of the product
        public float processTime { get; set; } // in minutes
        public float progress { get; set; } // in percentage     
        public bool inQueue { get; set; } // 1 or 0
        public int type { get; set; } 
        public string color { get; set; } // color of the task
        public int flow {  get; set; }
        public int temp { get; set; } // temperature of the task


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

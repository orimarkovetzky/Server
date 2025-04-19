using FlowServer.DBServices;

namespace FlowServer.Models
{
    public class Queue
    {
        //public static List<Task> GetMachineQueue(int machineId)
        //{
        //    QueueDBServices dbServices = new QueueDBServices();
        //    return dbServices.GetMachineQueue(machineId);
        //}

        public static List<Object> GetMachinePageData()
        {
           QueueDBServices dbServices = new QueueDBServices();
            return dbServices.GetMachinePageData();
        }

    }
}

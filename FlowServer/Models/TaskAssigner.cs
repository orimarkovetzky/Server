using FlowServer.DBServices;

namespace FlowServer.Models
{
    public class TaskAssigner
    {
        public static void AssignBatchToQueues(Batch batch)
        {
            //List<Machine> machines = Machine.ReadMachines(); // Assuming a method to read all machines

            //// Get the sequence of machine types based on the product type
            //List<int> machineSequence = GetMachineSequence(batch.ProductType);

            //foreach (var machineType in machineSequence)
            //{
            //    Machine machine = FindBestMachine(machines, machineType);
            //    if (machine != null)
            //    {
            //        DateTime startTimeEst = DateTime.Now; // Placeholder for actual start time estimation logic
            //        DateTime endTimeEst = startTimeEst.AddHours(1); // Placeholder for actual end time estimation logic

            //        TaskDBServices dbServices = new TaskDBServices();
            //        dbServices.ScheduleTask(batch.BatchId, machine.MachineId, startTimeEst, endTimeEst);

            //        // Update machine status or other relevant properties
            //        machine.Status = 1; // Assuming 1 means the machine is now busy
            //    }
            //}
        }

        private static List<int> GetMachineSequence(int productType)
        {
            if (productType == 0)
            {
                return new List<int> { 1, 2, 3 };
            }
            else if (productType == 1)
            {
                return new List<int> { 1, 3 };
            }
            return new List<int>();
        }

    }
}

using FlowServer.DBServices;

namespace FlowServer.Models
{
    public class TaskAssigner
    {
        public static void AssignBatchToQueues(Batch batch,int userId)
        {
            TaskDBServices taskService = new TaskDBServices();

            // Step 1: Get the machine sequence for the product
            List<int> machineSequence = GetMachineSequence(batch.ProductType);

            DateTime currentBatchTime = DateTime.Now;

            foreach (int machineType in machineSequence)
            {
                // Step 2: Get available queues for the current machine type
                Dictionary<int, List<Task>> machineQueues = GetQueuesByMachineType(machineType);

                // Step 3: Find the best machine (available soonest)
                int bestMachineId = FindBestMachine(machineQueues);

                if (bestMachineId == -1)
                {
                    // No available machine found
                    throw new Exception($"No available machine found for machine type {machineType}");
                }

                // Step 4: Find when the machine is free
                List<Task> bestMachineTasks = machineQueues[bestMachineId];
                DateTime machineAvailableTime;

                if (bestMachineTasks.Count == 0)
                {
                    machineAvailableTime = DateTime.Now;
                }
                else
                {
                    machineAvailableTime = bestMachineTasks.Max(t => t.endTimeEst ?? DateTime.Now);
                }

                DateTime startTimeEst = (machineAvailableTime > currentBatchTime) ? machineAvailableTime : currentBatchTime;

                int processMinutes = batch.GetProcessTimeMinutes(batch.ProductId, machineType);
                TimeSpan processDuration = TimeSpan.FromMinutes(processMinutes);
                TimeSpan delayBuffer = TimeSpan.FromMinutes(1);
                DateTime endTimeEst = startTimeEst.Add(processDuration).Add(delayBuffer);

                // Step 6: Schedule the task in DB
                taskService.ScheduleTask(batch.BatchId, bestMachineId, userId,startTimeEst, endTimeEst);

                // 🛠 NEW: Update batch's running clock
                currentBatchTime = endTimeEst;
            }
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

        public static Dictionary<int, List<Task>> GetQueuesByMachineType(int machineType)
{
    Dictionary<int, List<Task>> machineQueues = new Dictionary<int, List<Task>>();

    MachineDBServices machineService = new MachineDBServices();
    TaskDBServices taskService = new TaskDBServices();

    // Get all machines
    List<Machine> machines = machineService.ReadMachines();

    // Filter machines by type
    var filteredMachines = machines.Where(m => m.MachineType == machineType).ToList();

    foreach (var machine in filteredMachines)
    {
        List<Task> queue = machineService.GetMachineQueue(machine.MachineId);
        machineQueues[machine.MachineId] = queue;
    }

    return machineQueues;
}

        public static int FindBestMachine(Dictionary<int, List<Task>> machineQueues)
        {
            int bestMachineId = -1;
            DateTime earliestAvailable = DateTime.MaxValue;

            foreach (var machineQueue in machineQueues)
            {
                int machineId = machineQueue.Key;
                List<Task> tasks = machineQueue.Value;

                DateTime availableTime;

                if (tasks.Count == 0)
                {
                    // No tasks, machine is free now
                    availableTime = DateTime.Now;
                }
                else
                {
                    // Machine is busy, check last task's end time
                    availableTime = tasks.Max(t => t.endTimeEst ?? DateTime.Now);
                }

                if (availableTime < earliestAvailable)
                {
                    earliestAvailable = availableTime;
                    bestMachineId = machineId;
                }
            }

            return bestMachineId;
        }
    }
}

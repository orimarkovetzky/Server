using FlowServer.DBServices;

namespace FlowServer.Models
{
    public class TaskAssigner
    {
        public static void AssignBatchToQueues(Batch batch, int userId)
        {
            TaskDBServices taskService = new TaskDBServices();
            MachineDBServices machineService = new MachineDBServices();


            // Step 1: Get the machine sequence for the product
            List<int> machineSequence = GetMachineSequence(batch.ProductType);
            DateTime currentBatchTime = DateTime.Now;

            foreach (int machineType in machineSequence)
            {
                // Step 2: Get available queues for the current machine type
                Dictionary<int, List<Task>> machineQueues = GetQueuesByMachineType(machineType);

                // Load machines and build machineMap for setup times
                List<Machine> machines = machineService.ReadFunctionalMachines();
                Dictionary<int, Machine> machineMap = machines
                    .Where(m => m.MachineType == machineType)
                    .ToDictionary(m => m.MachineId, m => m);

                int bestMachineId;

                // 🔍 Step 3: Smart selection for nitrogen machines only
                if (machineType == 1) // nitrogen
                {
                    var taskSettings = TaskDBServices.GetSettings(batch.ProductId, machineType);
                    int[] targetSettings = new int[]
                    {
                Convert.ToInt32(taskSettings[0]), //FLOW
                Convert.ToInt32(taskSettings[1]) //TEMP
                    };

                    bestMachineId = FindBestMachineSmart(machineQueues, machineMap, targetSettings);
                }
                else
                {
                    // Default logic for grinding/washing
                    bestMachineId = FindBestMachine(machineQueues);
                }

                if (bestMachineId == -1)
                {
                    throw new Exception($"No available machine found for machine type {machineType}");
                }

                // Step 4: When is the machine free?
                List<Task> bestMachineTasks = machineQueues[bestMachineId];
                DateTime machineAvailableTime = bestMachineTasks.Count == 0
                    ? DateTime.Now
                    : bestMachineTasks.Max(t => t.endTimeEst ?? DateTime.Now);

                DateTime startTimeEst = (machineAvailableTime > currentBatchTime)
                    ? machineAvailableTime
                    : currentBatchTime;

                // Step 5: Calculate estimated end time
                int processMinutes = batch.GetProcessTimeMinutes(batch.ProductId, machineType);
                TimeSpan processDuration = TimeSpan.FromMinutes(processMinutes);
                TimeSpan delayBuffer = TimeSpan.FromMinutes(1);
                DateTime endTimeEst = startTimeEst.Add(processDuration).Add(delayBuffer);

                // Step 6: Schedule task
                taskService.ScheduleTask(batch.BatchId, bestMachineId, userId, startTimeEst, endTimeEst);

                // Step 7: Update batch current time for next step
                currentBatchTime = endTimeEst;
            }
        }
        private static List<int> GetMachineSequence(int productType)
        {
            if (productType == 0)
            {
                return new List<int> { 2, 3 };
            }
            else if (productType == 1)
            {
                return new List<int> { 1, 2, 3 };
            }
            return new List<int>();
        }

        public static Dictionary<int, List<Task>> GetQueuesByMachineType(int machineType)
        {
            Dictionary<int, List<Task>> machineQueues = new Dictionary<int, List<Task>>();

            MachineDBServices machineService = new MachineDBServices();
            TaskDBServices taskService = new TaskDBServices();

            // Get all machines
            List<Machine> machines = machineService.ReadFunctionalMachines();


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

        public static int FindBestMachineSmart(
    Dictionary<int, List<Task>> machineQueues,
    Dictionary<int, Machine> machineMap,
    int[] targetSettings)
        {
            int bestMachineId = -1;
            double bestScore = double.MaxValue;

            foreach (var entry in machineQueues)
            {
                int machineId = entry.Key;
                List<Task> tasks = entry.Value;

                DateTime availableTime = tasks.Count == 0
                    ? DateTime.Now
                    : tasks.Max(t => t.endTimeEst ?? DateTime.Now);

                double timeUntilFree = (availableTime - DateTime.Now).TotalMinutes;
                if (timeUntilFree < 0) timeUntilFree = 0;

                double setupCost = 0;

                if (tasks.Count > 0)
                {
                    Task lastTask = tasks.Last();
                    double tempDifference = Math.Abs(lastTask.temp - targetSettings[1]);
                    double setupTime = machineMap[machineId].SetupTime;

                    setupCost = tempDifference * setupTime;
                }

                double score = timeUntilFree + setupCost;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestMachineId = machineId;
                }
            }

            return bestMachineId;
        }


        public static void AssignRemainingSteps(Batch batch, int startingStepIndex)
        {
            TaskDBServices taskService = new TaskDBServices();
            MachineDBServices machineService = new MachineDBServices();

            List<int> fullSequence = GetMachineSequence(batch.ProductType);
            List<int> remainingSequence = fullSequence.Skip(startingStepIndex).ToList();

            DateTime currentTime = DateTime.Now;

            foreach (int machineType in remainingSequence)
            {
                // Use the same scheduling logic as AssignBatchToQueues
                Dictionary<int, List<Task>> machineQueues = GetQueuesByMachineType(machineType);
                List<Machine> machines = machineService.ReadFunctionalMachines();
                Dictionary<int, Machine> machineMap = machines
                    .Where(m => m.MachineType == machineType)
                    .ToDictionary(m => m.MachineId, m => m);

                var taskSettings = TaskDBServices.GetSettings(batch.ProductId, machineType);
                int bestMachineId = FindBestMachineSmart(machineQueues, machineMap, taskSettings);

                if (bestMachineId == -1)
                    throw new Exception($"No available machine for machineType {machineType}");

                List<Task> bestMachineTasks = machineQueues[bestMachineId];
                DateTime machineAvailableTime = bestMachineTasks.Count == 0
                    ? DateTime.Now
                    : bestMachineTasks.Max(t => t.endTimeEst ?? DateTime.Now);

                DateTime startTimeEst = machineAvailableTime > currentTime ? machineAvailableTime : currentTime;
                int processMinutes = batch.GetProcessTimeMinutes(batch.ProductId, machineType);
                DateTime endTimeEst = startTimeEst.AddMinutes(processMinutes).AddMinutes(1);

                taskService.ScheduleTask(batch.BatchId, bestMachineId, 1, startTimeEst, endTimeEst);
                currentTime = endTimeEst;
            }
        }


        public static void HandleMachineFailure(int machineId)
        {
            MachineDBServices machineService = new MachineDBServices();
            TaskDBServices taskService = new TaskDBServices();

            List<Task> failedTasks = machineService.GetMachineQueue(machineId);
            var affectedBatchIds = failedTasks.Select(t => t.batchId).Distinct();

            foreach (int batchId in affectedBatchIds)
            {
                bool hasProcessingTask = failedTasks.Any(t =>
                    t.batchId == batchId &&
                    t.status.Equals("Processing", StringComparison.OrdinalIgnoreCase));

                if (hasProcessingTask)
                {
                    // Full failure
                    Batch.UpdateBatchStatus(batchId, "ToBeRemade");
                    taskService.DeleteTasksByBatch(batchId);
                    Batch batch = Batch.FindBatch(batchId);
                }
                else
                {
                    // Partial failure

                    Batch batch = Batch.FindBatch(batchId);
                    int failedMachineType = machineService.FindMachine(machineId).MachineType;
                    taskService.DeletePendingTasksByBatchAndMachine(batchId, failedMachineType);

                    List<int> machineSequence = GetMachineSequence(batch.ProductType);
                    int startingIndex = machineSequence.FindIndex(mt => mt == failedMachineType);

                    if (startingIndex == -1)
                    {
                        Console.WriteLine($"⚠️ No remaining steps to reschedule for batch {batchId}");
                        continue;
                    }

                    AssignRemainingSteps(batch, startingIndex);
                }
            }
        }
    }
  }

using FlowServer.DBServices;

namespace FlowServer.Models
{
    public class Machine
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public int MachineType { get; set; }
        public double SetupTime { get; set; }
        public int Status { get; set; }
        public string ImagePath { get; set; }


        public Machine(int machineId, string machineName, int machineType, double setupTime, int status, string imagePath)
        {
            MachineId = machineId;
            MachineName = machineName;
            MachineType = machineType;
            ImagePath = imagePath;
            SetupTime = setupTime;
            Status = status;
        }
        public Machine()
        {

        }
        public bool AddMachine(string machineName, int machineType, double setupTime, string imagePath = null)
        {
            MachineDBServices dbs = new MachineDBServices();
            return dbs.AddMachine(machineName, machineType, setupTime, imagePath);
        }
        public bool DeleteMachine(string machineName)
        {
            MachineDBServices dbs = new MachineDBServices();
            return dbs.DeleteMachine(machineName);
        }

        public static List<Machine> ReadMachines()
        {
            MachineDBServices dbs = new MachineDBServices();
            return dbs.ReadMachines();
        }

        public static int UpdateMachineStatus(int machineId, int newStatus)
        {
            MachineDBServices dbs = new MachineDBServices();
           return dbs.UpdateMachineStatus(machineId, newStatus);
        }

        public static Machine FindMachine(int machineId)
        {
            MachineDBServices dbs = new MachineDBServices();
            return dbs.FindMachine(machineId);
        }

        public static int GetNonOperationalMachineCount()
        {
            MachineDBServices dbs = new MachineDBServices();
            return dbs.GetNonOperationalMachineCount();
        }
    }
}
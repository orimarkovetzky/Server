﻿using FlowServer.DBServices;

namespace FlowServer.Models
{
    public class Machine
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public int MachineType { get; set; }
        public double SetupTime { get; set; }
        public int Status { get; set; }


        public Machine(int machineId, string machineName, int machineType, float setupTime, int status)
        {
            MachineId = machineId;
            MachineName = machineName;
            MachineType = machineType;
            SetupTime = setupTime;
            Status = status;
        }
        public Machine()
        {

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
            return dbs.findMachine(machineId);
        }
    }
}
using System.Collections.Generic;

namespace ProjectEmployeesTimeRecording.Domain.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<WorkLog> WorkLogs { get; set; }

        public Employee()
        {
            WorkLogs = new List<WorkLog>();
        }
    }
}

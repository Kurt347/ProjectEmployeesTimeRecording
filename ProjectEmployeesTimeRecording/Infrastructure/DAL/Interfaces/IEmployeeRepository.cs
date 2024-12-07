using ProjectEmployeesTimeRecording.Domain.Models;
using System.Collections.Generic;

namespace ProjectEmployeesTimeRecording.Infrastructure.DAL.Interfaces
{
    public interface IEmployeeRepository
    {
        void AddEmployee(Employee employee);
        Employee GetEmployeeById(int id);
        Employee GetEmployeeByName(string name);
        void AddWorkLog(WorkLog workLog);
        WorkLog GetLastWorkLog(int employeeId);
        void UpdateWorkLog(WorkLog workLog);
        List<WorkLog> GetWorkLogsByEmployeeId(int employeeId);
    }
}

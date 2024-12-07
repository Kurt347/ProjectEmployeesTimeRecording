using ProjectEmployeesTimeRecording.Domain.Models;
using ProjectEmployeesTimeRecording.Infrastructure.DAL.Interfaces;
using System;

namespace ProjectEmployeesTimeRecording.BLL.Services
{
    public class EmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public void AddEmployee(string name)
        {
            var employee = new Employee { Name = name };
            _employeeRepository.AddEmployee(employee);
        }

        public Employee GetEmployeeByNameOrId(string identifier)
        {
            if (int.TryParse(identifier, out int id))
            {
                return _employeeRepository.GetEmployeeById(id);
            }
            else
            {
                return _employeeRepository.GetEmployeeByName(identifier);
            }
        }

        public void RegisterCheckIn(Employee employee, DateTime checkInTime)
        {
            var workLog = new WorkLog
            {
                EmployeeId = employee.Id,
                CheckInTime = checkInTime
            };
            _employeeRepository.AddWorkLog(workLog);
        }

        public void RegisterCheckOut(Employee employee, DateTime checkOutTime)
        {
            var workLog = _employeeRepository.GetLastWorkLog(employee.Id);
            if (workLog != null && !workLog.CheckOutTime.HasValue)
            {
                workLog.CheckOutTime = checkOutTime;
                _employeeRepository.UpdateWorkLog(workLog);
            }
        }
    }
}

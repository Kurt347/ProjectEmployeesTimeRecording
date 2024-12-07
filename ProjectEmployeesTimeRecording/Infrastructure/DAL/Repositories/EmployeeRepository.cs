using Dapper;
using Npgsql;
using ProjectEmployeesTimeRecording.Domain.Models;
using ProjectEmployeesTimeRecording.Infrastructure.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectEmployeesTimeRecording.Infrastructure.DAL.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddEmployee(Employee employee)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var sql = "INSERT INTO Employees (name) VALUES (@Name) RETURNING id";
                employee.Id = connection.QuerySingle<int>(sql, new { Name = employee.Name });
            }
        }

        public Employee GetEmployeeById(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var employee = connection.QuerySingleOrDefault<Employee>(
                    "SELECT * FROM Employees WHERE Id = @Id", new { Id = id });
                if (employee != null)
                {
                    employee.WorkLogs = GetWorkLogsByEmployeeId(employee.Id);
                }
                return employee;
            }
        }

        public Employee GetEmployeeByName(string name)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var employees = connection.Query<Employee>(
                    "SELECT * FROM Employees WHERE Name = @Name", new { Name = name }).ToList();

                if (employees.Count == 1)
                {
                    var employee = employees.First();
                    employee.WorkLogs = GetWorkLogsByEmployeeId(employee.Id);
                    return employee;
                }
                else if (employees.Count > 1)
                {
                    // Если найдено несколько сотрудников с одинаковым именем
                    throw new Exception("Найдено несколько сотрудников с таким именем. Пожалуйста, используйте ID сотрудника.");
                }
                else
                {
                    return null;
                }
            }
        }

        public void AddWorkLog(WorkLog workLog)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                // Используем RETURNING Id для получения идентификатора новой записи
                workLog.Id = connection.QuerySingle<int>(
                    "INSERT INTO WorkLogs (EmployeeId, CheckInTime) VALUES (@EmployeeId, @CheckInTime) RETURNING Id", workLog);
            }
        }

        public WorkLog GetLastWorkLog(int employeeId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault<WorkLog>(
                    "SELECT * FROM WorkLogs WHERE EmployeeId = @EmployeeId ORDER BY CheckInTime DESC LIMIT 1",
                    new { EmployeeId = employeeId });
            }
        }

        public void UpdateWorkLog(WorkLog workLog)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Execute(
                    "UPDATE WorkLogs SET CheckOutTime = @CheckOutTime WHERE Id = @Id",
                    workLog);
            }
        }

        public List<WorkLog> GetWorkLogsByEmployeeId(int employeeId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return connection.Query<WorkLog>(
                    "SELECT * FROM WorkLogs WHERE EmployeeId = @EmployeeId",
                    new { EmployeeId = employeeId }).ToList();
            }
        }
    }
}

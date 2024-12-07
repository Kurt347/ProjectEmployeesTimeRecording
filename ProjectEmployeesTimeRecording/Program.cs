using System;
using ProjectEmployeesTimeRecording.Infrastructure.UI;
using ProjectEmployeesTimeRecording.BLL.Services;
using ProjectEmployeesTimeRecording.Infrastructure.DAL.Repositories;

namespace ProjectEmployeesTimeRecording
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = AppSettings.Load("appsettings.json");

            string botToken = settings.BotToken;
            string connectionString = settings.ConnectionString;

            var repository = new EmployeeRepository(connectionString);
            var service = new EmployeeService(repository);
            var telegramUI = new TelegramUI(botToken, service);

            telegramUI.StartBot();

            Console.WriteLine("Бот работает. Нажмите любую клавишу для остановки...");
            Console.ReadKey();

            telegramUI.StopBot();
        }
    }
}

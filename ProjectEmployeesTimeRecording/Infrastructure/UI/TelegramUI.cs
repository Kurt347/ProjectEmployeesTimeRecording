using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ProjectEmployeesTimeRecording.BLL.Services;
using ProjectEmployeesTimeRecording.Domain.Models;

namespace ProjectEmployeesTimeRecording.Infrastructure.UI
{
    public class TelegramUI
    {
        private readonly TelegramBotClient _botClient;
        private readonly EmployeeService _employeeService;

        // Dictionary to store user states (pending commands)
        private readonly Dictionary<long, string> _userStates = new Dictionary<long, string>();

        public TelegramUI(string botToken, EmployeeService employeeService)
        {
            _botClient = new TelegramBotClient(botToken);
            _employeeService = employeeService;
        }

        public void StartBot()
        {
            var cancellationToken = new CancellationTokenSource().Token;

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new Telegram.Bot.Polling.ReceiverOptions(),
                cancellationToken
            );

            Console.WriteLine("Бот запущен");
        }

        public void StopBot()
        {
            _botClient.CloseAsync();
            Console.WriteLine("Бот остановлен");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message)
                return;

            var message = update.Message;

            if (message.Text != null)
            {
                var chatId = message.Chat.Id;
                var text = message.Text.Trim();

                // Проверяем, есть ли у пользователя незавершенная команда
                if (_userStates.ContainsKey(chatId))
                {
                    var pendingCommand = _userStates[chatId];
                    _userStates.Remove(chatId);
                    await HandleCommandWithParameters(message, pendingCommand, text);
                    return;
                }

                var messageParts = text.Split(new[] { ' ' }, 2);
                var command = messageParts[0];
                var parameters = messageParts.Length > 1 ? messageParts[1] : null;

                // Добавляем слеш перед командой, если его нет
                if (!command.StartsWith("/"))
                {
                    command = "/" + command;
                }

                try
                {
                    switch (command.ToLower())
                    {
                        case "/start":
                            await SendMainMenu(chatId);
                            break;

                        case "/new":
                        case "/timein":
                        case "/timeout":
                        case "/report":
                            if (!string.IsNullOrWhiteSpace(parameters))
                            {
                                await HandleCommandWithParameters(message, command, parameters);
                            }
                            else
                            {
                                // Сохраняем команду и запрашиваем дополнительные данные
                                _userStates[chatId] = command;
                                await SendMessageAsync(chatId, "Введите имя или ID сотрудника:");
                            }
                            break;

                        default:
                            await SendMessageAsync(chatId, "Неверная команда");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await SendMessageAsync(chatId, $"Ошибка: {ex.Message}");
                }
            }
        }

        private async Task HandleCommandWithParameters(Message message, string command, string parameters)
        {
            switch (command.ToLower())
            {
                case "/new":
                    await HandleNewCommand(message, parameters);
                    break;

                case "/timein":
                    await HandleTimeInCommand(message, parameters);
                    break;

                case "/timeout":
                    await HandleTimeOutCommand(message, parameters);
                    break;

                case "/report":
                    await HandleReportCommand(message, parameters);
                    break;

                default:
                    await SendMessageAsync(message.Chat.Id, "Неверная команда");
                    break;
            }
        }

        private async Task HandleNewCommand(Message message, string parameters)
        {
            string name = parameters.Trim();
            _employeeService.AddEmployee(name);
            await SendMessageAsync(message.Chat.Id, $"Пользователь {name} успешно создан");
        }

        private async Task HandleTimeInCommand(Message message, string parameters)
        {
            string identifier = parameters.Trim();
            var employee = _employeeService.GetEmployeeByNameOrId(identifier);

            if (employee != null)
            {
                DateTime checkInTime = DateTime.Now;
                _employeeService.RegisterCheckIn(employee, checkInTime);
                await SendMessageAsync(message.Chat.Id, "Время прихода успешно сохранено. Хорошего рабочего дня");
            }
            else
            {
                await SendMessageAsync(message.Chat.Id, "Сотрудник не найден");
            }
        }

        private async Task HandleTimeOutCommand(Message message, string parameters)
        {
            string identifier = parameters.Trim();
            var employee = _employeeService.GetEmployeeByNameOrId(identifier);

            if (employee != null)
            {
                DateTime checkOutTime = DateTime.Now;
                _employeeService.RegisterCheckOut(employee, checkOutTime);
                await SendMessageAsync(message.Chat.Id, "Время ухода успешно сохранено. Спасибо за работу");
            }
            else
            {
                await SendMessageAsync(message.Chat.Id, "Сотрудник не найден");
            }
        }

        private async Task HandleReportCommand(Message message, string parameters)
        {
            string identifier = parameters.Trim();
            var employee = _employeeService.GetEmployeeByNameOrId(identifier);

            if (employee != null)
            {
                var report = GetEmployeeWorkReport(employee);
                await SendMessageAsync(message.Chat.Id, report);
            }
            else
            {
                await SendMessageAsync(message.Chat.Id, "Сотрудник не найден");
            }
        }

        private string GetEmployeeWorkReport(Employee employee)
        {
            if (employee.WorkLogs == null || employee.WorkLogs.Count == 0)
            {
                return $"Нет записей о рабочем времени для сотрудника {employee.Name}.";
            }

            var report = $"Отчет по рабочему времени сотрудника: {employee.Name}\n";
            foreach (var log in employee.WorkLogs)
            {
                var checkOutTime = log.CheckOutTime.HasValue ? log.CheckOutTime.Value.ToString("g") : "Не указан";
                report += $"Приход: {log.CheckInTime:g}, Уход: {checkOutTime}\n";
            }
            return report;
        }

        private Task SendMessageAsync(long chatId, string message)
        {
            return _botClient.SendTextMessageAsync(chatId, message);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка бота: {exception.Message}");
            return Task.CompletedTask;
        }

        // Method to send the main menu with command buttons
        private async Task SendMainMenu(long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "new", "timein", "timeout" },
                new KeyboardButton[] { "report" }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите команду:",
                replyMarkup: keyboard
            );
        }
    }
}

using System.IO;
using Newtonsoft.Json;

namespace ProjectEmployeesTimeRecording
{
    public class AppSettings
    {
        public string BotToken { get; set; }
        public string ConnectionString { get; set; }

        public static AppSettings Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<AppSettings>(json);
        }
    }
}

using System.Net.Mail;

namespace SampleCosmosCore2App.Notifications
{
    public class EmailErrorNotifierOptions
    {
        public object EnvironmentName { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
    }
}
using System.Net.Mail;

namespace GymManagement.ViewModels
{
    public class EmailMessage
    {
        public List<EmailAddress> ToAddresses { get; set; } = new List<EmailAddress>();
        public string Subject { get; set; } = "";
        public string Content { get; set; } = "";
    }
}

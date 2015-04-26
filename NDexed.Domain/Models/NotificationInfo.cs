
namespace NDexed.Domain.Models
{
    public enum NotificationType
    {
        Email = 0,
        SMS = 1
    }
    public class NotificationInfo
    {
        public NotificationType NotificationType { get; set; }
        public string Address{get;set;}
    }
}

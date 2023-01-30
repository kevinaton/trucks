using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.DriverApp.Messages.Dto
{
    public class PostInput
    {
        public long TargetUserId { get; set; }

        [Required]
        [StringLength(EntityStringFieldLengths.ChatMessage.Message)]
        public string Message { get; set; }
    }
}

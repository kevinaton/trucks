using System;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.Emailing.Dto
{
    public class TrackEmailOpenInput : EntityDto<Guid>
    {
        public TrackEmailOpenInput()
        {
        }

        public TrackEmailOpenInput(Guid id, string email)
            : base(id)
        {
            Email = email;
        }

        public string Email { get; set; }
    }
}

using Abp.Domain.Entities;

namespace DispatcherWeb.HostEmails
{
    public class HostEmailRole : Entity
    {
        public int HostEmailId { get; set; }
        public virtual HostEmail HostEmail { get; set; }

        public string RoleName { get; set; }
    }
}

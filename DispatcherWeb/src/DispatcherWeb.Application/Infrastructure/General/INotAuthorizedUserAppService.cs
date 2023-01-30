namespace DispatcherWeb.Infrastructure.General
{
	public interface INotAuthorizedUserAppService
	{
		string GetTenancyNameOrNull(int? tenantId);
	}
}
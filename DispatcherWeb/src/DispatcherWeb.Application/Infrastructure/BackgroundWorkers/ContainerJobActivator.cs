namespace DispatcherWeb.Infrastructure.BackgroundWorkers
{
    //TODO: Do we need this?
    //Looks like this was added simultaneously with the ScheduledReports, and the ScheduledReports work without this, so we might not need this
    //I'll keep this commented out for now and we can remove it after more testing is done
    //public class ContainerJobActivator : JobActivator
    //{
    //    private readonly IIocManager _iocManager;

    //    public ContainerJobActivator(IIocManager iocManager)
    //    {
    //        _iocManager = iocManager;
    //    }

    //    public override object ActivateJob(Type jobType)
    //    {
    //        return _iocManager.Resolve(jobType);
    //    }
    //}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.EntityUpdaters
{
    public interface IEntityUpdaterFactory<TEntity>
    {
        IEntityUpdater<TEntity> Create(int entityId);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Infrastructure.EntityReadonlyCheckers
{
    public interface IReadonlyCheckerFactory<T>
    {
        IReadonlyChecker<T> Create(int entityId);
    }
}

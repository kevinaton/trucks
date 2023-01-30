using System.Linq;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Migrations.Seed.Tenants
{
    public class DefaultUnitOfMeasureCreator
    {
        private readonly DispatcherWebDbContext _context;
        private readonly int _tenantId;

        public DefaultUnitOfMeasureCreator(DispatcherWebDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            if (!_context.UnitsOfMeasure.Any(x => x.TenantId == _tenantId))
            {
                var uoms = new[] { "Hours", "Tons", "Loads", "Cubic Yards", "Each", "Cubic Meters", "Miles" }.ToList();
                uoms.ForEach(uom => _context.UnitsOfMeasure.Add(new UnitOfMeasure { TenantId = _tenantId, Name = uom }));
                _context.SaveChanges();
            }
        }
    }
}

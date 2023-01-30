using System.Linq;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Locations;

namespace DispatcherWeb.Migrations.Seed.Tenants
{
    public class DefaultLocationsCreator
    {
        private readonly DispatcherWebDbContext _context;
        private readonly int _tenantId;

        public DefaultLocationsCreator(DispatcherWebDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            //if (!_context.Locations.Any(x => x.TenantId == _tenantId && x.PredefinedLocationKind == PredefinedLocationKind.InitialLoadAt))
            //{
            //    _context.Locations.Add(new Location { TenantId = _tenantId, Name = "Initial Load At", IsActive = true, PredefinedLocationKind = PredefinedLocationKind.InitialLoadAt });
            //    _context.SaveChanges();
            //}

            if (!_context.LocationCategories.Any(x => x.TenantId == _tenantId))
            {
                var categories = new[]
                {
                    new LocationCategory { TenantId = _tenantId, Name = "Asphalt Plant", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.AsphaltPlant },
                    new LocationCategory { TenantId = _tenantId, Name = "Concrete Plant", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.ConcretePlant },
                    new LocationCategory { TenantId = _tenantId, Name = "Landfill/Recycling", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.LandfillOrRecycling },
                    new LocationCategory { TenantId = _tenantId, Name = "Miscellaneous", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Miscellaneous },
                    new LocationCategory { TenantId = _tenantId, Name = "Yard", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Yard },
                    new LocationCategory { TenantId = _tenantId, Name = "Quarry", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Quarry },
                    new LocationCategory { TenantId = _tenantId, Name = "Sand Pit", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.SandPit },
                    new LocationCategory { TenantId = _tenantId, Name = "Project Site", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.ProjectSite },
                    new LocationCategory { TenantId = _tenantId, Name = "Temporary", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Temporary },
                    new LocationCategory { TenantId = _tenantId, Name = "Unknown Load Site", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.UnknownLoadSite },
                    new LocationCategory { TenantId = _tenantId, Name = "Unknown Delivery Site", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.UnknownDeliverySite },
                };
                categories.ToList().ForEach(x => _context.LocationCategories.Add(x));
                _context.SaveChanges();

                //NWW data
                //var locationGroups = new[]
                //{
                //    new { Category = PredefinedLocationCategoryKind.AsphaltPlant, Names = new[] { "Ashmore Plant 1", "Banks Const. Banco Plant", "Banks Const. Summerville", "CRJ Columbia", "F & R Gray Court", "F & R Lyman", "F & R Pendleton", "King Liberty", "King Simpsonville", "Pickens Construction", "Satterfield Blair", "Satterfield Eureka", "Satterfield Stoney Pt.", "Sloan Blacksburg Plant", "Sloan Blacksburg Quarry", "Sloan Duncan", "Sloan Pacolet" } },
                //    new { Category = PredefinedLocationCategoryKind.ConcretePlant, Names = new[] { "Century" } },
                //    new { Category = PredefinedLocationCategoryKind.LandfillOrRecycling, Names = new[] { "Ashmore Recyclers", "Berkeley County Landfill/Recycling", "Carolina Materials", "Carolina Waste Landfill/Recycling", "Carolina Wrecking", "Concrete Recyclers", "378 Landfill", "Barr Construction", "Bees Ferry Landfill", "Bees Ferry Landfill", "LCR Recycling", "Northeast Landfill/Recycling", "Oakridge Landfill/Recycling", "Oconee Rock Crusher", "Old Romney St Landfill", "Spring grove Landfill/Recycling", "Twin Chimney Landfill", "Waste Industries Screaming Eagle", "WM Richland Landfill", "Northeast Landfill", "Pepperhill", "Republic Mauldin Road Landfill/Recycling", "Republic Savannah", "Republic Union County Landfill/Recycling", "Hickory Hill Landfill", "WM Palmetto Landfill", "WM Richland Landfill", "Curry Lake Landfill/Recycling", "Shilo Landfill/Recycling" } },
                //    new { Category = PredefinedLocationCategoryKind.Miscellaneous, Names = new[] { "BHC Yard Greenwood", "Caledonia", "Charleston Mill", "JRD", "Keywell", "MRR Southern", "Garco", "Landmark 212 Cooper Store Rd.", "Midtown Pond Fill", "Profile Products", "Olympic Mill Services", "Palmetto Aggregates", "RG 1385 Ferry Rd", "Cross Generating Station", "Winyah Generating Station", "SCMI", "SR Grading", "SRR Newberry", "Stalite", "Strange Bros. Wade Hampton", "Tankersly Brothers", "Upstate Mulch Spindale" } },
                //    new { Category = PredefinedLocationCategoryKind.Yard, Names = new[] { "NWW Anderson", "NWW Charleston", "NWW Columbia", "NWW Greenwood", "NWW Greer" } },
                //    new { Category = PredefinedLocationCategoryKind.Quarry, Names = new[] { "Bluegrass Quarry", "Hanson Anderson", "Hanson Clinton", "Hanson Jefferson", "Hanson Lowrys", "Hanson Marlboro", "Hanson Pelham", "Hanson Sandy Flats", "Hanson Toccoa", "Inman Stone", "MM 215", "MM Arrowood", "MM Augusta", "MM Berkeley", "MM Cayce", "MM Chesterfield", "MM Georgetown Yard", "MM Jamestown", "MM Kinder Morgan Yard", "MM Kings Mountain", "MM Loamy Sand & Gravel", "MM Montague", "MM Myrtle Beach Yard", "MM Rock Hill", "MM Summerville Railyard", "MM Warrenton Quarry", "Newport Sand & Gravel", "Aggregates USA", "Buckhorn Materials", "DECO", "Mid South Aggregates Warren County", "Rogers Group Quarry", "Vulcan 215", "Vulcan Anderson", "Vulcan Aviation (Charleston)", "Vulcan Blacksburg", "Vulcan Blair", "Vulcan Gray Court", "Vulcan Greenwood", "Vulcan Hendersonville", "Vulcan Lakeside", "Vulcan Liberty", "Vulcan Lyman", "Vulcan Mount Holly", "Vulcan Olympia", "Vulcan Pacolet", "Vulcan Pineville", "Vulcan Rains Yard", "Vulcan Ridgeland", "Vulcan Rockingham" } },
                //    new { Category = PredefinedLocationCategoryKind.SandPit, Names = new[] { "Atlas Way", "Austin's Sandhill Pit", "Guerard Pit", "B & T Services", "B&T Sand Edmund", "B&T Sand Gilbert", "Belo Pit", "BHC Dirt Pit Greenwood Highway 10", "Burdett's Pit", "Carolina 41 Pit", "Carter's Pit", "Awendaw Pit", "Collins Back Pasture", "Collins Hill", "Collins Hyde Park", "Collins Van Ess Pit", "Highway 27 Pit", "Red Oak", "CSS Pine Ridge", "CSS Shuler", "Danzler Pit", "Demtek Pit", "Frazier's Pit", "Glasscock", "Columbia Sand", "McIntire Sand", "Murray Mines", "OL Thompson Cainhoy 1", "OL Thompson Green Bay Pit", "OL Thompson Laurel Oaks Pit", "3 Oaks Chicken Farm Pit", "Affordable Pit", "Cale Till Pit", "Carolina Aggregates", "Deerfield Sand", "Dirt Cheap Pit", "Doar Rd Pit", "Dr. Williamson Pit", "Early Branch Pit", "Haile Pit", "Hill Pit", "John Porth Pit", "Johnny Williamson Pit", "Keys Dairy Pit", "LA Stokes Pit", "Lanier Sand", "Lempa Farms Pit", "Lenny Buckner Pit", "MC Dirt", "MEM Pit", "Padgett Pit", "RGW pit", "Richardson RNDC", "Richardson Two Notch", "SC Minerals", "Tom's Creek Pit", "Tuttle Pit", "Unimin-Lugoff Pit", "Wellborne Pit", "Williams C&D Landfill", "Woodland Pit", "Palmetto Sand", "S & S Dirt Pit", "Sanders Bros. Berkeley", "Sanders Bros. Silver Mine", "Clay Pit", "County Line Pit", "W. Frazier - St. John's", "Wilson Sand" } },
                //    new { Category = PredefinedLocationCategoryKind.Temporary, Names = new[] { "Conrad Yelvington Distributors", "Ladson Wood Recycling", "Oldcastle Stone", "W.W. Williams Landfill" } }
                //};
                //var locationsToAdd = new List<Supplier>();
                //foreach (var group in locationGroups)
                //{
                //    var categoryId = categories.FirstOrDefault(x => x.PredefinedLocationCategoryKind == group.Category)?.Id;
                //    locationsToAdd.AddRange(group.Names.Select(x => new Supplier { TenantId = _tenantId, Name = x, CategoryId = categoryId }));
                //}
                //locationsToAdd.ForEach(x => _context.Locations.Add(x));
                //_context.SaveChanges();
            }
        }
    }
}

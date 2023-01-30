using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.IntelliShift
{
    public class TruckUnitDto
    {
        [JsonProperty("plateNumber")]
        public string PlateNumber { get; set; }

        [JsonProperty("registrationExpiration")]
        public DateTime? RegistrationExpiration { get; set; }

        [JsonProperty("registrationState")]
        public string RegistrationState { get; set; }

        [JsonProperty("odometer")]
        public decimal? Odometer { get; set; }

        [JsonProperty("tripOdometer")]
        public decimal? TripOdometer { get; set; }

        [JsonProperty("cumulativeHours")]
        public decimal? CumulativeHours { get; set; }

        [JsonProperty("cumulativeHoursThrough")]
        public DateTime CumulativeHoursThrough { get; set; }

        [JsonProperty("vin")]
        public string Vin { get; set; }

        [JsonProperty("imei")]
        public string Imei { get; set; }

        [JsonProperty("makeModelText")]
        public string MakeModelText { get; set; }

        [JsonProperty("make")]
        public string Make { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("trim")]
        public string Trim { get; set; }

        [JsonProperty("fuelCapacity")]
        public double? FuelCapacity { get; set; }

        [JsonProperty("colorText")]
        public string ColorText { get; set; }

        [JsonProperty("grossWeight")]
        public double? GrossWeight { get; set; }

        [JsonProperty("emptyWeight")]
        public double? EmptyWeight { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("branchId")]
        public int? BranchId { get; set; }

        [JsonProperty("assignedOperatorId")]
        public int? AssignedOperatorId { get; set; }

        [JsonProperty("assignedOperatorText")]
        public string AssignedOperatorText { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        public override string ToString() => $"Id:{Id} IsActive:{IsActive} TruckCode:{Name}";

        public static string GetJsonPropertyAttribute(string propertyName)
        {
            var attribute = typeof(TruckUnitDto)
                        .GetProperty(propertyName)
                        .GetCustomAttributes(false)
                        .FirstOrDefault(p => p.GetType() == typeof(JsonPropertyAttribute));

            if (attribute == null)
                return string.Empty;

            var jsonPropertyAttribute = (JsonPropertyAttribute)attribute;
            return jsonPropertyAttribute.PropertyName;
        }
    }

    public class TruckUnitLink
    {
        [JsonProperty("targetUri")]
        public string TargetUri { get; set; }

        [JsonProperty("relationType")]
        public string RelationType { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
    }

    public class TruckUnitsPage
    {
        [JsonProperty("collection")]
        public List<TruckUnitDto> TruckUnitsCollection { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; }

        [JsonProperty("recordCount")]
        public int RecordCount { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("links")]
        public List<TruckUnitLink> TruckUnitLinks { get; set; }

        public static TruckUnitsPage Parse(string jsonSource)
        {
            try
            {
                var results = JsonConvert.DeserializeObject<TruckUnitsPage>(jsonSource);
                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool HasMorePages => PageNumber < TotalPages;
    }


}

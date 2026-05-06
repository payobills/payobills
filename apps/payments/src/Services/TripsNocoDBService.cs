using Payobills.Payments.Data.Contracts.Models;
using Payobills.Payments.NocoDB;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services;

public class TripsNocoDBService
{
    private const string BaseName = "payobills";
    private const string TableName = "trips";
    private readonly NocoDBClientService nocoDBClientService;

    public TripsNocoDBService(NocoDBClientService nocoDBClientService)
    {
        this.nocoDBClientService = nocoDBClientService;
    }

    public async Task<IEnumerable<TripDTO>> GetTripsAsync()
    {
        var page = await nocoDBClientService.GetRecordsPageAsync<Trip>(BaseName, TableName, "*", "l=1000");
        return (page?.List ?? []).Select(ToDTO);
    }

    public async Task<TripDTO> CreateTripAsync(TripCreateDTO input)
    {
        var existing = await GetTripsAsync();
        ValidateNoOverlap(input.StartDate, input.EndDate, existing, excludeId: null);

        var created = await nocoDBClientService.CreateRecordAsync<TripCreateDTO, Trip>(BaseName, TableName, input);
        return ToDTO(created);
    }

    public async Task<TripDTO> UpdateTripAsync(string id, TripUpdateDTO input)
    {
        var existing = await GetTripsAsync();
        ValidateNoOverlap(input.StartDate, input.EndDate, existing, excludeId: id);

        var updated = await nocoDBClientService.UpdateRecordAsync<TripUpdateDTO, Trip>(id, BaseName, TableName, input);
        return ToDTO(updated);
    }

    private static void ValidateNoOverlap(string? startDateStr, string? endDateStr, IEnumerable<TripDTO> existing, string? excludeId)
    {
        if (startDateStr is null || endDateStr is null) return;

        var newStart = DateTime.Parse(startDateStr);
        var newEnd = DateTime.Parse(endDateStr);

        var conflicts = existing.Where(t =>
            t.Id != excludeId &&
            t.StartDate.HasValue &&
            t.EndDate.HasValue &&
            newStart < t.EndDate.Value &&
            newEnd > t.StartDate.Value
        ).ToList();

        if (conflicts.Count > 0)
        {
            var names = string.Join(", ", conflicts.Select(c => c.Title));
            throw new InvalidOperationException($"Date range overlaps with existing trip(s): {names}");
        }
    }

    private static TripDTO ToDTO(Trip trip) => new()
    {
        Id = trip.Id.ToString(),
        Title = trip.Title,
        StartDate = trip.StartDate,
        EndDate = trip.EndDate,
    };
}

using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Orleans;
using Talks.Contracts;
using Talks.Data;

namespace Talks.Grains;

public class MetersSummaryGrain : Grain, IMetersSummary
{
    private readonly Func<MetersDbContext> _factory;
    private double _totalValue;
    private double _monthValue;
    private DateTime _monthStart;

    public MetersSummaryGrain(Func<MetersDbContext> factory)
    {
        _factory = factory;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var dbContext = _factory();

        _monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        
        _totalValue = await dbContext.Measurements
            .SumAsync(x => x.Delta, cancellationToken);

        _monthValue = await dbContext.Measurements
            .Where(m => m.Time > _monthStart)
            .SumAsync(m => m.Delta, cancellationToken);
    }

    public Task<Balance> GetSummary()
    {
        return Task.FromResult(new Balance
        {
            Month = _monthValue,
            Total = _totalValue
        });
    }

    public Task AddMeasurement(double delta)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        if (monthStart != _monthStart)
        {
            _monthValue = delta;
            _monthStart = monthStart;
        }
        else
        {
            _monthValue += delta;
        }

        _totalValue += delta;

        return Task.CompletedTask;
    }
}

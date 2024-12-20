using Microsoft.EntityFrameworkCore;
using Talks.Contracts;
using Talks.Data;

namespace Talks.Grains;

public class MeterGrain : Grain, IMeter
{
    private readonly Func<MetersDbContext> _factory;
    private double _lastValue;
    private DateTime _monthStart;
    private double _monthValue;

    public MeterGrain(Func<MetersDbContext> factory)
    {
        _factory = factory;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var dbContext = _factory();

        _monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        _lastValue = await dbContext.Measurements
            .Where(m => m.MeterId == this.GetPrimaryKeyLong())
            .OrderByDescending(m => m.Time)
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        _monthValue = await dbContext.Measurements
            .Where(m => m.MeterId == this.GetPrimaryKeyLong())
            .Where(m => m.Time > _monthStart)
            .SumAsync(m => m.Delta, cancellationToken);
    }

    public async Task<double> AddMeasurement(double value)
    {
        var context = _factory();

        var delta = value - _lastValue;

        if(DateTime.UtcNow.Month != _monthStart.Month)
        {
            _monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            _monthValue = 0;
        }

        _lastValue = value;

        context.Measurements.Add(new Measurement
        {
            MeterId = this.GetPrimaryKeyLong(),
            Delta = delta,
            Value = value,
            Time = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        await GrainFactory.GetGrain<IMetersSummary>(0).AddMeasurement(delta);

        return delta;
    }

    public Task<Balance> GetBalance()
    {
        return Task.FromResult(new Balance
        {
            Month = _monthValue,
            Total = _lastValue
        });
    }
}

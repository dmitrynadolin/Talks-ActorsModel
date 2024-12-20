using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Talks;
using Talks.Data;

namespace Talks.Services;

public class DirectMetersService : DirectMeters.DirectMetersBase
{
    private readonly MetersDbContext _dbContext;

    public DirectMetersService(MetersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<AddMeasurementReply> AddMeasurement(AddMeasurementRequest request, ServerCallContext context)
    {
        var lastValue = await _dbContext.Measurements.Where(m => m.MeterId == request.MeterId)
            .OrderByDescending(m => m.Time)
            .Select(m => m.Value)
            .FirstOrDefaultAsync(context.CancellationToken);

        var delta = request.Value - lastValue;

        _dbContext.Measurements.Add(new Measurement
        {
            MeterId = request.MeterId,
            Delta = delta,
            Value = request.Value,
            Time = DateTime.UtcNow,
        });

        await _dbContext.SaveChangesAsync(context.CancellationToken);

        return new AddMeasurementReply
        {
            Delta = delta
        };
    }

    public override async Task<GetBalanceReply> GetBalance(GetBalanceRequest request, ServerCallContext context)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);


        var month = await _dbContext.Measurements.Where(m => m.MeterId == request.MeterId)
            .Where(m => m.Time > monthStart)
            .SumAsync(m => m.Delta, context.CancellationToken);

        var total = await _dbContext.Measurements.Where(m => m.MeterId == request.MeterId)
            .OrderByDescending(m => m.Time)
            .Select(m => m.Value).FirstOrDefaultAsync(context.CancellationToken);

        return new GetBalanceReply
        {
            Balance1M = month,
            BalanceTotal = total
        };
    }

    public override async Task<GetSummaryReply> GetSummary(GetSummaryRequest request, ServerCallContext context)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var month = await _dbContext.Measurements
            .Where(m => m.Time > monthStart)
            .SumAsync(m => m.Delta, context.CancellationToken);

        var total = await _dbContext.Measurements
            .SumAsync(m => m.Delta, context.CancellationToken);

        return new GetSummaryReply
        {
            Balance1M = month,
            BalanceTotal = total
        };
    }
}
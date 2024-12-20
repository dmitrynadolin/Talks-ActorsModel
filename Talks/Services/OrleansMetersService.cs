using Grpc.Core;
using Talks.Contracts;

namespace Talks.Services;

public class OrleansMetersService : OrleansMeters.OrleansMetersBase
{
    private readonly IClusterClient _client;

    public OrleansMetersService(IClusterClient client)
    {
        _client = client;
    }

    public override async Task<AddMeasurementReply> AddMeasurement(AddMeasurementRequest request, ServerCallContext context)
    {
        var meter = _client.GetGrain<IMeter>(request.MeterId);

        var delta = await meter.AddMeasurement(request.Value);

        return new AddMeasurementReply
        {
            Delta = delta
        };
    }

    public override async Task<GetBalanceReply> GetBalance(GetBalanceRequest request, ServerCallContext context)
    {
        var balance = await _client.GetGrain<IMeter>(request.MeterId).GetBalance();

        return new GetBalanceReply
        {
            Balance1M = balance.Month,
            BalanceTotal = balance.Total
        };
    }

    public override async Task<GetSummaryReply> GetSummary(GetSummaryRequest request, ServerCallContext context)
    {
        var balance = await _client.GetGrain<IMetersSummary>(0).GetSummary();

        return new GetSummaryReply
        {
            Balance1M = balance.Month,
            BalanceTotal = balance.Total
        };
    }
}
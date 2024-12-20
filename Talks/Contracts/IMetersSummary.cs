namespace Talks.Contracts;

public interface IMetersSummary : IGrainWithIntegerKey
{
    Task<Balance> GetSummary();
    Task AddMeasurement(double value);
}
namespace Talks.Contracts;

public interface IMeter : IGrainWithIntegerKey
{
    Task<double> AddMeasurement(double value);
    Task<Balance> GetBalance();
}
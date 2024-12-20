namespace Talks.Contracts;

[GenerateSerializer]
public class Balance
{
    [Id(0)] public double Month { get; set; }
    [Id(1)] public double Total { get; set; }
}
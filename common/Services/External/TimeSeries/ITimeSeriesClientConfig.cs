namespace Mmm.Platform.IoT.Common.Services.External.TimeSeries
{
    public interface ITimeSeriesClientConfig
    {
        string TimeSeriesFqdn { get; }
        string TimeSeriesAuthority { get; }
        string TimeSeriesAudience { get; }
        string TimeSeriesExplorerUrl { get; }
        string TimeSertiesApiVersion { get; }
        string TimeSeriesTimeout { get; }
        string ActiveDirectoryTenant { get; }
        string ActiveDirectoryAppId { get; }
        string ActiveDirectoryAppSecret { get; }
    }
}
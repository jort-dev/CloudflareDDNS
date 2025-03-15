using System.Diagnostics;
using Cloudflare_DDNS.Dtos;

namespace Cloudflare_DDNS;

public class MainWorker : BackgroundService
{
    private readonly ILogger<MainWorker> _logger;
    private readonly MainService _mainService;
    private readonly IConfiguration _configuration;

    public MainWorker(ILogger<MainWorker> logger, MainService mainService, IConfiguration configuration)
    {
        _logger = logger;
        _mainService = mainService;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<DnsRecord> dnsRecords = await _mainService.RetrieveDnsRecords();
        if (dnsRecords.Count == 0)
        {
            _logger.LogError("No DNS records found");
            Environment.Exit(0);
            return;
        }

        IEnumerable<string> dnsRecordIds = _configuration.GetSection("DnsRecordIds").Get<IEnumerable<string>>();
        bool found = false;
        foreach (string dnsRecordId in dnsRecordIds)
        {
            if (!string.IsNullOrWhiteSpace(dnsRecordId))
            {
                found = true;
            }
        }

        if (!found)
        {
            _logger.LogInformation("No DNS record IDS specified yet. See available options below:");
            _logger.LogInformation("Found DNS records:");
            foreach (DnsRecord record in dnsRecords)
            {
                _logger.LogInformation(record.ToString());
            }
        }


        _mainService.ValidateDnsRecords(dnsRecords);

        string? ipAddress = "Uninitialized in memory";
        do
        {
            string? newIpAddress = await _mainService.GetPublicIpAddress();
            if (newIpAddress == null)
            {
                continue;
            }

            if (newIpAddress != ipAddress)
            {
                _logger.LogInformation("IP address changed from {Old} to {New}", ipAddress, newIpAddress);
                ipAddress = newIpAddress;
                await _mainService.SetIpAddress(dnsRecords, ipAddress);
            }
            else
            {
                _logger.LogInformation("Waiting for IP address to update");
            }


            if (bool.Parse(_configuration["ExitAfterExecute"]))
            {
                break;
            }

            await Task.Delay(int.Parse(_configuration["CheckFrequencyMs"]), stoppingToken);
        } while (!stoppingToken.IsCancellationRequested);

        _logger.LogInformation("Exiting");
        Environment.Exit(0);
    }
}
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using Cloudflare_DDNS.Dtos;

namespace Cloudflare_DDNS;

public class MainService
{
    private readonly ILogger<MainService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public MainService(ILogger<MainService> logger, IConfiguration configuration, HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<string?> GetPublicIpAddress()
    {
        try
        {
            string ipAddress = await _httpClient.GetStringAsync(_configuration["IpApi"]);
            _logger.LogInformation("IP address: {Ip}", ipAddress);
            return ipAddress;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving IP address: {Message}", e.Message);
        }

        return null;
    }

    public async Task<List<DnsRecord>> RetrieveDnsRecords()
    {
        try
        {
            List<DnsRecord> dnsRecords = [];
            string zoneId = _configuration["ZoneId"]!;
            string email = _configuration["ApiEmail"]!;
            string apiKey = _configuration["GlobalApiToken"]!;
            string url = _configuration["DnsRecordIdApiUrl"]!;
            url = string.Format(url, zoneId);
            _logger.LogDebug("Retrieving DNS records ZoneID={0}, Email={1}, ApiKey={2}, URL={3}", zoneId, email, apiKey,
                url);
            using HttpRequestMessage request = new(HttpMethod.Get, url);
            request.Headers.Add("X-Auth-Email", email);
            request.Headers.Add("X-Auth-Key", apiKey);
            using HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseJson);
            foreach (JsonElement item in doc.RootElement.GetProperty("result").EnumerateArray())
            {
                try
                {
                    DnsRecord record = JsonSerializer.Deserialize<DnsRecord>(item.GetRawText())!;
                    dnsRecords.Add(record);
                }
                catch (JsonException jsonException)
                {
                    _logger.LogError(jsonException, "Error reading DNS record from JSON: {0}", item.GetRawText());
                }
            }

            if (dnsRecords.Count == 0)
            {
                _logger.LogError("No DNS records found in response: {0}", responseJson);
            }

            return dnsRecords;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving DNS records: {Message}", e.Message);
        }

        return [];
    }

    public async Task<string> GetDnsRecordId()
    {
        string zoneId = _configuration["ZoneId"]!;
        string email = _configuration["ApiEmail"]!;
        string apiKey = _configuration["GlobalApiToken"]!;
        string url = _configuration["DnsRecordIdApiUrl"]!;
        url = string.Format(url, zoneId);
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Add("X-Auth-Email", email);
        request.Headers.Add("X-Auth-Key", apiKey);
        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        string responseJson = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(responseJson);
        string? result = null;
        foreach (JsonElement item in doc.RootElement.GetProperty("result").EnumerateArray())
        {
            _logger.LogInformation(item.ToString());
            result = item.GetProperty("id").GetString();
        }

        if (result == null)
        {
            _logger.LogInformation(responseJson);
            _logger.LogError("Could not find a DNS record ID in above response");
        }

        _logger.LogInformation("Found DNS record ID: {Id}", result);

        return result;
    }

    public void ValidateDnsRecords(List<DnsRecord> dnsRecords)
    {
        IEnumerable<string> dnsRecordIds = _configuration.GetSection("DnsRecordIds").Get<IEnumerable<string>>();

        foreach (string id in dnsRecordIds)
        {
            bool found = false;
            foreach (DnsRecord record in dnsRecords)
            {
                if (id == record.DnsRecordId)
                {
                    found = true;
                }
            }

            if (!found)
            {
                _logger.LogWarning("DNS record {Id} not found in Cloudflare", id);
            }
        }

        List<DnsRecord> recordsToIgnore = [];
        foreach (DnsRecord record in dnsRecords)
        {
            bool found = false;
            foreach (string id in dnsRecordIds)
            {
                if (id == record.DnsRecordId)
                {
                    found = true;
                }
            }

            if (!found)
            {
                recordsToIgnore.Add(record);
            }
        }

        _logger.LogInformation("Ignoring {Count} record(s) not specified in the config", recordsToIgnore.Count);
        dnsRecords.RemoveAll(record => recordsToIgnore.Contains(record));
    }

    public async Task SetIpAddress(List<DnsRecord> dnsRecords, string ipAddress)
    {
        string zoneId = _configuration["ZoneId"]!;
        string email = _configuration["ApiEmail"]!;
        string apiKey = _configuration["GlobalApiToken"]!;

        foreach (DnsRecord record in dnsRecords)
        {
            _logger.LogInformation("Updating IP address of record {Name} (type={Type}, TTL={Ttl})", record.Name, record.Type, record.Ttl);
            try
            {
                string url = _configuration["DnsUpdateDnsRecordApiUrl"];

                url = string.Format(url, zoneId, record.DnsRecordId);

                var requestData = new
                {
                    content = ipAddress,
                    name = record.Name,
                    type = record.Type,
                    ttl = record.Ttl
                };

                string json = JsonSerializer.Serialize(requestData,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                StringContent requestContent = new(json, Encoding.UTF8, "application/json");

                using HttpRequestMessage requestMessage = new(HttpMethod.Put, url);
                requestMessage.Headers.Add("X-Auth-Email", email);
                requestMessage.Headers.Add("X-Auth-Key", apiKey);
                requestMessage.Content = requestContent;

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                string result = await response.Content.ReadAsStringAsync();
                if (!result.Contains("\"success\":true")) //yeah i could also parse the json and deal with more exceptions..
                {
                    throw new Exception($"Response was not success but {result}");
                }

                _logger.LogInformation(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting IP address for DNS record {Record}", record.ToString());
                continue;
            }

            _logger.LogInformation("Successfully updated IP address of record {Name} (type={Type}, TTL={Ttl})", record.Name, record.Type, record.Ttl);
        }
    }
}
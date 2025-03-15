namespace Cloudflare_DDNS.Dtos;

using System;
using System.Text.Json.Serialization;

public class DnsRecord
{
    [JsonPropertyName("id")] public required string DnsRecordId { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("type")] public required string Type { get; set; }

    [JsonPropertyName("content")] public required string Content { get; set; }

    [JsonPropertyName("proxiable")] public required bool Proxiable { get; set; }

    [JsonPropertyName("proxied")] public required bool Proxied { get; set; }

    [JsonPropertyName("ttl")] public required int Ttl { get; set; }

    [JsonPropertyName("settings")] public required object Settings { get; set; }

    [JsonPropertyName("meta")] public required object Meta { get; set; }

    [JsonPropertyName("comment")] public string? Comment { get; set; }

    [JsonPropertyName("tags")] public required string[] Tags { get; set; }

    [JsonPropertyName("created_on")] public required DateTime CreatedOn { get; set; }

    [JsonPropertyName("modified_on")] public required DateTime ModifiedOn { get; set; }

    public override string ToString()
    {
        return
            $"{nameof(DnsRecordId)}: {DnsRecordId}, {nameof(Name)}: {Name}, {nameof(Type)}: {Type}, {nameof(Content)}: {Content}, {nameof(Proxiable)}: {Proxiable}, {nameof(Proxied)}: {Proxied}, {nameof(Ttl)}: {Ttl}, {nameof(Settings)}: {Settings}, {nameof(Meta)}: {Meta}, {nameof(Comment)}: {Comment}, {nameof(Tags)}: {Tags}, {nameof(CreatedOn)}: {CreatedOn}, {nameof(ModifiedOn)}: {ModifiedOn}";
    }
}
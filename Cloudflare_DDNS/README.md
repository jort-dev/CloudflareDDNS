# Cloudflare DDNS

In `appsettings.json`, update the following values:  
```
DnsRecordIds, ApiEmail, GlobalApiToken, ZoneId, AccountId  
```
If you don't know the values for `DnsRecordIds`:
* In Cloudflare DNS management, go to the record, and inspect element on the `Edit >` button. The ID is shown like this: aria-controls="XXXXXXX-dns-edit-row"
* Or run this program with the field empty. It will print all your DNS record IDs.

### Packages
* Microsoft.Extensions.Http
<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (23 Aug 2026)</h1>
</div>

Here is a summary of what I did today on the public website, the hosted admin site, and the API. This is a hosting and wiring day, not a new clinical feature day.

## 1. Public website moved to the new Forge server

[raphcare.com](https://raphcare.com) now runs on the new Forge server (`server3.yindula.com`), not the old DigitalOcean host.

What I did:

- Switched Afrihost nameservers so Afrihost controls DNS (it was still on DigitalOcean nameservers).
- Pointed the website A records (`raphcare.com`, `www`, and the wildcard) at the Forge IP.
- Left mail and cPanel records on Afrihost, so email stays where it is.
- Created the site on Forge, issued HTTPS (Let's Encrypt), and deployed the marketing site.

`https://www.raphcare.com` redirects to `https://raphcare.com`. The live page is the RaphCare marketing site, not the old WordPress maintenance page that was sitting on the previous server.

## 2. The admin site was talking to the wrong place

On the live admin website, clinic lists and email verification failed because the site was still trying to reach a copy of the API that only exists on a developer PC.

The hosted admin site is now set to call the hosted API:

[https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net](https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net)

The admin site itself is:

[https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net)

Email settings on the API were already in place. They were not the reason verification failed. The website never reached the API, so no mail was sent.

## 3. The API could not stay running on Azure

Once the admin site pointed at the real API, I could see why the API itself was falling over.

- It was still trying to use a laptop-only database. That does not exist on Azure.
- The connection name on Azure was then corrected so the API uses Azure SQL (`raphcare-database` on `raphcare-server`).
- Azure SQL was set to block all public access. The API cannot use a private-only network on the free web plan, so public access was switched to **selected networks**, with **Allow Azure services** turned on.

After that, the API reached the database. The LocalDB / public-access errors are resolved.

## 4. Free hosting quota (this is why the API will not start tonight)

**raphcare-api** and the admin site share the same free App Service plan (F1). That plan allows about 60 minutes of CPU per day.

Repeated crash-and-restart today used that quota up. Azure then shows **Start succeeded**, but the status stays **Quota exceeded** and the site does not actually run.

This is a plan limit, not another configuration miss. The daily free quota usually resets around midnight UTC. After that, start **raphcare-api** once.

The public website on Forge is separate. It is not on that Azure free quota.

## Still to confirm

1. Open [https://raphcare.com](https://raphcare.com) and confirm the marketing site looks right (and that mail still works).
2. Once the API is running: open the admin site and confirm the clinic list loads.
3. Send a verification email and confirm it arrives from the RaphCare mailbox.
4. Confirm Azure SQL stays on **selected networks** with **Allow Azure services** left on.

The full product status board is still the partner checklist (overall about 74%). Today did not add new patient-app screens. It moved the public website to Forge, and unblocked the hosted admin site from talking to the hosted API.

Thanks,  
Daskana

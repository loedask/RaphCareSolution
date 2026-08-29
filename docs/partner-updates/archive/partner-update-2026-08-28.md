<div class="doc-header">
<img src="brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (28 Aug 2026)</h1>
</div>

Here is a summary of what I did today. Two things: paid hosting so the admin site and API can stay up, and a layout change in the clinic portal when you open a hospital. This is not a new patient-app feature.

## 1. Hosting: off the free plan (pricing)

The 23 Aug note explained why the API would not stay running: **raphcare-api** and the admin site share one Azure App Service plan. That plan was **Free (F1)**. Free allows about 60 minutes of CPU per day. Testing used that up, so Azure said Start succeeded while the status stayed **Quota exceeded**.

I moved that shared plan to **Basic B1 Linux**. Always On is now on, so the apps should not sleep.

What that changes:

- The 60-minute daily cap is gone. The API and admin site can stay up while we test.
- Both apps are still on the **same** plan. One change covers both.
- List price in **South Africa North** is about **$0.0243 per hour**, so roughly **$18 per month** if it runs all month.
- The **$15** figure people quote is the usual US-region B1 price (about **$13** list, often rounded to $15). Same B1 size. South Africa North is a bit higher. Azure Cost analysis will show the exact bill.
- We can scale back to Free later if we want to stop the monthly charge.

**What we owe so far (Azure, billed in USD):** about **$7** in accrued usage from 8 to 27 Aug. No invoice has been issued yet (this account invoices around the 9th of the month).

That $7 is almost all the SQL database, not the websites:

- About **$4** SQL storage (around 1 GB).
- About **$3** SQL serverless compute (only a few hours of actual use).
- **$0** for the App Service plan while it was Free.
- **$0** for outbound bandwidth so far.

Last night's move to B1 will not show on that $7 yet. Azure cost figures lag by about a day. From here, the web plan adds about **$18 per month** on top of SQL. SQL will keep varying with how much the database is used.

The public website on Forge ([raphcare.com](https://raphcare.com)) is separate. It is not on this Azure plan.

Admin site: [https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net)

API: [https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net](https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net)

## 2. Opening a hospital hides the left menu

The left menu is for platform work: dashboard, the list of hospitals, and register hospital.

Once you open a specific hospital, that menu was taking space and competing with the hospital tabs (Overview, Facilities, Patients, Providers, Appointments, Devices, Inpatient, Staff). Appointments even appeared in both places.

The left menu now hides when you are inside a hospital. The hospital tabs are the navigation that matters. Those screens get more room.

## 3. How staff get back

The top bar now carries the way out, plus the account controls that used to sit in the left menu.

- The RaphCare mark goes to the dashboard.
- **All hospitals** goes back to the hospital list. Register hospital is still on that list.
- The current hospital name stays visible.
- Language and **Sign out** sit in the top bar on these pages.

If someone looks after more than one hospital, the hospital switcher in the top bar still works.

## 4. Partner checklist

I updated the partner status board so the hospital layout is easy to see:

- New row: **Opening a hospital** (Done).
- First item under **What to try this week** is this check.
- Overall completion is now **75%**. Hospital admin stays at **100%**.

## Still to confirm

1. Open the admin site and confirm the clinic list loads (the sites should stay up now, not die after an hour).
2. Open a hospital in admin and confirm the left menu is gone.
3. Use **All hospitals** in the top bar to return to the list.
4. Confirm language and sign out still work from the top bar.
5. If you have more than one hospital, switch hospitals and confirm you land in the new one.

The full product status board is still the partner checklist. Today did not add new patient-app screens.

Thanks,  
Daskana

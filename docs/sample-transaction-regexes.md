# Sample Transaction Regex Patterns

NocoDB rows to insert into `transaction_regex_patterns`. Table columns: `BillType`, `Pattern`, `Fields`.

---

## Amex

**Sample text:**
```
Alert: You've spent INR 299.00 on your AMEX card ** 81009 at PAYU SWIGGY on 5 May 2026 at 12:36 AM IST. Call 18004190691 if this was not made by you.
```

**Pattern:**
```
Alert: You've spent (?P<currency>[A-Z]+) (?P<amount>[\d,]+\.\d+) on your AMEX card \*\* \d+ at (?P<merchant>.+?) on \d{1,2} \w+ \d{4} at \d{1,2}:\d{2} [AP]M \w+\.
```

**Fields:**
```json
[
  {"name": "currency", "type": "string", "format": null},
  {"name": "amount",   "type": "string", "format": "currency:INR"},
  {"name": "merchant", "type": "string", "format": null}
]
```

> Date not captured — timezone is an abbreviation (`IST`) which jiff cannot parse. `BackDate` falls back to `back_date_string`.

> The `currency` capture group handles any currency (INR, USD, GBP, etc.) — one pattern covers all Amex transactions regardless of the currency spent.

---

## SBI-Prime

**Sample text:**
```
Rs.550.00 spent on your SBI Credit Card ending 3172 at GOOGLEPLAY on 05/05/26. Trxn. not done by you? Report at https://sbicard.com/Dispute
```

**Pattern:**
```
Rs\.(?P<amount>[\d,]+\.\d+) spent on your SBI Credit Card ending \d+ at (?P<merchant>\w+) on \d{2}/\d{2}/\d{2}\.
```

**Fields:**
```json
[
  {"name": "amount",   "type": "string", "format": "currency:INR"},
  {"name": "merchant", "type": "string", "format": null}
]
```

> Date not captured — date-only string (`05/05/26`) has no time component, so `jiff::Timestamp::strptime` would fail. `BackDate` falls back to `back_date_string`.

> Currency not captured — `Rs.` prefix does not map to an ISO code. `Currency` field will not be set; currency normalisation is a no-op for INR.

---

## SavingsAccount

**Sample text:**
```
Dear UPI user A/C X6902 debited by 640.00 on date 14Mar26 trf to P N NARASIMHAMUR Refno 874010630736 If not u? call-1800111109 for other services-18001234-SBI
```

**Pattern:**
```
Dear UPI user A/C X\d+ debited by (?P<amount>[\d,]+\.\d+) on date \w+ trf to (?P<merchant>[\w ]+?) Refno
```

**Fields:**
```json
[
  {"name": "amount",   "type": "string", "format": "currency:INR"},
  {"name": "merchant", "type": "string", "format": null}
]
```

> Currency not captured — always INR for this bill type. Same no-op note as SBI-Prime.

---

## Jupiter

**Sample text:**
```
₹1670.00 paid from your Edge CSB Bank RuPay Credit Card to AVENUE SUPERMARTS LTD BANGALORE KAIN on 2026-05-05T11:40:22.814201+05:30 IST. To raise an issue, call 8655055086. Thank you for using Jupiter.
```

**Pattern:**
```
₹(?P<amount>[\d,]+\.\d+) paid from your .+ to (?P<merchant>.+?) on (?P<date>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+[+-]\d{2}:\d{2}) \w+\.
```

**Fields:**
```json
[
  {"name": "amount",   "type": "string", "format": "currency:INR"},
  {"name": "merchant", "type": "string", "format": null},
  {"name": "date",     "type": "date",   "format": "%Y-%m-%dT%H:%M:%S%.f%z"}
]
```

> Date parsing works here — the ISO 8601 timestamp includes a numeric offset (`+05:30`) which jiff can parse directly.

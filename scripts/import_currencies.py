import json
import os
import requests

NOCODB_URL = "http://localhost:8081"
API_KEY = os.environ["NOCODB_API_KEY"]

SYMBOLS = {
    "AED": "د.إ", "AFN": "؋", "ALL": "L", "AMD": "֏", "ANG": "ƒ",
    "AOA": "Kz", "ARS": "$", "AUD": "$", "AWG": "ƒ", "AZN": "₼",
    "BAM": "KM", "BBD": "$", "BDT": "৳", "BGN": "лв", "BHD": ".د.ب",
    "BIF": "Fr", "BMD": "$", "BND": "$", "BOB": "Bs.", "BRL": "R$",
    "BSD": "$", "BTN": "Nu", "BWP": "P", "BYN": "Br", "BZD": "$",
    "CAD": "$", "CDF": "Fr", "CHF": "Fr", "CLP": "$", "CNY": "¥",
    "COP": "$", "CRC": "₡", "CUP": "$", "CVE": "$", "CZK": "Kč",
    "DJF": "Fr", "DKK": "kr", "DOP": "$", "DZD": "د.ج", "EGP": "£",
    "ERN": "Nfk", "ETB": "Br", "EUR": "€", "FJD": "$", "FKP": "£",
    "GBP": "£", "GEL": "₾", "GHS": "₵", "GIP": "£", "GMD": "D",
    "GNF": "Fr", "GTQ": "Q", "GYD": "$", "HKD": "$", "HNL": "L",
    "HRK": "kn", "HTG": "G", "HUF": "Ft", "IDR": "Rp", "ILS": "₪",
    "INR": "₹", "IQD": "ع.د", "IRR": "﷼", "ISK": "kr", "JMD": "$",
    "JOD": "د.ا", "JPY": "¥", "KES": "Ksh", "KGS": "с", "KHR": "៛",
    "KMF": "Fr", "KPW": "₩", "KRW": "₩", "KWD": "د.ك", "KYD": "$",
    "KZT": "₸", "LAK": "₭", "LBP": "ل.ل", "LKR": "Rs", "LRD": "$",
    "LSL": "L", "LYD": "ل.د", "MAD": "د.م.", "MDL": "L", "MGA": "Ar",
    "MKD": "ден", "MMK": "K", "MNT": "₮", "MOP": "P", "MRU": "UM",
    "MUR": "Rs", "MVR": "Rf", "MWK": "MK", "MXN": "$", "MYR": "RM",
    "MZN": "MT", "NAD": "$", "NGN": "₦", "NIO": "C$", "NOK": "kr",
    "NPR": "Rs", "NZD": "$", "OMR": "ر.ع.", "PAB": "B/.", "PEN": "S/.",
    "PGK": "K", "PHP": "₱", "PKR": "Rs", "PLN": "zł", "PYG": "₲",
    "QAR": "ر.ق", "RON": "lei", "RSD": "din", "RUB": "₽", "RWF": "Fr",
    "SAR": "ر.س", "SBD": "$", "SCR": "Rs", "SDG": "£", "SEK": "kr",
    "SGD": "$", "SHP": "£", "SLL": "Le", "SOS": "Sh", "SRD": "$",
    "STN": "Db", "SVC": "₡", "SYP": "£", "SZL": "L", "THB": "฿",
    "TJS": "SM", "TMT": "T", "TND": "د.ت", "TOP": "T$", "TRY": "₺",
    "TTD": "$", "TWD": "$", "TZS": "Sh", "UAH": "₴", "UGX": "Sh",
    "USD": "$", "UYU": "$", "UZS": "so'm", "VES": "Bs.S", "VND": "₫",
    "VUV": "Vt", "WST": "T", "XAF": "Fr", "XCD": "$", "XOF": "Fr",
    "XPF": "Fr", "YER": "﷼", "ZAR": "R", "ZMW": "ZK", "ZWL": "$",
}

BASE_ID = os.environ["NOCODB_BASE_ID"]
TABLE_ID = os.environ["NOCODB_TABLE_ID"]

h = {"xc-token": API_KEY, "Content-Type": "application/json"}

with open("currencies.json") as f:
    currencies = json.load(f)

# Fetch all existing codes in one request
existing_resp = requests.get(
    f"{NOCODB_URL}/api/v1/db/data/noco/{BASE_ID}/{TABLE_ID}",
    headers=h,
    params={"fields": "Code", "limit": 10000},
)
existing_resp.raise_for_status()
existing_codes = {row["Code"] for row in existing_resp.json()["list"]}
print(f"Found {len(existing_codes)} existing records.")

inserted = skipped = 0
for code, name in currencies.items():
    if code in existing_codes:
        skipped += 1
        continue
    record = {"Code": code, "Name": name, "Symbol": SYMBOLS.get(code, "")}
    resp = requests.post(
        f"{NOCODB_URL}/api/v1/db/data/noco/{BASE_ID}/{TABLE_ID}",
        headers=h,
        json=record,
    )
    resp.raise_for_status()
    print(f"Inserted: {code}")
    inserted += 1

print(f"\nDone. Inserted: {inserted}, Skipped: {skipped}")

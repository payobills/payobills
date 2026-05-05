use chrono::DateTime;
use clap::Parser;
use regex::Regex;
use reqwest::header::HeaderMap;
use serde::{Serialize as SerializeImpl, Serializer};
use serde_derive::{Deserialize, Serialize};
use std::collections::HashMap;
use std::error::Error;

// Kept until patterns are pre-populated in NocoDB (Step 6) and hardcoded block is removed (Step 7)
#[allow(dead_code)]
const BILL_TYPE_AMEX: &str = "Amex";
#[allow(dead_code)]
const BILL_TYPE_SAVINGS_ACCOUNT: &str = "SavingsAccount";
#[allow(dead_code)]
const BILL_TYPE_TESTING: &str = "Testing";
#[allow(dead_code)]
const BILL_TYPE_JUPITER: &str = "Jupiter";
#[allow(dead_code)]
const BILL_TYPE_SBI_PRIME: &str = "SBI-Prime";

// References
// https://docs.rs/jiff/latest/jiff/#parsing-an-rfc-2822-datetime-string
// https://docs.rs/strptime/latest/strptime/struct.Parser.html
#[allow(dead_code)]
const TRANSACTION_DATE_FORMAT_AMEX: &str = "%d %B %Y at %I:%M %p %z";
#[allow(dead_code)]
const TRANSACTION_DATE_FORMAT_JUPITER: &str = "%-m/%-d/%y, %I:%M %p %z";
#[allow(dead_code)]
const TRANSACTION_DATE_FORMAT_SBI_PRIME: &str = "%d/%m/%y %H:%M %z";

#[derive(Serialize, Deserialize, Clone)]
struct CurrencyRecord {
    #[serde(rename = "Symbol")]
    symbol: String,
    #[serde(rename = "Name")]
    name: String,
    #[serde(rename = "Code")]
    code: String,
    #[serde(rename = "Aliases")]
    aliases: Vec<String>
}

#[derive(Clone)]
pub struct SLMParserEnv {
    pub base_url: String,
}

#[derive(Clone)]
pub struct NocoDBEnv {
    pub base_url: String,
    pub api_key: String,
    pub base_name_currencies: String,
    pub base_name_payobills: String,
    pub table_name_currencies_historical: String,
    pub table_name_currencies_currencies: String,
    pub table_name_payobills_transactions: String,
    pub table_name_regex_patterns: String,
}

#[derive(Serialize, Deserialize)]
struct PageInfo {
    #[serde(rename = "totalRows")]
    total_rows: i32,
    page: i32,
    #[serde(rename = "pageSize")]
    page_size: i32,
    #[serde(rename = "isFirstPage")]
    is_first_page: bool,
    #[serde(rename = "isLastPage")]
    is_last_page: bool,
}

#[derive(Serialize, Deserialize)]
struct NocoDBResponse<T> {
    list: Vec<T>,
    #[serde(rename = "pageInfo")]
    page_info: PageInfo,
}

// Command-line argument parsing using Clap
#[derive(Parser)]
struct Cli {
    #[arg(long)]
    base_name: String,
    #[arg(long)]
    table_name: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]

struct Bill {
    #[serde(rename = "Id")]
    id: u32,
    #[serde(rename = "Name")]
    name: String,
}

// Struct to represent a transaction record, with appropriate serde attributes
#[derive(Clone, Debug, Serialize, Deserialize)]
struct Transaction {
    #[serde(rename = "TransactionText")]
    transaction_text: Option<String>,
    #[serde(rename = "SourceSystemID")]
    source_system_id: Option<String>,
    #[serde(rename = "Currency")]
    currency: Option<String>,
    #[serde(rename = "Id")]
    id: i32,
    // #[serde(rename = "CreatedAt")]
    // created_at: Option<String>,
    #[serde(rename = "PaidAt")]
    paid_at: Option<String>,
    bills: Option<Bill>,
    #[serde(rename = "BackDateString", skip_serializing_if = "Option::is_none")]
    back_date_string: Option<String>,
    // #[serde(rename = "UpdatedAt")]
    // updated_at: Option<String>,
    #[serde(rename = "Amount")]
    amount: Option<f64>,
    #[serde(rename = "NormalizedAmount")]
    normalized_amount: Option<f64>,
    #[serde(rename = "nc_14ri__bills_id")]
    bills_id: Option<i32>,
    notes: Option<String>,
    #[serde(rename = "ParseStatus")]
    parse_status: Option<String>,
    #[serde(rename = "BillType")]
    bill_type: Option<String>,
}

enum Value {
    // Int(i32),
    F64(f64),
    Str(String),
}

impl SerializeImpl for Value {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: Serializer,
    {
        match *self {
            // Value::Int(ref v) => serializer.serialize_i32(*v),
            Value::F64(ref v) => serializer.serialize_f64(*v),
            Value::Str(ref v) => serializer.serialize_str(v),
        }
    }
}

fn apply_captures(
    caps: &regex::Captures,
    fields: &[crate::payobills::slm_client::FieldDescriptor],
    changes: &mut HashMap<String, Value>,
) {
    for field in fields {
        let Some(m) = caps.name(&field.name) else { continue };
        let raw = m.as_str().trim();
        match field.field_type.as_str() {
            "date" => {
                if let Some(ref fmt) = field.format {
                    match parse_custom_date(raw, fmt, true) {
                        Ok(date_str) => { changes.insert("BackDate".to_string(), Value::Str(date_str)); }
                        Err(e) => { eprintln!("Unable to parse date from transaction text - {}", e); }
                    }
                }
            }
            _ => match field.name.as_str() {
                "amount" => {
                    let cleaned = raw.replace(',', "");
                    if let Ok(val) = cleaned.parse::<f64>() {
                        changes.insert("Amount".to_string(), Value::F64(val));
                    }
                }
                "merchant" => { changes.insert("Merchant".to_string(), Value::Str(raw.to_string())); }
                "currency" => { changes.insert("Currency".to_string(), Value::Str(raw.to_string())); }
                _ => {}
            },
        }
    }
}

async fn parse_transaction(
    record: Transaction,
    nocodb_env: NocoDBEnv,
    slm_env: Option<&SLMParserEnv>,
) -> Result<(), Box<dyn std::error::Error + Send + Sync>> {
    // Bag to keep all changes to be made to the transaction during parsing
    let mut changes: HashMap<String, Value> = HashMap::new();

    // Try to parse record back_date_string (which is added from the Page title on Notion by default - time when the transaction was added to notion)
    // This is near real time time so it's a good fallback
    let format = "%a, %d %b %Y %H:%M:%S %z";

    // println!("Trying to parse {:?}", record.back_date_string.clone());
    if record.back_date_string != None {
        match parse_custom_date(
            record.back_date_string.clone().unwrap().as_str(),
            format,
            false,
        ) {
            Ok(date_string) => {
                // println!("Parsed date: {:?}", date_string);
                changes.insert("BackDate".to_string(), Value::Str(date_string));
            }
            Err(_) => {
                // If this parsing also failed, there is nothing we can do, NocoDB will pick
                // Transaction Creation Date as the payment date
            }
        }
    }

    match record.bill_type {
        Some(ref bill_type) => {
            match slm_env {
                Some(slm) => {
                    let transaction_text = record.transaction_text.clone().unwrap_or_default();
                    let stored_patterns = crate::payobills::regex_store::get_patterns(&nocodb_env, bill_type).await?;
                    let matched = stored_patterns.iter().find_map(|stored| {
                        Regex::new(&stored.pattern).ok().and_then(|re| {
                            re.captures(&transaction_text).map(|caps| (caps, stored))
                        })
                    });
                    match matched {
                        Some((caps, stored)) => {
                            apply_captures(&caps, &stored.fields, &mut changes);
                            changes.insert(
                                "ParseStatus".to_string(),
                                Value::Str("ParsedV1".to_string()),
                            );
                        }
                        None if !stored_patterns.is_empty() => {
                            changes.insert(
                                "ParseStatus".to_string(),
                                Value::Str("FailedV1".to_string()),
                            );
                        }
                        None => {
                            match crate::payobills::slm_client::generate_regex(
                                &slm.base_url,
                                bill_type,
                                &transaction_text,
                            )
                            .await
                            {
                                Ok(generated) => {
                                    match Regex::new(&generated.regex) {
                                        Ok(re) if re.is_match(&transaction_text) => {
                                            let _ = crate::payobills::regex_store::save_pattern(
                                                &nocodb_env,
                                                bill_type,
                                                &generated.regex,
                                                &generated.fields,
                                            )
                                            .await;
                                            let caps = re.captures(&transaction_text).unwrap();
                                            apply_captures(&caps, &generated.fields, &mut changes);
                                            changes.insert(
                                                "ParseStatus".to_string(),
                                                Value::Str("ParsedBySLM".to_string()),
                                            );
                                        }
                                        _ => {
                                            changes.insert(
                                                "ParseStatus".to_string(),
                                                Value::Str("FailedNoPattern".to_string()),
                                            );
                                        }
                                    }
                                }
                                Err(e) => {
                                    eprintln!("SLM unreachable for {}: {}", bill_type, e);
                                    changes.insert(
                                        "ParseStatus".to_string(),
                                        Value::Str("NotStarted".to_string()),
                                    );
                                }
                            }
                        }
                    }
                }
                None => {
                    // SLM not configured — fall back to hardcoded regex per BillType
                    if bill_type == BILL_TYPE_AMEX {
                        let re = Regex::new(
                    r"^Alert: You've spent (?P<currency>[^\d\s]+)\s{0,1}(?P<amount>[\d,]+\.?\d+) on your AMEX card \*\* (?P<card>\d+)( at ){0,1}(?P<merchant>.*) on (?P<date>[\d]{1,2} \w+,? [\d]{4} at \d{2,2}:\d{2,2} [A-Z]{2,2}) (?P<timezone>[A-Z]{2,3})\..*$",
                )
                .unwrap();
                        if let Some(caps) = re.captures(&record.transaction_text.clone().unwrap_or_default()) {
                            let amount = caps.name("amount").expect("CAPTURE TO BE PRESENT").as_str().trim().to_string().replace(',', "");
                            let date_string_capture = caps.name("date").expect("CAPTURE TO BE PRESENT").as_str().replace(',', "").trim().to_string();
                            let time_zone_capture = caps.name("timezone").expect("CAPTURE TO BE PRESENT").as_str();
                            let full_back_date_capture = format!("{} {}", date_string_capture, timezone_to_offset(time_zone_capture));
                            match parse_custom_date(full_back_date_capture.as_str(), TRANSACTION_DATE_FORMAT_AMEX, false) {
                                Ok(date_string) => { changes.insert("BackDate".to_string(), Value::Str(date_string)); }
                                Err(e) => { eprintln!("Unable to parse date from transaction text - {}", e); }
                            }
                            changes.insert("Merchant".to_string(), Value::Str(caps.name("merchant").expect("CAPTURE").as_str().trim().to_string()));
                            changes.insert("Currency".to_string(), Value::Str(caps.name("currency").expect("CAPTURE").as_str().trim().to_string()));
                            changes.insert("Amount".to_string(), Value::F64(amount.parse::<f64>().unwrap()));
                            changes.insert("ParseStatus".to_string(), Value::Str("ParsedV1".to_string()));
                        } else {
                            changes.insert("ParseStatus".to_string(), Value::Str("FailedV1".to_string()));
                        }
                    } else if bill_type == BILL_TYPE_JUPITER {
                        let re = Regex::new(r"Rs (\d+\.\d+) debited .* on ([^-]*) -").unwrap();
                        if let Some(caps) = re.captures(&record.transaction_text.clone().unwrap_or_default()) {
                            changes.insert("Amount".to_string(), Value::F64(caps.get(1).unwrap().as_str().trim().parse::<f64>().unwrap()));
                            let full_back_date_capture = format!("{} +0530", caps.get(2).unwrap().as_str().trim());
                            match parse_custom_date(&full_back_date_capture, TRANSACTION_DATE_FORMAT_JUPITER, true) {
                                Ok(date_string) => { changes.insert("BackDate".to_string(), Value::Str(date_string)); }
                                Err(e) => { eprintln!("Unable to parse date from transaction text - {}", e); }
                            }
                            changes.insert("ParseStatus".to_string(), Value::Str("ParsedV1".to_string()));
                        } else {
                            changes.insert("ParseStatus".to_string(), Value::Str("FailedV1".to_string()));
                        }
                    } else if bill_type == BILL_TYPE_SBI_PRIME {
                        let re = Regex::new(r"([\w\.]*)(\d+\,?\d+.\d+) spent .* at ([a-zA-Z\*]*) on ([\d\/]*)\.",).unwrap();
                        if let Some(caps) = re.captures(&record.transaction_text.clone().unwrap_or_default()) {
                            changes.insert("Currency".to_string(), Value::Str(caps.get(1).unwrap().as_str().trim().to_string()));
                            changes.insert("Amount".to_string(), Value::F64(caps.get(2).unwrap().as_str().trim().replace(',', "").parse::<f64>().unwrap()));
                            changes.insert("Merchant".to_string(), Value::Str(caps.get(3).unwrap().as_str().trim().to_string()));
                            let full_back_date_capture = format!("{} 00:00 +0530", caps.get(4).unwrap().as_str().trim());
                            match parse_custom_date(&full_back_date_capture, TRANSACTION_DATE_FORMAT_SBI_PRIME, true) {
                                Ok(date_string) => { changes.insert("BackDate".to_string(), Value::Str(date_string)); }
                                Err(e) => { eprintln!("Unable to parse date from transaction text - {}", e); }
                            }
                            changes.insert("ParseStatus".to_string(), Value::Str("ParsedV1".to_string()));
                        } else {
                            changes.insert("ParseStatus".to_string(), Value::Str("FailedV1".to_string()));
                        }
                    } else if bill_type == BILL_TYPE_SAVINGS_ACCOUNT {
                        let re = Regex::new(r"Dear UPI user A/C X\d{4} debited by (\d+\.\d+) on date .* trf to ([\s\w]*) Refno .*",).unwrap();
                        if let Some(caps) = re.captures(&record.transaction_text.clone().unwrap_or_default()) {
                            changes.insert("Merchant".to_string(), Value::Str(caps.get(2).unwrap().as_str().trim().to_string()));
                            changes.insert("Currency".to_string(), Value::Str("INR".to_string()));
                            changes.insert("Amount".to_string(), Value::F64(caps.get(1).unwrap().as_str().trim().parse::<f64>().unwrap()));
                            changes.insert("ParseStatus".to_string(), Value::Str("ParsedV1".to_string()));
                        } else {
                            changes.insert("ParseStatus".to_string(), Value::Str("FailedV1".to_string()));
                        }
                    } else if bill_type == BILL_TYPE_TESTING {
                        let re = Regex::new(r"(\w+): You've spent (\w+) (\d+\,?\d+.\d+) on your AMEX card .* at (.*)\s*on",).unwrap();
                        if let Some(caps) = re.captures(&record.transaction_text.clone().unwrap_or_default()) {
                            let amount = caps.get(3).unwrap().as_str().trim().replace(',', "");
                            changes.insert("Merchant".to_string(), Value::Str(caps.get(4).unwrap().as_str().trim().to_string()));
                            changes.insert("Currency".to_string(), Value::Str(caps.get(2).unwrap().as_str().trim().to_string()));
                            changes.insert("Amount".to_string(), Value::F64(amount.parse::<f64>().unwrap()));
                            changes.insert("ParseStatus".to_string(), Value::Str("ParsedV1".to_string()));
                        } else {
                            changes.insert("ParseStatus".to_string(), Value::Str("FailedV1".to_string()));
                        }
                    } else {
                        changes.insert("ParseStatus".to_string(), Value::Str("FailedV1".to_string()));
                    }
                }
            }
        }
        None => {}
    }

    match record.currency {
        Some(currency) => {
            if currency != "INR" {
                let paid_at =
                    DateTime::parse_from_rfc3339(record.paid_at.clone().unwrap().as_str()).unwrap();
                let transaction_date_string = paid_at.format("%Y-%m-%d").to_string();

                let app_id = std::env::var("EXTERNAL_CONVERSION_SERVICE__APP_ID")
                    .unwrap_or_default();

                match crate::payobills::currency_sync::get_conversion_rates(
                    nocodb_env.clone(),
                    app_id,
                    transaction_date_string,
                )
                .await
                {
                    Ok(exchange_rates) => {
                        match record.amount {
                            Some(amount) => {
                                let currencies: Vec<CurrencyRecord> = get_nocodb_records(
                                    nocodb_env.clone(),
                                    nocodb_env.clone().base_name_currencies,
                                    nocodb_env.clone().table_name_currencies_currencies,
                                    "(Code,neq,'')",
                                )
                                .await?
                                .list;

                                let matching_currency_records: Vec<&CurrencyRecord> = currencies
                                    .iter()
                                    .filter(|curr| currency == curr.code || curr.aliases.contains(&currency) || currency == curr.symbol)
                                    .collect();

                                if matching_currency_records.len() > 0 {
                                    let matching_currency_record = matching_currency_records[0];

                                    let exchange_rate_usd_to_source: f64 = exchange_rates
                                        .get(matching_currency_record.code.as_str())
                                        .unwrap()
                                        .clone();
                                    let exchange_rate_usd_to_inr: f64 =
                                        exchange_rates.get("INR").unwrap().clone();
                                    let normalized_amount =
                                        amount / exchange_rate_usd_to_source * exchange_rate_usd_to_inr;

                                    changes.insert(
                                        "Currency".to_string(),
                                        Value::Str(matching_currency_record.code.as_str().to_string()),
                                    );
                                    changes.insert(
                                        "NormalizedAmount".to_string(),
                                        Value::F64(normalized_amount),
                                    );
                                }
                            }
                            None => {}
                        }
                    }
                    Err(e) => {
                        eprintln!("Failed to get conversion rates: {}", e);
                        changes.insert(
                            "ParseStatus".to_string(),
                            Value::Str(String::from("ReParse")),
                        );
                    }
                }
            }
            // println!("Parsing currency exchange data on ReParse - Transaction ID{:?}", record.clone().id);
        }
        None => {}
    }

    let mut map = HeaderMap::new();
    map.insert("xc-token", nocodb_env.api_key.parse().unwrap());
    map.insert(
        "Content-Type",
        "application/json".to_string().as_str().parse().unwrap(),
    );

    let url: String = format!(
        "{}/api/v1/db/data/nc/{}/{}/{}",
        nocodb_env.base_url,
        nocodb_env.base_name_payobills,
        nocodb_env.table_name_payobills_transactions,
        record.id
    );

    let response = reqwest::Client::new()
        .patch(url)
        .body(serde_json::to_string(&changes)?)
        .headers(map)
        .send()
        .await?
        // .text()
        .json::<Transaction>()
        .await?;

    // println!("Transaction updated - {}", response);
    println!("Transaction updated - {}", response.id);
    Ok(())
}

async fn get_nocodb_records<T>(
    nocodb_env: NocoDBEnv,
    base_name: String,
    table_name: String,
    filter: &str,
) -> Result<NocoDBResponse<T>, Box<dyn Error + Send + Sync>>
where
    T: serde::de::DeserializeOwned,
{
    let mut map = HeaderMap::new();
    map.insert("xc-token", nocodb_env.api_key.as_str().parse().unwrap());

    let url: String = format!(
        "{}/api/v1/db/data/nc/{}/{}?w={}&l=1000&fields=*",
        nocodb_env.base_url, base_name, table_name, filter
    );

    let response = reqwest::Client::new()
        .get(&url)
        .headers(map)
        .send()
        .await?
        .json::<NocoDBResponse<T>>()
        .await?;

    Ok(response)
}

pub async fn parse_transaction_by_id(nocodb_env: NocoDBEnv, transaction_id: String, slm_env: Option<SLMParserEnv>) -> Result<(), Box<dyn Error + Send + Sync>> {
    let mut map = HeaderMap::new();
    map.insert("xc-token", nocodb_env.api_key.parse().unwrap());

    let url: String = format!(
        "{}/api/v1/db/data/nc/{}/{}/{}",
        nocodb_env.base_url,
        nocodb_env.base_name_payobills,
        nocodb_env.table_name_payobills_transactions,
        transaction_id
    );

    let response = reqwest::Client::new()
        .get(url)
        .headers(map)
        .send()
        .await?
        .json::<Transaction>()
        .await?;

    parse_transaction(response, nocodb_env, slm_env.as_ref()).await
}

pub async fn process_transactions(nocodb_env: NocoDBEnv, slm_env: Option<SLMParserEnv>) -> Result<(), Box<dyn Error + Send + Sync>> {
    // let mut offset = 0;
    let mut parse_more = true;

    while parse_more == true {
        let mut map = HeaderMap::new();
        map.insert("xc-token", nocodb_env.api_key.parse().unwrap());

        let url: String = format!(
            "{}/api/v1/db/data/nc/{}/{}?w=(ParseStatus,eq,ReParse)~or(ParseStatus,eq,NotStarted)&l=1000&fields=*",
            nocodb_env.base_url,
            nocodb_env.base_name_payobills,
            nocodb_env.table_name_payobills_transactions
        );

        println!("{}", url.clone());

        // let response: NocoBDResponse
        let response = reqwest::Client::new()
            .get(url.clone())
            .headers(map)
            .send()
            .await?
            .json::<NocoDBResponse<Transaction>>()
            .await?;

        println!(
            "Retrieved transactions. Transaction Count - {}",
            response.list.len()
        );
        // println!("{}", serde_json::to_string(&response)?);
        // parse_more = !response.page_info.is_last_page;
        // println!("parse_more: {}", parse_more);
        // offset = offset + response.page_info.page_size;

        // Parse transactions only if it is attached to a bill
        for transaction in response.list {
            match transaction.bills {
                Some(_) => parse_transaction(transaction.clone(), nocodb_env.clone(), slm_env.as_ref()).await?,
                None => {}
            }
        }

        parse_more = false;
    }

    Ok(())
}

/// Parses a custom date string and converts it to an RFC 3339 format.
fn parse_custom_date(
    input_date: &str,
    date_format: &str,
    use_jiff: bool,
) -> Result<String, Box<dyn std::error::Error + Send + Sync>> {
    if use_jiff {
        let zdt = jiff::Timestamp::strptime(date_format, input_date)?;
        println!("{}", zdt);
        return Ok(zdt.to_string());
    }

    // Parse the date without timezone into NaiveDateTime
    match DateTime::parse_from_str(input_date, date_format) {
        Ok(date) => {
            // println!("Parsed date: {:?}", date);
            let parsed_date_time_string = date.clone().to_rfc3339();
            return Ok(parsed_date_time_string);
        }
        Err(_) => match DateTime::parse_from_rfc3339(input_date) {
            Ok(date) => {
                // println!("Parsed date from RFC 3339 Fallback: {:?}", date);
                let parsed_date_string = date.clone().to_rfc3339();
                return Ok(parsed_date_string);
            }
            Err(_) => {
                return Err("Could not parse date".into());
            }
        },
    }
}

// Time Zone stuff

/// Converts a timezone abbreviation (e.g., "IST") to standard %z notation (e.g., "+0530").
/// Returns "+0000" for unrecognized timezones.
#[allow(dead_code)]
fn timezone_to_offset(tz: &str) -> String {
    let timezone_map = get_timezone_offset_map();

    // Retrieve the offset in seconds or default to UTC (0 seconds offset)
    let offset = timezone_map.get(tz).copied().unwrap_or(0);

    // Convert the offset to %z format
    let total_minutes = offset / 60;
    let hours = total_minutes / 60;
    let minutes = total_minutes.abs() % 60;
    format!("{:+03}{:02}", hours, minutes)
}

/// Returns a map of timezone abbreviations to their offsets in seconds.
#[allow(dead_code)]
fn get_timezone_offset_map() -> HashMap<&'static str, i32> {
    let mut map = HashMap::new();

    // Common timezones (add more as needed)
    map.insert("IST", 5 * 3600 + 30 * 60); // Indian Standard Time: +0530
    map.insert("PST", -8 * 3600); // Pacific Standard Time: -0800
    map.insert("EST", -5 * 3600); // Eastern Standard Time: -0500
    map.insert("CST", -6 * 3600); // Central Standard Time: -0600
    map.insert("GMT", 0); // Greenwich Mean Time: +0000
    map.insert("UTC", 0); // Coordinated Universal Time: +0000
    map.insert("MST", -7 * 3600); // Mountain Standard Time: -0700
    map.insert("AEDT", 11 * 3600); // Australian Eastern Daylight Time: +1100
    map.insert("ACST", 9 * 3600 + 30 * 60); // Australian Central Standard Time: +0930

    map
}

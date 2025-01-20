use chrono::DateTime;
use clap::Parser;
use regex::Regex;
use reqwest::header::HeaderMap;
use serde::{Serialize as SerializeImpl, Serializer};
use serde_derive::{Deserialize, Serialize};
use std::collections::HashMap;
use std::error::Error;
use tokio;

const BILL_TYPE_AMEX: &str = "Amex";
const BILL_TYPE_SAVINGS_ACCOUNT: &str = "SavingsAccount";
const BILL_TYPE_TESTING: &str = "Testing";
const BILL_TYPE_JUPITER: &str = "Jupiter";
const BILL_TYPE_SBI_PRIME: &str = "SBI-Prime";

// References
// https://docs.rs/jiff/latest/jiff/#parsing-an-rfc-2822-datetime-string
// https://docs.rs/strptime/latest/strptime/struct.Parser.html
const TRANSACTION_DATE_FORMAT_AMEX: &str = "%d %B, %Y at %I:%M %p %z";
const TRANSACTION_DATE_FORMAT_JUPITER: &str = "%-m/%-d/%y, %I:%M %p %z";
const TRANSACTION_DATE_FORMAT_SBI_PRIME: &str = "%d/%m/%y %H:%M %z";

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
struct NocoBDResponse {
    list: Vec<Transaction>,
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
    #[serde(rename = "Id")]
    id: i32,
    #[serde(rename = "CreatedAt")]
    created_at: Option<String>,
    bills: Option<Bill>,
    #[serde(rename = "BackDateString", skip_serializing_if = "Option::is_none")]
    back_date_string: Option<String>,
    #[serde(rename = "UpdatedAt")]
    updated_at: Option<String>,
    // #[serde(rename = "Image")]
    // image: Option<String>,
    #[serde(rename = "Amount")]
    amount: Option<f64>,
    #[serde(rename = "nc_14ri__bills_id")]
    bills_id: Option<i32>,
    notes: Option<String>,
    #[serde(rename = "ParseStatus")]
    parse_status: Option<String>,
    #[serde(rename = "BillType")]
    bill_type: String,
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

// Function to parse the transaction text for AMEX transactions
async fn parse_transaction(
    record: Transaction,
    base_name: String,
    table_name: String,
) -> Result<(), Box<dyn std::error::Error>> {
    let token =
        std::env::var("NOCODB_INTEGRATION_TOKEN").expect("NOCODB_INTEGRATION_TOKEN must be set");

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

    if record.bill_type == BILL_TYPE_AMEX {
        let re = Regex::new(
            r"(\w+): You've spent (\w+) (\d+\,?\d+.\d+) on your AMEX [a-zA-Z\s]*[Cc]ard .* at (.*)\s* on ([^\.]*)(\w{3,4})\.",
        )
        .unwrap();
        if let Some(caps) = re.captures(&record.transaction_text.unwrap()) {
            let amount = caps
                .get(3)
                .unwrap()
                .as_str()
                .trim()
                .to_string()
                .replace(',', "");

            let date_string_capture = caps.get(5).unwrap().as_str().trim().to_string();

            let time_zone_capture = caps.get(6).unwrap().as_str();
            let time_zone_percentage_z_format = timezone_to_offset(time_zone_capture);

            let full_back_date_capture =
                format!("{} {}", date_string_capture, time_zone_percentage_z_format);

            println!("captured date string - {}", date_string_capture.clone());
            match parse_custom_date(
                full_back_date_capture.clone().as_str(),
                TRANSACTION_DATE_FORMAT_AMEX,
                false,
            ) {
                Ok(date_string) => {
                    println!("Parsed date: {:?}", date_string);
                    changes.insert("BackDate".to_string(), Value::Str(date_string));
                }
                Err(_) => {
                    // No worries if parsing failed, other fallbacks are present
                    // Ignore this error
                    println!("Unable to parse date from transaction text");
                }
            }

            changes.insert(
                "Merchant".to_string(),
                Value::Str(caps.get(4).unwrap().as_str().trim().to_string()),
            );
            changes.insert(
                "Currency".to_string(),
                Value::Str(caps.get(2).unwrap().as_str().trim().to_string()),
            );
            changes.insert(
                "Amount".to_string(),
                Value::F64(amount.parse::<f64>().unwrap()),
            );
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("ParsedV1".to_string()),
            );
        } else {
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("FailedV1".to_string()),
            );
        }
    } else if record.bill_type == BILL_TYPE_JUPITER {
        // println!("trying to parse SB");
        // let re = Regex::new(r"(\w+): You've spent (\w+) (\d+\.\d+) on your AMEX card .* at (.*)\s*on")
        let re = Regex::new(r"Rs (\d+\.\d+) debited .* on ([^-]*) -").unwrap();
        if let Some(caps) = re.captures(&record.transaction_text.unwrap()) {
            // changes.insert("Currency".to_string(), Value::Str("INR".to_string()));
            changes.insert(
                "Amount".to_string(),
                Value::F64(
                    caps.get(1)
                        .unwrap()
                        .as_str()
                        .trim()
                        .to_string()
                        .parse::<f64>()
                        .unwrap(),
                ),
            );
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("ParsedV1".to_string()),
            );

            let date_string_capture = caps.get(2).unwrap().as_str().trim().to_string();
            let full_back_date_capture = format!("{} +0530", date_string_capture);
            println!("Date string captured {}", full_back_date_capture);

            match parse_custom_date(
                full_back_date_capture.clone().as_str(),
                TRANSACTION_DATE_FORMAT_JUPITER,
                true,
            ) {
                Ok(date_string) => {
                    println!("Parsed date: {:?}", date_string);
                    changes.insert("BackDate".to_string(), Value::Str(date_string));
                }
                Err(e) => {
                    // No worries if parsing failed, other fallbacks are present
                    // Ignore this error
                    eprintln!("Unable to parse date from transaction text - {}", e);
                }
            }
        } else {
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("FailedV1".to_string()),
            );
        }
    } else if record.bill_type == BILL_TYPE_SBI_PRIME {
        // println!("trying to parse SB");
        // let re = Regex::new(r"(\w+): You've spent (\w+) (\d+\.\d+) on your AMEX card .* at (.*)\s*on")
        let re = Regex::new(r"([\w\.]*)(\d+\,?\d+.\d+) spent .* at ([a-zA-Z\*]*) on ([\d\/]*)\.")
            .unwrap();
        if let Some(caps) = re.captures(&record.transaction_text.unwrap()) {
            changes.insert(
                "Currency".to_string(),
                Value::Str(caps.get(1).unwrap().as_str().trim().to_string()),
            );
            // println!("amount cpature {}", caps.get(1).unwrap().as_str());
            changes.insert(
                "Amount".to_string(),
                Value::F64(
                    caps.get(2)
                        .unwrap()
                        .as_str()
                        .trim()
                        .to_string()
                        .replace(',', "")
                        .parse::<f64>()
                        .unwrap(),
                ),
            );

            changes.insert(
                "Merchant".to_string(),
                Value::Str(caps.get(3).unwrap().as_str().trim().to_string()),
            );

            let date_string_capture = caps.get(4).unwrap().as_str().trim().to_string();
            let full_back_date_capture = format!("{} 00:00 +0530", date_string_capture);
            println!("Date string captured {}", full_back_date_capture);

            match parse_custom_date(
                full_back_date_capture.clone().as_str(),
                TRANSACTION_DATE_FORMAT_SBI_PRIME,
                true,
            ) {
                Ok(date_string) => {
                    println!("Parsed date: {:?}", date_string);
                    changes.insert("BackDate".to_string(), Value::Str(date_string));
                }
                Err(e) => {
                    // No worries if parsing failed, other fallbacks are present
                    // Ignore this error
                    eprintln!("Unable to parse date from transaction text - {}", e);
                }
            }
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("ParsedV1".to_string()),
            );
        } else {
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("FailedV1".to_string()),
            );
        }
    } else if record.bill_type == BILL_TYPE_SAVINGS_ACCOUNT {
        // println!("trying to parse SB");
        // let re = Regex::new(r"(\w+): You've spent (\w+) (\d+\.\d+) on your AMEX card .* at (.*)\s*on")
        let re = Regex::new(
            r"Dear UPI user A/C X\d{4} debited by (\d+\.\d+) on date .* trf to ([\s\w]*) Refno .*",
        )
        .unwrap();
        if let Some(caps) = re.captures(&record.transaction_text.unwrap()) {
            // changes.insert("Merchant".to_string(), Value::Str("".to_string()));
            changes.insert(
                "Merchant".to_string(),
                Value::Str(caps.get(2).unwrap().as_str().trim().to_string()),
            );
            changes.insert("Currency".to_string(), Value::Str("INR".to_string()));
            changes.insert(
                "Amount".to_string(),
                Value::F64(
                    caps.get(1)
                        .unwrap()
                        .as_str()
                        .trim()
                        .to_string()
                        .parse::<f64>()
                        .unwrap(),
                ),
            );
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("ParsedV1".to_string()),
            );
        } else {
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("FailedV1".to_string()),
            );
        }
    } else if record.bill_type == BILL_TYPE_TESTING {
        println!("Parsing for test bill");
        let re = Regex::new(
            r"(\w+): You've spent (\w+) (\d+\,?\d+.\d+) on your AMEX card .* at (.*)\s*on",
        )
        .unwrap();
        if let Some(caps) = re.captures(&record.transaction_text.unwrap()) {
            println!("captured details");
            let amount = caps
                .get(3)
                .unwrap()
                .as_str()
                .trim()
                .to_string()
                .replace(',', "");

            changes.insert(
                "Merchant".to_string(),
                Value::Str(caps.get(4).unwrap().as_str().trim().to_string()),
            );
            changes.insert(
                "Currency".to_string(),
                Value::Str(caps.get(2).unwrap().as_str().trim().to_string()),
            );
            changes.insert(
                "Amount".to_string(),
                Value::F64(amount.parse::<f64>().unwrap()),
            );
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("ParsedV1".to_string()),
            );
        } else {
            changes.insert(
                "ParseStatus".to_string(),
                Value::Str("FailedV1".to_string()),
            );
        }
    } else {
        changes.insert(
            "ParseStatus".to_string(),
            Value::Str("FailedV1".to_string()),
        );
    }

    // match serde_json::to_string(&changes) {
    //     Ok(json_str) => {
    //         // Print the `curl` command with the serialized JSON data.
    //         println!("curl --location -X PATCH 'http://NOCODB_BASE_URL/api/v1/db/data/nc/payobills/transactions/{}' --header 'xc-token: {}' --header 'Content-Type: application/json' --data '{}'", record.id, token,json_str);
    //     },
    //     Err(e) => {
    //         println!("Error serializing changes: {:?}", e);
    //     },
    // }

    let mut map = HeaderMap::new();
    map.insert("xc-token", token.parse().unwrap());
    map.insert(
        "Content-Type",
        "application/json".to_string().as_str().parse().unwrap(),
    );

    let nocodb_base_url = std::env::var("NOCODB_BASE_URL").expect("NOCODB_BASE_URL should be set");
    let url: String = format!(
        "{}/api/v1/db/data/nc/{}/{}/{}",
        nocodb_base_url, base_name, table_name, record.id
    );

    // let response: NocoBDResponse
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

// Function to process the CSV transactions
async fn process_transactions(base_name: String, table_name: String) -> Result<(), Box<dyn Error>> {
    // let mut offset = 0;
    let mut parse_more = true;

    let token =
        std::env::var("NOCODB_INTEGRATION_TOKEN").expect("NOCODB_INTEGRATION_TOKEN must be set");

    while parse_more == true {
        let mut map = HeaderMap::new();
        map.insert("xc-token", token.parse().unwrap());

        let nocodb_base_url =
            std::env::var("NOCODB_BASE_URL").expect("NOCODB_BASE_URL should be set");
        let url: String = format!(
            "{}/api/v1/db/data/nc/{}/{}?w=(ParseStatus,eq,NotStarted)&l=1000&fields=*",
            nocodb_base_url,
            base_name.clone(),
            table_name.clone()
        );

        println!("{}", url.clone());

        // let response: NocoBDResponse
        let response = reqwest::Client::new()
            .get(url.clone())
            .headers(map)
            .send()
            .await?
            .json::<NocoBDResponse>()
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
                Some(_) => {
                    parse_transaction(transaction.clone(), base_name.clone(), table_name.clone())
                        .await?
                }
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
) -> Result<String, Box<dyn std::error::Error>> {
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

#[tokio::main]
async fn main() {
    let base_name = std::env::var("NOCODB_BASE_NAME").expect("NOCODB_BASE_NAME must be set");
    let table_name = std::env::var("NOCODB_TABLE_NAME").expect("NOCODB_TABLE_NAME must be set");

    process_transactions(base_name, table_name)
        .await
        .expect("Unable to parse transactions");
}

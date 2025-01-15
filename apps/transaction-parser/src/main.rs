use chrono::{format, DateTime};
use clap::{builder::TypedValueParser, Parser};
use regex::Regex;
use reqwest::header::HeaderMap;
use serde::{Serialize as SerializeImpl, Serializer};
use serde_derive::{Deserialize, Serialize};
use std::collections::HashMap;
use std::{error::Error, str::FromStr};
use tokio;

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
    bills_id: i32,
    notes: Option<String>,
    #[serde(rename = "ParseStatus")]
    parse_status: Option<String>,
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

    let mut parsed_date_string: String = String::new();

    let mut changes: HashMap<String, Value> = HashMap::new();
    let format = "%a, %d %b %Y %H:%M:%S %z";

    println!("Trying to parse {:?}", record.back_date_string.clone());
    match DateTime::parse_from_str(record.back_date_string.clone().unwrap().as_str(), format) {
        Ok(date) => {
            println!("Parsed date: {:?}", date);
            parsed_date_string = date.clone().to_rfc3339();
            changes.insert("BackDate".to_string(), Value::Str(parsed_date_string));
        }
        Err(_) => {
            match DateTime::parse_from_rfc3339(record.back_date_string.clone().unwrap().as_str()) {
                Ok(date) => {
                    println!("Parsed date: {:?}", date);
                    parsed_date_string = date.clone().to_rfc3339();
                }
                Err(_) => {
                    // return Err("Could not parse date".into());
                }
            }
        }
    }


    if record.bills.as_ref().unwrap().name.to_string() == String::from("AMEX") {
        let re = Regex::new(
            r"(\w+): You've spent (\w+) (\d+\,?\d+.\d+) on your AMEX card .* at (.*)\s*on",
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
    } else if record.bills.as_ref().unwrap().name.to_string() == String::from("SB Account") {
        // println!("trying to parse SB");
        // let re = Regex::new(r"(\w+): You've spent (\w+) (\d+\.\d+) on your AMEX card .* at (.*)\s*on")
        let re = Regex::new(
            r"Dear UPI user A/C X6902 debited by (\d+\.\d+) on date .* trf to ([\s\w]*) Refno .*",
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
    } else if record.bills.unwrap().name.to_string() == String::from("Test Bill") {
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
        .text()
        .await?;

    println!("parse update resp {:?}", response);
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

        println!("retrieved transactions to parse");
        // println!("{}", serde_json::to_string(&response)?);
        // parse_more = !response.page_info.is_last_page;
        // println!("parse_more: {}", parse_more);
        // offset = offset + response.page_info.page_size;

        // Parse transactions only if it is attached to a bill
        for transaction in response.list {
            match transaction.bills {
                Some(_) => {
                    parse_transaction(transaction.clone(), base_name.clone(), table_name.clone()).await?
                }
                None => {}
            }
        }

        parse_more = false;
    }

    Ok(())
}

#[tokio::main]
async fn main() {
    let base_name = std::env::var("NOCODB_BASE_NAME").expect("NOCODB_BASE_NAME must be set");
    let table_name = std::env::var("NOCODB_TABLE_NAME").expect("NOCODB_TABLE_NAME must be set");

    process_transactions(base_name, table_name)
        .await
        .expect("Unable to parse transactions");
}

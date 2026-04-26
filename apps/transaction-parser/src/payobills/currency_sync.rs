use reqwest::header::HeaderMap;
use serde_derive::{Deserialize, Serialize};
use std::collections::HashMap;

use crate::payobills::transaction_parser::NocoDBEnv;

#[derive(Deserialize)]
struct OpenExchangeRatesResponse {
    base: String,
    rates: HashMap<String, f64>,
}

#[derive(Serialize)]
struct HistoricalRateRecord {
    #[serde(rename = "Date")]
    date: String,
    #[serde(rename = "ExchangeData")]
    exchange_data: ExchangeData,
}

#[derive(Serialize, Deserialize)]
struct ExchangeData {
    rates: HashMap<String, f64>,
}

#[derive(Deserialize)]
struct RateRecord {
    #[serde(rename = "ExchangeData")]
    exchange_data: ExchangeData,
}

#[derive(Deserialize)]
struct RateListResponse {
    list: Vec<RateRecord>,
}

pub async fn fetch_and_store_rates(
    nocodb_env: NocoDBEnv,
    app_id: String,
    date: String,
) -> Result<HashMap<String, f64>, Box<dyn std::error::Error>> {
    let mut headers = HeaderMap::new();
    headers.insert("xc-token", nocodb_env.api_key.parse()?);

    // Check if rates for this date already exist in NocoDB
    let check_url = format!(
        "{}/api/v1/db/data/nc/{}/{}?w=(Date,eq,exactDate,{})&l=1",
        nocodb_env.base_url,
        nocodb_env.base_name_currencies,
        nocodb_env.table_name_currencies_historical,
        date,
    );

    let check_response = reqwest::Client::new()
        .get(&check_url)
        .headers(headers.clone())
        .send()
        .await?
        .json::<RateListResponse>()
        .await?;

    if let Some(existing) = check_response.list.into_iter().next() {
        return Ok(existing.exchange_data.rates);
    }

    // Fetch historical rates from openexchangerates.org
    let oxr_url = format!(
        "https://openexchangerates.org/api/historical/{}.json?app_id={}",
        date, app_id
    );

    let oxr_resp = reqwest::Client::new().get(&oxr_url).send().await?;

    if !oxr_resp.status().is_success() {
        let status = oxr_resp.status();
        let body = oxr_resp.text().await.unwrap_or_default();
        return Err(
            format!("openexchangerates.org request failed ({}): {}", status, body).into(),
        );
    }

    let oxr_data = oxr_resp.json::<OpenExchangeRatesResponse>().await?;

    if oxr_data.rates.is_empty() {
        return Err(format!("openexchangerates.org returned empty rates for {}", date).into());
    }

    let record = HistoricalRateRecord {
        date: date.clone(),
        exchange_data: ExchangeData {
            rates: oxr_data.rates.clone(),
        },
    };

    // Insert into NocoDB
    let insert_url = format!(
        "{}/api/v1/db/data/nc/{}/{}",
        nocodb_env.base_url,
        nocodb_env.base_name_currencies,
        nocodb_env.table_name_currencies_historical,
    );

    headers.insert("Content-Type", "application/json".parse()?);

    let insert_resp = reqwest::Client::new()
        .post(&insert_url)
        .headers(headers)
        .json(&record)
        .send()
        .await?;

    if !insert_resp.status().is_success() {
        let status = insert_resp.status();
        let body = insert_resp.text().await.unwrap_or_default();
        return Err(format!("NocoDB insert failed ({}): {}", status, body).into());
    }

    Ok(oxr_data.rates)
}

pub async fn get_conversion_rates(
    nocodb_env: NocoDBEnv,
    app_id: String,
    date: String,
) -> Result<HashMap<String, f64>, Box<dyn std::error::Error>> {
    fetch_and_store_rates(nocodb_env, app_id, date).await
}

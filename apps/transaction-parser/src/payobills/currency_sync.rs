use chrono::Utc;
use reqwest::header::HeaderMap;
use serde_derive::{Deserialize, Serialize};
use std::collections::HashMap;

use crate::payobills::transaction_parser::NocoDBEnv;

pub struct OXREnv {
    pub app_id: String,
}

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
    #[serde(skip_serializing_if = "Option::is_none")]
    current: Option<bool>,
}

#[derive(Deserialize)]
struct RateRecord {
    id: u64,
    #[serde(rename = "ExchangeData")]
    exchange_data: ExchangeData,
}

#[derive(Deserialize)]
struct RateListResponse {
    list: Vec<RateRecord>,
}

fn today_utc() -> String {
    Utc::now().format("%Y-%m-%d").to_string()
}

async fn fetch_latest_rates(
    oxr_base_url: &str,
    app_id: &str,
) -> Result<HashMap<String, f64>, Box<dyn std::error::Error + Send + Sync>> {
    let url = format!("{}/api/latest.json?app_id={}", oxr_base_url, app_id);
    let resp = reqwest::Client::new().get(&url).send().await?;
    if !resp.status().is_success() {
        let status = resp.status();
        let body = resp.text().await.unwrap_or_default();
        return Err(
            format!("openexchangerates.org latest request failed ({}): {}", status, body).into(),
        );
    }
    let data = resp.json::<OpenExchangeRatesResponse>().await?;
    if data.rates.is_empty() {
        return Err("openexchangerates.org returned empty latest rates".into());
    }
    Ok(data.rates)
}

async fn fetch_historical_rates(
    oxr_base_url: &str,
    app_id: &str,
    date: &str,
) -> Result<Option<HashMap<String, f64>>, Box<dyn std::error::Error + Send + Sync>> {
    let url = format!(
        "{}/api/historical/{}.json?app_id={}",
        oxr_base_url, date, app_id
    );
    let resp = reqwest::Client::new().get(&url).send().await?;
    let status = resp.status();
    if status == reqwest::StatusCode::NOT_FOUND {
        // 404 means the day hasn't closed in the westernmost timezone yet
        return Ok(None);
    }
    if !status.is_success() {
        let body = resp.text().await.unwrap_or_default();
        return Err(format!(
            "openexchangerates.org historical request failed ({}): {}",
            status, body
        )
        .into());
    }
    let data = resp.json::<OpenExchangeRatesResponse>().await?;
    if data.rates.is_empty() {
        return Err(format!(
            "openexchangerates.org returned empty historical rates for {}",
            date
        )
        .into());
    }
    Ok(Some(data.rates))
}

async fn insert_rate_record(
    nocodb_env: &NocoDBEnv,
    headers: HeaderMap,
    date: &str,
    exchange_data: ExchangeData,
) -> Result<(), Box<dyn std::error::Error + Send + Sync>> {
    let insert_url = format!(
        "{}/api/v1/db/data/nc/{}/{}",
        nocodb_env.base_url,
        nocodb_env.base_name_currencies,
        nocodb_env.table_name_currencies_historical,
    );
    let record = HistoricalRateRecord {
        date: date.to_string(),
        exchange_data,
    };
    let resp = reqwest::Client::new()
        .post(&insert_url)
        .headers(headers)
        .json(&record)
        .send()
        .await?;
    if !resp.status().is_success() {
        let status = resp.status();
        let body = resp.text().await.unwrap_or_default();
        return Err(format!("NocoDB insert failed ({}): {}", status, body).into());
    }
    Ok(())
}

async fn patch_rate_record(
    nocodb_env: &NocoDBEnv,
    headers: HeaderMap,
    row_id: u64,
    exchange_data: ExchangeData,
) -> Result<(), Box<dyn std::error::Error + Send + Sync>> {
    let patch_url = format!(
        "{}/api/v1/db/data/nc/{}/{}/{}",
        nocodb_env.base_url,
        nocodb_env.base_name_currencies,
        nocodb_env.table_name_currencies_historical,
        row_id,
    );

    #[derive(Serialize)]
    struct PatchRecord {
        #[serde(rename = "ExchangeData")]
        exchange_data: ExchangeData,
    }

    let resp = reqwest::Client::new()
        .patch(&patch_url)
        .headers(headers)
        .json(&PatchRecord { exchange_data })
        .send()
        .await?;
    if !resp.status().is_success() {
        let status = resp.status();
        let body = resp.text().await.unwrap_or_default();
        return Err(format!("NocoDB patch failed ({}): {}", status, body).into());
    }
    Ok(())
}

async fn fetch_and_store_rates_inner(
    nocodb_env: NocoDBEnv,
    app_id: String,
    date: String,
    oxr_base_url: &str,
    today: &str,
) -> Result<HashMap<String, f64>, Box<dyn std::error::Error + Send + Sync>> {
    let mut headers = HeaderMap::new();
    headers.insert("xc-token", nocodb_env.api_key.parse()?);
    headers.insert("Content-Type", "application/json".parse()?);

    let is_today = date == today;

    let check_url = format!(
        "{}/api/v1/db/data/nc/{}/{}?w=(Date,eq,exactDate,{})&l=1",
        nocodb_env.base_url,
        nocodb_env.base_name_currencies,
        nocodb_env.table_name_currencies_historical,
        date,
    );

    let existing = reqwest::Client::new()
        .get(&check_url)
        .headers(headers.clone())
        .send()
        .await?
        .json::<RateListResponse>()
        .await?
        .list
        .into_iter()
        .next();

    if is_today {
        match existing {
            Some(row) if row.exchange_data.current == Some(true) => {
                // Refresh today's in-progress rates
                let rates = fetch_latest_rates(oxr_base_url, &app_id).await?;
                patch_rate_record(
                    &nocodb_env,
                    headers,
                    row.id,
                    ExchangeData { rates: rates.clone(), current: Some(true) },
                )
                .await?;
                Ok(rates)
            }
            _ => {
                // No row yet for today — insert fresh current record
                let rates = fetch_latest_rates(oxr_base_url, &app_id).await?;
                insert_rate_record(
                    &nocodb_env,
                    headers,
                    &date,
                    ExchangeData { rates: rates.clone(), current: Some(true) },
                )
                .await?;
                Ok(rates)
            }
        }
    } else {
        match existing {
            Some(row) if row.exchange_data.current.is_none() => {
                // Finalized historical record — return as-is
                Ok(row.exchange_data.rates)
            }
            Some(row) => {
                // current=true row from a past date — try to promote to historical
                match fetch_historical_rates(oxr_base_url, &app_id, &date).await? {
                    Some(rates) => {
                        patch_rate_record(
                            &nocodb_env,
                            headers,
                            row.id,
                            ExchangeData { rates: rates.clone(), current: None },
                        )
                        .await?;
                        Ok(rates)
                    }
                    // 404: date hasn't closed in the westernmost timezone yet — reuse cached rates
                    None => Ok(row.exchange_data.rates),
                }
            }
            None => {
                // No record at all — try historical, fall back to latest only on 404
                match fetch_historical_rates(oxr_base_url, &app_id, &date).await? {
                    Some(rates) => {
                        insert_rate_record(
                            &nocodb_env,
                            headers,
                            &date,
                            ExchangeData { rates: rates.clone(), current: None },
                        )
                        .await?;
                        Ok(rates)
                    }
                    // 404: date hasn't closed in the westernmost timezone — store as current
                    None => {
                        let rates = fetch_latest_rates(oxr_base_url, &app_id).await?;
                        insert_rate_record(
                            &nocodb_env,
                            headers,
                            &date,
                            ExchangeData { rates: rates.clone(), current: Some(true) },
                        )
                        .await?;
                        Ok(rates)
                    }
                }
            }
        }
    }
}

pub async fn fetch_and_store_rates(
    nocodb_env: NocoDBEnv,
    app_id: String,
    date: String,
) -> Result<HashMap<String, f64>, Box<dyn std::error::Error + Send + Sync>> {
    fetch_and_store_rates_inner(
        nocodb_env,
        app_id,
        date,
        "https://openexchangerates.org",
        &today_utc(),
    )
    .await
}

pub async fn get_conversion_rates(
    nocodb_env: NocoDBEnv,
    app_id: String,
    date: String,
) -> Result<HashMap<String, f64>, Box<dyn std::error::Error + Send + Sync>> {
    fetch_and_store_rates(nocodb_env, app_id, date).await
}

#[cfg(test)]
mod tests {
    use super::*;
    use httpmock::prelude::*;
    use httpmock::Method::PATCH;
    use serde_json::json;

    const BASE: &str = "base1";
    const TABLE: &str = "table1";
    const APP_ID: &str = "test-app-id";
    const TODAY: &str = "2026-04-30";
    const PAST: &str = "2026-04-28";
    const ROW_ID: u64 = 42;

    fn nocodb_env(base_url: &str) -> NocoDBEnv {
        NocoDBEnv {
            base_url: base_url.to_string(),
            api_key: "test-token".to_string(),
            base_name_currencies: BASE.to_string(),
            table_name_currencies_historical: TABLE.to_string(),
            base_name_payobills: String::new(),
            table_name_currencies_currencies: String::new(),
            table_name_payobills_transactions: String::new(),
        }
    }

    fn nocodb_check_path() -> String {
        format!("/api/v1/db/data/nc/{}/{}", BASE, TABLE)
    }

    fn nocodb_row_path(id: u64) -> String {
        format!("/api/v1/db/data/nc/{}/{}/{}", BASE, TABLE, id)
    }

    fn nocodb_insert_path() -> String {
        format!("/api/v1/db/data/nc/{}/{}", BASE, TABLE)
    }

    fn empty_list() -> serde_json::Value {
        json!({ "list": [] })
    }

    fn list_with_row(id: u64, current: Option<bool>, rates: serde_json::Value) -> serde_json::Value {
        let mut exchange_data = json!({ "rates": rates });
        if let Some(c) = current {
            exchange_data["current"] = json!(c);
        }
        json!({ "list": [{ "id": id, "ExchangeData": exchange_data }] })
    }

    fn oxr_rates_body() -> serde_json::Value {
        json!({ "base": "USD", "rates": { "EUR": 0.92, "INR": 83.5 } })
    }

    fn expected_rates() -> HashMap<String, f64> {
        let mut m = HashMap::new();
        m.insert("EUR".to_string(), 0.92);
        m.insert("INR".to_string(), 83.5);
        m
    }

    async fn run(
        nocodb: &MockServer,
        oxr: &MockServer,
        date: &str,
        today: &str,
    ) -> Result<HashMap<String, f64>, Box<dyn std::error::Error + Send + Sync>> {
        fetch_and_store_rates_inner(
            nocodb_env(&nocodb.base_url()),
            APP_ID.to_string(),
            date.to_string(),
            &oxr.base_url(),
            today,
        )
        .await
    }

    // Case 1: today, no row → fetch latest → insert current=true
    #[tokio::test]
    async fn today_no_row_inserts_current() {
        let nocodb = MockServer::start();
        let oxr = MockServer::start();

        let check = nocodb.mock(|when, then| {
            when.method(GET).path(nocodb_check_path());
            then.status(200).json_body(empty_list());
        });
        let oxr_mock = oxr.mock(|when, then| {
            when.method(GET).path("/api/latest.json");
            then.status(200).json_body(oxr_rates_body());
        });
        let insert = nocodb.mock(|when, then| {
            when.method(POST)
                .path(nocodb_insert_path())
                .body_contains("\"current\":true");
            then.status(200).json_body(json!({}));
        });

        let result = run(&nocodb, &oxr, TODAY, TODAY).await.unwrap();

        assert_eq!(result, expected_rates());
        check.assert();
        oxr_mock.assert();
        insert.assert();
    }

    // Case 2: today, row with current=true → fetch latest → patch current=true
    #[tokio::test]
    async fn today_current_row_patches_current() {
        let nocodb = MockServer::start();
        let oxr = MockServer::start();

        let cached = json!({ "EUR": 0.90, "INR": 83.0 });
        let check = nocodb.mock(|when, then| {
            when.method(GET).path(nocodb_check_path());
            then.status(200).json_body(list_with_row(ROW_ID, Some(true), cached));
        });
        let oxr_mock = oxr.mock(|when, then| {
            when.method(GET).path("/api/latest.json");
            then.status(200).json_body(oxr_rates_body());
        });
        let patch = nocodb.mock(|when, then| {
            when.method(PATCH)
                .path(nocodb_row_path(ROW_ID))
                .body_contains("\"current\":true");
            then.status(200).json_body(json!({}));
        });

        let result = run(&nocodb, &oxr, TODAY, TODAY).await.unwrap();

        assert_eq!(result, expected_rates());
        check.assert();
        oxr_mock.assert();
        patch.assert();
    }

    // Case 3: past date, row with current=null → return cached, no OXR calls
    #[tokio::test]
    async fn past_historical_row_returns_cached() {
        let nocodb = MockServer::start();
        let oxr = MockServer::start();

        let cached = json!({ "EUR": 0.91, "INR": 83.2 });
        let check = nocodb.mock(|when, then| {
            when.method(GET).path(nocodb_check_path());
            then.status(200)
                .json_body(list_with_row(ROW_ID, None, cached.clone()));
        });
        let oxr_mock = oxr.mock(|when, then| {
            when.any_request();
            then.status(500);
        });

        let result = run(&nocodb, &oxr, PAST, TODAY).await.unwrap();

        let mut expected = HashMap::new();
        expected.insert("EUR".to_string(), 0.91);
        expected.insert("INR".to_string(), 83.2);
        assert_eq!(result, expected);
        check.assert();
        oxr_mock.assert_hits(0);
    }

    // Case 4: past date, row with current=true, historical available → patch to current=null
    #[tokio::test]
    async fn past_current_row_historical_available_promotes_to_historical() {
        let nocodb = MockServer::start();
        let oxr = MockServer::start();

        let stale = json!({ "EUR": 0.90, "INR": 83.0 });
        let check = nocodb.mock(|when, then| {
            when.method(GET).path(nocodb_check_path());
            then.status(200).json_body(list_with_row(ROW_ID, Some(true), stale));
        });
        let oxr_hist = oxr.mock(|when, then| {
            when.method(GET)
                .path(format!("/api/historical/{}.json", PAST));
            then.status(200).json_body(oxr_rates_body());
        });
        let patch = nocodb.mock(|when, then| {
            when.method(PATCH).path(nocodb_row_path(ROW_ID));
            // current field must be absent (serialized as null → omitted)
            then.status(200).json_body(json!({}));
        });

        let result = run(&nocodb, &oxr, PAST, TODAY).await.unwrap();

        assert_eq!(result, expected_rates());
        check.assert();
        oxr_hist.assert();
        patch.assert();
    }

    // Case 5: past date, row with current=true, historical not yet available → reuse cached
    #[tokio::test]
    async fn past_current_row_historical_unavailable_reuses_cached() {
        let nocodb = MockServer::start();
        let oxr = MockServer::start();

        let cached_rates = json!({ "EUR": 0.90, "INR": 83.0 });
        let check = nocodb.mock(|when, then| {
            when.method(GET).path(nocodb_check_path());
            then.status(200)
                .json_body(list_with_row(ROW_ID, Some(true), cached_rates.clone()));
        });
        let oxr_hist = oxr.mock(|when, then| {
            when.method(GET)
                .path(format!("/api/historical/{}.json", PAST));
            then.status(404).body("not found");
        });

        let result = run(&nocodb, &oxr, PAST, TODAY).await.unwrap();

        let mut expected = HashMap::new();
        expected.insert("EUR".to_string(), 0.90);
        expected.insert("INR".to_string(), 83.0);
        assert_eq!(result, expected);
        check.assert();
        oxr_hist.assert();
    }

    // Case 6: past date, no row, historical available → insert with current=null
    #[tokio::test]
    async fn past_no_row_historical_available_inserts_historical() {
        let nocodb = MockServer::start();
        let oxr = MockServer::start();

        let check = nocodb.mock(|when, then| {
            when.method(GET).path(nocodb_check_path());
            then.status(200).json_body(empty_list());
        });
        let oxr_hist = oxr.mock(|when, then| {
            when.method(GET)
                .path(format!("/api/historical/{}.json", PAST));
            then.status(200).json_body(oxr_rates_body());
        });
        // current field must be absent in the inserted record
        let insert = nocodb.mock(|when, then| {
            when.method(POST).path(nocodb_insert_path());
            then.status(200).json_body(json!({}));
        });

        let result = run(&nocodb, &oxr, PAST, TODAY).await.unwrap();

        assert_eq!(result, expected_rates());
        check.assert();
        oxr_hist.assert();
        insert.assert();
    }

    // Case 7: past date, no row, historical not available → fetch latest → insert with current=true
    #[tokio::test]
    async fn past_no_row_historical_unavailable_falls_back_to_latest() {
        let nocodb = MockServer::start();
        let oxr = MockServer::start();

        let check = nocodb.mock(|when, then| {
            when.method(GET).path(nocodb_check_path());
            then.status(200).json_body(empty_list());
        });
        let oxr_hist = oxr.mock(|when, then| {
            when.method(GET)
                .path(format!("/api/historical/{}.json", PAST));
            then.status(404).body("not found");
        });
        let oxr_latest = oxr.mock(|when, then| {
            when.method(GET).path("/api/latest.json");
            then.status(200).json_body(oxr_rates_body());
        });
        let insert = nocodb.mock(|when, then| {
            when.method(POST)
                .path(nocodb_insert_path())
                .body_contains("\"current\":true");
            then.status(200).json_body(json!({}));
        });

        let result = run(&nocodb, &oxr, PAST, TODAY).await.unwrap();

        assert_eq!(result, expected_rates());
        check.assert();
        oxr_hist.assert();
        oxr_latest.assert();
        insert.assert();
    }
}

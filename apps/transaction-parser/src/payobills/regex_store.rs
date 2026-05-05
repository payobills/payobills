use reqwest::header::HeaderMap;
use serde_derive::{Deserialize, Serialize};
use serde_json::Value;
use std::error::Error;

use crate::payobills::slm_client::FieldDescriptor;
use crate::payobills::transaction_parser::NocoDBEnv;

#[derive(Deserialize)]
struct RegexPatternRecord {
    #[serde(rename = "Pattern")]
    pattern: String,
    #[serde(rename = "Fields", default)]
    fields: Vec<FieldDescriptor>,
}

#[derive(Deserialize)]
struct NocoPaged<T> {
    list: Vec<T>,
}

pub struct StoredPattern {
    pub pattern: String,
    pub fields: Vec<FieldDescriptor>,
}

pub async fn get_patterns(
    nocodb_env: &NocoDBEnv,
    bill_type: &str,
) -> Result<Vec<StoredPattern>, Box<dyn Error + Send + Sync>> {
    let mut headers = HeaderMap::new();
    headers.insert("xc-token", nocodb_env.api_key.parse().unwrap());

    let url = format!(
        "{}/api/v1/db/data/nc/{}/{}?w=(BillType,eq,{})&l=1000&fields=Pattern,Fields",
        nocodb_env.base_url,
        nocodb_env.base_name_payobills,
        nocodb_env.table_name_regex_patterns,
        bill_type
    );

    let response = reqwest::Client::new()
        .get(&url)
        .headers(headers)
        .send()
        .await?
        .json::<NocoPaged<RegexPatternRecord>>()
        .await?;

    Ok(response.list.into_iter().map(|r| StoredPattern {
        pattern: r.pattern,
        fields: r.fields,
    }).collect())
}

#[derive(Serialize)]
struct NewRegexPattern<'a> {
    #[serde(rename = "BillType")]
    bill_type: &'a str,
    #[serde(rename = "Pattern")]
    pattern: &'a str,
    #[serde(rename = "Fields")]
    fields: Value,
}

pub async fn save_pattern(
    nocodb_env: &NocoDBEnv,
    bill_type: &str,
    pattern: &str,
    fields: &[FieldDescriptor],
) -> Result<(), Box<dyn Error + Send + Sync>> {
    let mut headers = HeaderMap::new();
    headers.insert("xc-token", nocodb_env.api_key.parse().unwrap());

    let url = format!(
        "{}/api/v1/db/data/nc/{}/{}",
        nocodb_env.base_url,
        nocodb_env.base_name_payobills,
        nocodb_env.table_name_regex_patterns
    );

    let fields_json: Vec<serde_json::Map<String, Value>> = fields
        .iter()
        .map(|f| {
            let mut m = serde_json::Map::new();
            m.insert("name".into(), Value::String(f.name.clone()));
            m.insert("type".into(), Value::String(f.field_type.clone()));
            m.insert(
                "format".into(),
                f.format.as_ref().map(|s| Value::String(s.clone())).unwrap_or(Value::Null),
            );
            m
        })
        .collect();

    let body = NewRegexPattern {
        bill_type,
        pattern,
        fields: Value::Array(fields_json.into_iter().map(Value::Object).collect()),
    };

    reqwest::Client::new()
        .post(&url)
        .headers(headers)
        .json(&body)
        .send()
        .await?;

    Ok(())
}

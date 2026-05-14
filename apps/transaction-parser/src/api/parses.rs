use axum::{extract::{Path, Query}, http::StatusCode, response::Json};
use serde_derive::Deserialize;
use serde_json::{json, Value};

#[derive(Deserialize)]
pub(crate) struct NormalizedAmountQuery {
    target_currency_code: Option<String>,
}

fn build_nocodb_env() -> crate::payobills::transaction_parser::NocoDBEnv {
    crate::payobills::transaction_parser::NocoDBEnv {
        base_url: std::env::var("NOCODB__BASE_URL").expect("NOCODB__BASE_URL must be set"),
        base_name_payobills: String::from("payobills"),
        table_name_payobills_transactions: String::from("transactions"),
        base_name_currencies: std::env::var("NOCODB__BASE_NAME__CURRENCIES")
            .expect("NOCODB__BASE_NAME__CURRENCIES must be set"),
        table_name_currencies_historical: std::env::var(
            "NOCODB__TABLE_NAME__CURRENCIES__HISTORICAL",
        )
        .expect("NOCODB__TABLE_NAME__CURRENCIES__HISTORICAL must be set"),
        table_name_currencies_currencies: std::env::var(
            "NOCODB__TABLE_NAME__CURRENCIES__CURRENCIES",
        )
        .expect("NOCODB__TABLE_NAME__CURRENCIES__CURRENCIES must be set"),
        api_key: std::env::var("NOCODB__INTEGRATION_TOKEN")
            .expect("NOCODB__INTEGRATION_TOKEN must be set"),
        table_name_regex_patterns: std::env::var("NOCODB__TABLE_NAME__REGEX_PATTERNS")
            .unwrap_or_else(|_| String::from("transaction_regex_patterns")),
    }
}

pub(crate) async fn patch_normalized_amount_handler(
    Path(transaction_id): Path<String>,
    Query(params): Query<NormalizedAmountQuery>,
) -> (StatusCode, Json<Value>) {
    let nocodb_env = build_nocodb_env();
    let target_currency_code = params.target_currency_code.unwrap_or_else(|| "USD".to_string());

    match crate::payobills::transaction_parser::normalize_transaction_amount(
        nocodb_env,
        transaction_id,
        target_currency_code.clone(),
    )
    .await
    {
        Ok(Some(())) => (
            StatusCode::OK,
            Json(json!({ "message": "NormalizedAmount updated successfully" })),
        ),
        Ok(None) => (
            StatusCode::OK,
            Json(json!({ "message": format!("Transaction already in {} — skipped", target_currency_code) })),
        ),
        Err(e) => (
            StatusCode::INTERNAL_SERVER_ERROR,
            Json(json!({ "error": e.to_string() })),
        ),
    }
}

pub(crate) async fn post_new_parse_handler(Path(transaction_id): Path<String>) -> Json<Value> {
    let nocodb_env = build_nocodb_env();

    let slm_env = std::env::var("SLMPARSER__BASE_URL")
        .ok()
        .map(|url| crate::payobills::transaction_parser::SLMParserEnv { base_url: url });

    let _ = crate::payobills::transaction_parser::parse_transaction_by_id(nocodb_env, transaction_id, slm_env)
            .await;

    Json(json!({ "message": "Transaction parsed successfully"}))
}

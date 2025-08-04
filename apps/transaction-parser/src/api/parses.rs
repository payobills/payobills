use axum::{extract::Path, response::Json};
use serde_json::{json, Value};

pub(crate) async fn post_new_parse_handler(Path(transaction_id): Path<String>) -> Json<Value> {
    // Here you would implement the logic to handle the parsing of the transaction
    // For now, we will just return a placeholder response
    // format!("Parsing transaction with ID: {}", transaction_id)
    let nocodb_env = crate::payobills::transaction_parser::NocoDBEnv {
        base_url: std::env::var("NOCODB__BASE_URL").expect("NOCODB__BASE_URL must be set"),

        base_name_payobills: String::from("payobills"),

        table_name_payobills_transactions: String::from("transactions"), // std::env::var("NOCODB_TABLE_NAME").expect("NOCODB_TABLE_NAME must be set")

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
    };

    let res =
        crate::payobills::transaction_parser::parse_transaction_by_id(nocodb_env, transaction_id)
            .await;

    Json(json!({ "message": "Transaction parsed successfully"}))
}

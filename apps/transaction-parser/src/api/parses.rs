use axum::{extract::Path, response::Json};
use serde_json::{json, Value};

pub(crate) async fn post_new_parse_handler(Path(transaction_id): Path<String>) -> Json<Value> {
    // Here you would implement the logic to handle the parsing of the transaction
    // For now, we will just return a placeholder response
    // format!("Parsing transaction with ID: {}", transaction_id)
    Json(json!({ "data": transaction_id }))
}

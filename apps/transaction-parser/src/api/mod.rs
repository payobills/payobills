use axum::{routing::*, Json};
use serde_json::{json, Value};

use crate::api::parses::post_new_parse_handler;

mod parses;

async fn get_health_check() -> Json<Value> { 
    return Json(json!({ "app": "payobills.transaction-parser"}))
}

pub(crate) fn create_router() -> Router {
    return Router::new()
    .route(
        "/",
        get(get_health_check),
    )
    .route(
        "/api/parses/:transaction_id",
        post(move |transaction_id| post_new_parse_handler(transaction_id)),
    )
}

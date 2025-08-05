use axum::{routing::*};

use crate::api::parses::post_new_parse_handler;

mod parses;

pub(crate) fn create_router() -> Router {
    Router::new().merge(Router::new().route(
        "/api/parses/:transaction_id",
        post(move |transaction_id| post_new_parse_handler(transaction_id)),
    ))
}

use axum::{routing::*, *};
use api::parses;

pub fn create_router() -> Router {
    Router::new()
        .merge(
            Router::new()
                .route("/api/parses/:transactionId", post(parses::parse_handler))
        )
}

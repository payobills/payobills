use tokio;
pub mod payobills;
pub mod cli;

#[tokio::main]
async fn main() {

    cli::cli().await;
}

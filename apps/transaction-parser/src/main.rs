use clap::Parser;

pub mod api;
pub mod cli;
pub mod payobills;

#[derive(Debug, Clone, Parser)]
#[command(version, about, long_about = None)]
struct Args {
    #[arg(name = "sub_command")]
    sub_command: String,
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let matches = Args::parse();

    if matches.sub_command == String::from("api") {
        println!("Starting API server at 0.0.0.0:3000");

        let address = String::from("0.0.0.0:3000");
        return axum::Server::bind(&address.parse().unwrap())
        .serve(crate::api::create_router().into_make_service())
        .await
        .map_err(|e| Box::new(e) as Box<dyn std::error::Error>);
    } else {
        return crate::cli::cli()
        .await;
    }
}

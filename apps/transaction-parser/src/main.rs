use clap::Parser;

pub mod api;
pub mod cli;
pub mod payobills;

#[derive(Debug, Clone, Parser)]
#[command(about, long_about = None)]
struct Args {
    #[arg(
        name = "mode",
        default_value = "cli",
        help = "Mode to run, either 'api' or 'cli'"
    )]
    mode: String,
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let matches = Args::parse();

    if matches.mode == String::from("api") {
        println!("Starting API server at 0.0.0.0:3000");

        let address = String::from("0.0.0.0:3000");
        return axum::Server::bind(&address.parse().unwrap())
            .serve(crate::api::create_router().into_make_service())
            .await
            .map_err(|e| Box::new(e) as Box<dyn std::error::Error>);
    } else if matches.mode == String::from("cli") {
        return crate::cli::cli().await;
    } else {
        {
            return Err(Box::new(std::io::Error::new(
                std::io::ErrorKind::InvalidInput,
                "Unknown mode specified. Use 'api' or 'cli'.",
            )));
        }
    }
}

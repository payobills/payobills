use clap::Parser;
use log::info;
use simplelog::{CombinedLogger, Config, LevelFilter, SharedLogger, WriteLogger};

pub mod api;
pub mod cli;
pub mod payobills;

#[derive(Debug, Clone, Parser)]
#[command(version, about, long_about = None)]
struct Args {
    #[command(subcommand)]
    command: Commands,
}

#[derive(Debug, Clone, clap::Subcommand)]
enum Commands {
    Api,
    Cli,
}

fn init_logger() {
    let mut loggers: Vec<Box<dyn SharedLogger>> = vec![
        WriteLogger::new(LevelFilter::Debug, Config::default(), std::io::stdout()),
    ];

    match std::fs::OpenOptions::new()
        .create(true)
        .append(true)
        .open("/var/log/transaction-parser.log")
    {
        Ok(file) => loggers.push(WriteLogger::new(LevelFilter::Debug, Config::default(), file)),
        Err(e) => eprintln!("Could not open log file, file logging disabled: {}", e),
    }

    CombinedLogger::init(loggers).expect("Failed to initialise logger");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    init_logger();

    let matches = Args::parse();

    match matches.command {
        Commands::Api => {
            let address =
                std::env::var("SERVER_ADDRESS").unwrap_or_else(|_| "0.0.0.0:31004".to_string());
            info!("Starting API server at {}", address);

            return axum::Server::bind(&address.parse()?)
                .serve(crate::api::create_router().into_make_service())
                .await
                .map_err(|e| Box::new(e) as Box<dyn std::error::Error>);
        }
        Commands::Cli => {
            return crate::cli::cli().await.map_err(|e| e as Box<dyn std::error::Error>);
        }
    }
}

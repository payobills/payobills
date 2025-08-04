pub(crate) async fn cli() {
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

    crate::payobills::transaction_parser::process_transactions(nocodb_env.clone())
        .await
        .expect("Unable to parse transactions");
}

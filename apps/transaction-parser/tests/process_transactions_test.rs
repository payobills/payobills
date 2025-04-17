use tokio;
use payobills::payobills::transaction_parser::process_transactions;
#[cfg(test)]
mod tests {
    use httpmock::Method::GET;
    use payobills::payobills::transaction_parser::NocoDBEnv;

    use super::*;

    #[tokio::test]
    async fn test_process_transactions_no_transactions() {
        // Mock the HTTP client to return an empty list of transactions
        let server = httpmock::MockServer::start();

        server.mock(|when, then| {
            when.method(GET);
            then.status(200)
                .header("content-type", "application/json")
                .body(r#"{"list": [], "pageInfo": {"totalRows": 0, "page": 1, "pageSize": 1000, "isFirstPage": true, "isLastPage": true}}"#.to_string());
        });

        // Create a mock NocoDBEnv with the mock server's URL
        let nocodb_env = NocoDBEnv {
            base_url: server.base_url(),
            api_key: "mock-api-key".to_string(),
            base_name_currencies: "mock-currencies".to_string(),
            base_name_payobills: "payobills".to_string(),
            table_name_currencies_historical: "mock-historical".to_string(),
            table_name_currencies_currencies: "mock-currencies-table".to_string(),
            table_name_payobills_transactions: "transactions".to_string(),
        };

        // Call the process_transactions function and ensure no errors are thrown
        let result = process_transactions(nocodb_env).await;

        // Assert that the function executed successfully
        assert!(result.is_ok());
    }
}

use notion::ids::{BlockId, DatabaseId, PageId};
use notion::models::block::{Block, CreateBlock};
use notion::models::paging::Paging;
use notion::models::search::{
    DatabaseQuery, FilterCondition, FilterProperty, FilterValue, NotionSearch, PropertyCondition,
    SelectCondition,
};
use notion::models::{Database, ListResponse, Object, Page};
use notion::NotionApi;
use reqwest::header::{HeaderMap, HeaderValue};
use serde_json::json;
use serde_derive::{Deserialize, Serialize};
use serde::{Serialize as SerializeImpl, Serializer};
use std::collections::HashMap;
use std::str::FromStr;
use std::{process, result};
use tokio;

#[derive(Serialize, Deserialize)]
struct PageInfo {
    #[serde(rename = "totalRows")]
    total_rows: i32,
    page: i32,
    #[serde(rename = "pageSize")]
    page_size: i32,
    #[serde(rename = "isFirstPage")]
    is_first_page: bool,
    #[serde(rename = "isLastPage")]
    is_last_page: bool,
}

#[derive(Serialize, Deserialize)]
struct NocoBDResponse {
    #[serde(rename = "pageInfo")]
    page_info: PageInfo,
}

async fn write_transaction_to_nocodb(
    token: &str,
    transaction: String,
    source_system_id: String,
    back_date_string: String,
    bill_id: i32,
) -> Result<(), Box<dyn std::error::Error>> {
    let mut map = HeaderMap::new();
    map.append("xc-token", HeaderValue::from_str(token)?);

    let client = reqwest::Client::new();
    let bill_id = bill_id;

    let body = json!({
        "BackDateString": back_date_string,
        "SourceSystemID": source_system_id,
        "TransactionText": transaction,
        "bills": { "Id": bill_id }
    });

    let nocodb_transaction_table_url = std::env::var("NOCODB_TRANSACTION_TABLE_URL")
        .expect("NOCODB_TRANSACTION_TABLE_URL must be set");

    let existing_transaction_url = format!(
        "{}?w=(SourceSystemID,eq,{})",
        nocodb_transaction_table_url.clone(),
        source_system_id
    );
    let existing_transaction_response = client
        .get(existing_transaction_url)
        .headers(map.clone())
        .json(&body)
        .send()
        .await?
        .json::<NocoBDResponse>()
        .await?;

        if existing_transaction_response.page_info.total_rows != 0 {
            return Ok(())
        }

    let resp = client
        .post(nocodb_transaction_table_url)
        .headers(map)
        .json(&body)
        .send()
        .await?;

    println!("{resp:#?}");

    Ok(())
}

async fn update_sync_result_in_notion(
    notion_token: &str,
    page_id: String,
) -> Result<(), Box<dyn std::error::Error>> {
    let mut map = HeaderMap::new();
    map.insert(
        "Authorization",
        HeaderValue::from_str(&format!("Bearer {0}", notion_token))?,
    );

    map.insert("Notion-Version", "2022-06-28".parse().unwrap());

    let client = reqwest::Client::new();

    let body = json!({
        "properties": {
            "SyncResult": { "select": { "name": "SUCCESS" } }
        }
    });

    let resp = client
        .patch(format!("https://api.notion.com/v1/pages/{0}", page_id))
        .headers(map)
        .json(&body)
        .send()
        .await?;

    println!("{resp:#?}");

    Ok(())
}

async fn search_database_items(
    notion_api: NotionApi,
    db_id: String,
    notion_token: &str,
    nocodb_integration_token: &str,
) {
    let db_id: DatabaseId = DatabaseId::from_str(&db_id).expect("Can't parse to DatabaseId");

    let db_query = DatabaseQuery {
        sorts: None,
        filter: Some(FilterCondition::Property {
            property: "SyncResult".to_string(),
            condition: PropertyCondition::Select(SelectCondition::IsEmpty),
        }),
        paging: Some(Paging {
            start_cursor: None,
            page_size: Some(u8::from_str_radix("1", 10).expect("Invalid u8")),
        }),
    };

    let mut bill_id = 0;

    match notion_api.get_database(db_id.clone()).await {
        Ok(Database { title, .. }) => {
            let bill_id_str_db_title = title
                .iter()
                .map(|rich_text| rich_text.plain_text())
                .collect::<Vec<&str>>()
                .join(" ");
            let bill_id_string = bill_id_str_db_title.split(".").collect::<Vec<&str>>()[0];

            bill_id = bill_id_string
                .parse::<i32>()
                .expect("Unable to convert NOCODB_BILL_ID to a number");

            println!("Bill ID: {}", bill_id);
        }
        Err(e) => eprintln!("Error reading page: {:?}", e),
    }

    loop {
        match notion_api.query_database(db_id.clone(), db_query.clone()).await {
            Ok(ListResponse::<Page> {
                results: response, ..
            }) => {
                if response.len() == 0 {
                    println!("No transaction found");
                    process::exit(0)
                }

                for page in response {
                    println!("_____________");
                    let page_title = page.title().expect("cannot get page title");
                    println!("{}", page_title);
                    let block_id = BlockId::from(page.id.clone());

                    let blocks = notion_api.get_block_children(block_id).await;
                    match blocks {
                        Ok(ListResponse::<Block> { results, .. }) => {
                            match results.into_iter().nth(0) {
                                None => {}
                                Some(Block::Paragraph { paragraph, .. }) => {
                                    let content = paragraph
                                        .rich_text
                                        .iter()
                                        .map(|rich_text| rich_text.plain_text())
                                        .collect::<Vec<&str>>()
                                        .join(" ");

                                    // println!("{:?}", content);

                                    let page_id = PageId::from(page.id.clone());

                                    write_transaction_to_nocodb(
                                        nocodb_integration_token,
                                        content,
                                        page_id.clone().to_string(),
                                        page_title.clone().to_string(),
                                        bill_id,
                                    )
                                    .await
                                    .expect("Unable to write to NocoDB");

                                    update_sync_result_in_notion(
                                        notion_token,
                                        page_id.clone().to_string(),
                                    )
                                    .await
                                    .expect("Unable to update Notion with SyncResult");
                                }
                                _ => todo!(),
                            }
                        }
                        Err(e) => {
                            eprintln!("Error reading page: {:?}", e);
                        }
                    }
                }
            }
            Err(e) => {
                eprintln!("Error searching pages: {:?}", e);
            }
        }
    }
}

async fn _search_databases(notion_api: NotionApi) {
    // Call the search method to list all databases
    let search_request = NotionSearch::Filter {
        property: FilterProperty::Object,
        value: FilterValue::Database,
    };

    match notion_api.search(search_request).await {
        Ok(ListResponse::<Object> {
            results: response, ..
        }) => {
            println!("Finding Notion Databases");
            for obj in response {
                match obj {
                    Object::Database { database } => {
                        let joined_plain_texts: String = database
                            .title
                            .iter()
                            .map(|rich_text| rich_text.plain_text())
                            .collect::<Vec<&str>>()
                            .join(" ");

                        println!("{}", joined_plain_texts);
                    }
                    _ => {
                        println!("The object is not a Database");
                    }
                }
            }
        }
        Err(e) => {
            eprintln!("Error searching databases: {:?}", e);
        }
    }
}

#[tokio::main]
async fn main() {
    // Create a new NotionApi instance with your API key
    let notion_token =
        std::env::var("NOTION_INTEGRATION_TOKEN").expect("NOTION_INTEGRATION_TOKEN must be set");

    let db_id = std::env::var("NOTION_DB_ID").expect("NOTION_DB_ID must be set");

    let nocodb_integration_token =
        std::env::var("NOCODB_INTEGRATION_TOKEN").expect("NOCODB_INTEGRATION_TOKEN must be set");

    match NotionApi::new(notion_token.clone()) {
        Ok(notion_api) => {
            // search_databases(notion_api).await
            search_database_items(
                notion_api,
                db_id,
                &notion_token.clone(),
                &nocodb_integration_token,
            )
            .await
        }
        Err(e) => {
            eprintln!("Error creating NotionApi instance {:?}", e);
        }
    }
}

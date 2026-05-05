use serde_derive::Deserialize;
use std::error::Error;

#[derive(Deserialize)]
pub struct FieldDescriptor {
    pub name: String,
    #[serde(rename = "type")]
    pub field_type: String,
    pub format: Option<String>,
}

pub struct GeneratedPattern {
    pub regex: String,
    pub fields: Vec<FieldDescriptor>,
}

#[derive(Deserialize)]
struct OllamaResponse {
    response: String,
}

#[derive(Deserialize)]
struct SlmOutput {
    regex: String,
    fields: Vec<FieldDescriptor>,
}

const PROMPT_TEMPLATE: &str = r#"You are a regex generation assistant. Generate a Rust `regex` crate compatible regular expression to extract fields from a bank transaction SMS.

Bill type: {bill_type}
Transaction text: "{sample_text}"

Extract these fields as named capture groups:
- amount (numeric, may include commas)
- merchant (business name, if present)
- currency (currency symbol or code, if present)
- date (the transaction date/time string, if present)

Rules:
- Use named capture groups: (?P<name>...)
- Do NOT use lookaheads, lookbehinds, or backreferences (not supported by Rust regex crate)
- The pattern must match the sample text above
- Make the pattern general enough to match other transactions of the same format (different amounts, merchants, dates)
- Omit capture groups for fields that are not present in this format

Respond with a JSON object only:
{
  "regex": "<pattern>",
  "fields": [
    { "name": "amount", "type": "string", "format": "currency:<ISO-code, e.g. INR>" },
    { "name": "merchant", "type": "string", "format": null },
    { "name": "currency", "type": "string", "format": null },
    { "name": "date", "type": "date", "format": "<strptime format string matching the captured date>" }
  ]
}
Only include entries for fields actually captured in the regex. For type "date", format must be a strptime format string. For other types, format is null unless a known format applies (e.g. currency ISO code for amount)."#;

pub async fn generate_regex(
    base_url: &str,
    bill_type: &str,
    sample_text: &str,
) -> Result<GeneratedPattern, Box<dyn Error + Send + Sync>> {
    let prompt = PROMPT_TEMPLATE
        .replace("{bill_type}", bill_type)
        .replace("{sample_text}", sample_text);

    let body = serde_json::json!({
        "model": "qwen2.5-coder:1.5b",
        "prompt": prompt,
        "stream": false,
        "format": "json"
    });

    let mut last_err: Box<dyn Error + Send + Sync> = "SLM failed after retries".into();

    for _ in 0..2 {
        let url = format!("{}/api/generate", base_url);
        let resp = reqwest::Client::new()
            .post(&url)
            .json(&body)
            .send()
            .await?
            .json::<OllamaResponse>()
            .await?;

        match serde_json::from_str::<SlmOutput>(&resp.response) {
            Ok(output) => match regex::Regex::new(&output.regex) {
                Ok(_) => {
                    return Ok(GeneratedPattern {
                        regex: output.regex,
                        fields: output.fields,
                    });
                }
                Err(e) => {
                    last_err = format!("SLM returned invalid regex: {}", e).into();
                }
            },
            Err(e) => {
                last_err = format!("Failed to parse SLM response: {}", e).into();
            }
        }
    }

    Err(last_err)
}

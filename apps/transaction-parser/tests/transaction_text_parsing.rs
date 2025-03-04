use regex::Regex;
// use std::collections::HashMap;

#[cfg(test)]
mod tests {
    use super::*;
    use test_case::test_case;

    //https://regex101.com/r/YLD29Q/1
    #[test_case("Alert: You've spent INR 410.58 on your AMEX card ** 81009 at MERCHANT 1 on 3 March, 2025 at 04:44 PM IST. Call 18004190691 if this was not made by you.", "INR", "410.58", "81009", "MERCHANT 1", "3 March, 2025 at 04:44 PM", "IST"; "INR")]
    #[test_case("Alert: You've spent €60.00 on your AMEX card ** 81009 at MERCHANT 2 on 13 February, 2025 at 04:35 PM IST. Call 18004190691 if this was not made by you.", "€", "60.00", "81009", "MERCHANT 2", "13 February, 2025 at 04:35 PM", "IST"; "EUR")]
    #[test_case("Alert: You've spent €60.00 on your AMEX card ** 81009 on 13 February, 2025 at 04:35 PM IST. Call 18004190691 if this was not made by you.", "€", "60.00", "81009", "", "13 February, 2025 at 04:35 PM", "IST"; "EUR without merchant")]
    // #[test_case("when both operands are positive")]
    // #[test_case("when operands are swapped")]
    fn test_transaction_text_parsing(test_transaction_text: &str,
        currency: &str,
        amount: &str,
        card: &str,
        merchant: &str,
        date: &str,
        timezone: &str
        ) {
        let regex_pattern = r"^Alert: You've spent (?P<currency>[^\d\s]+)\s{0,1}(?P<amount>\d+\.?\d+) on your AMEX card \*\* (?P<card>\d+)( at ){0,1}(?P<merchant>[a-zA-Z0-9\s]*) on (?P<date>[\d]{1,2} \w+, [\d]{4} at \d{2,2}:\d{2,2} [A-Z]{2,2}) (?P<timezone>[A-Z]{2,3})\..*$";
        let re = Regex::new(regex_pattern).unwrap();
        let caps = re.captures(test_transaction_text).unwrap();

        // let mut expected_matches = HashMap::<usize, String>::new();
        // expected_matches.insert(0, test_transaction_text.to_string());
        // expected_matches.insert(1, currency.to_string());
        // expected_matches.insert(2, amount.to_string());
        // expected_matches.insert(3, card.to_string());
        // expected_matches.insert(4, merchant.to_string());
        // expected_matches.insert(5, date.to_string());
        // expected_matches.insert(6, timezone.to_string());

        assert_eq!(currency.to_string(), caps.name("currency").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(amount.to_string(), caps.name("amount").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(card.to_string(), caps.name("card").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(merchant.to_string(), caps.name("merchant").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(date.to_string(), caps.name("date").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(timezone.to_string(), caps.name("timezone").expect("CAPTURE TO BE PRESENT").as_str());
        // assert_eq!(expected_matches, match_to_dict(caps));

    }

    // fn match_to_dict(caps: regex::Captures) -> HashMap<usize, String> {
    //     let mut dict = HashMap::new();
    //     for (i, name) in caps.iter().enumerate() {
    //         dict.insert(i, format!("{}", name.expect("REASON").as_str()));
    //     }
    //     dict
    // }
}

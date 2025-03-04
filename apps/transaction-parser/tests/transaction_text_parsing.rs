use regex::Regex;
use std::collections::HashMap;

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_transaction_text_parsing() {
        let regex_pattern = r"^Alert: You've spent (?P<currency>\w+) (?P<amount>\d+\.?\d+) on your AMEX card \*\* (?P<card>\d+) at (?P<merchant>[a-zA-Z0-9\s]*) on (?P<date>[\d]{1,2} \w+, [\d]{4} at \d{2,2}:\d{2,2} [A-Z]{2,2}) (?P<timezone>[A-Z]{2,3})\..*$";
        let re = Regex::new(regex_pattern).unwrap();

        // Test 1: Alert transaction
        let test_transaction_text = "Alert: You've spent INR 410.58 on your AMEX card ** 81009 at MERCHANT 1 on 3 March, 2025 at 04:44 PM IST. Call 18004190691 if this was not made by you.";

        let caps = re.captures(test_transaction_text).unwrap();
        assert_eq!(7, caps.len());

        let mut expected_matches = HashMap::<usize, String>::new();
        expected_matches.insert(0, test_transaction_text.to_string());
        expected_matches.insert(1, "INR".to_string());
        expected_matches.insert(2, "410.58".to_string());
        expected_matches.insert(3, "81009".to_string());
        expected_matches.insert(4, "MERCHANT 1".to_string());
        expected_matches.insert(5, "3 March, 2025 at 04:44 PM".to_string());
        expected_matches.insert(6, "IST".to_string());

        assert_eq!("INR".to_string(), caps.name("currency").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!("410.58".to_string(), caps.name("amount").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!("81009".to_string(), caps.name("card").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!("MERCHANT 1".to_string(), caps.name("merchant").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!("3 March, 2025 at 04:44 PM".to_string(), caps.name("date").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!("IST".to_string(), caps.name("timezone").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(expected_matches, match_to_dict(caps));

    }

    fn match_to_dict(caps: regex::Captures) -> HashMap<usize, String> {
        let mut dict = HashMap::new();
        for (i, name) in caps.iter().enumerate() {
            dict.insert(i, format!("{}", name.expect("REASON").as_str()));
        }
        dict
    }
}

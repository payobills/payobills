use regex::Regex;

#[cfg(test)]
mod tests {
    use super::*;
    use test_case::test_case;

    //https://regex101.com/r/YLD29Q/2
    #[test_case("Alert: You've spent INR 410.58 on your AMEX card ** 81009 at MERCHANT 1 on 3 March, 2025 at 04:44 PM IST. Call 18004190691 if this was not made by you.", "INR", "410.58", "81009", "MERCHANT 1", "3 March, 2025 at 04:44 PM", "IST"; "INR")]
    #[test_case("Alert: You've spent €60.00 on your AMEX card ** 81009 at MERCHANT 2 on 13 February, 2025 at 04:35 PM IST. Call 18004190691 if this was not made by you.", "€", "60.00", "81009", "MERCHANT 2", "13 February, 2025 at 04:35 PM", "IST"; "EUR")]
    #[test_case("Alert: You've spent €60.00 on your AMEX card ** 81009 on 13 February, 2025 at 04:35 PM IST. Call 18004190691 if this was not made by you.", "€", "60.00", "81009", "", "13 February, 2025 at 04:35 PM", "IST"; "EUR without merchant")]
    #[test_case("Alert: You've spent €12.00 on your AMEX card ** 81009 at SNCF-VOYAGEU on 5 February, 2025 at 06:11 PM IST. Call 18004190691 if this was not made by you.", "€", "12.00", "81009", "SNCF-VOYAGEU", "5 February, 2025 at 06:11 PM", "IST"; "EUR merchant with dash")]
    #[test_case("Alert: You've spent INR 1,00,000.00 on your AMEX card ** 81009 at MERCHANT 1 on 3 March, 2025 at 04:44 PM IST. Call 18004190691 if this was not made by you.", "INR", "1,00,000.00", "81009", "MERCHANT 1", "3 March, 2025 at 04:44 PM", "IST"; "Large amount")] 
    fn test_transaction_text_parsing(test_transaction_text: &str,
        currency: &str,
        amount: &str,
        card: &str,
        merchant: &str,
        date: &str,
        timezone: &str
        ) {
        let regex_pattern = r"^Alert: You've spent (?P<currency>[^\d\s]+)\s{0,1}(?P<amount>[\d,]+\.?\d+) on your AMEX card \*\* (?P<card>\d+)( at ){0,1}(?P<merchant>.*) on (?P<date>[\d]{1,2} \w+, [\d]{4} at \d{2,2}:\d{2,2} [A-Z]{2,2}) (?P<timezone>[A-Z]{2,3})\..*$";
        let re = Regex::new(regex_pattern).unwrap();
        let caps = re.captures(test_transaction_text).unwrap();

        assert_eq!(currency.to_string(), caps.name("currency").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(amount.to_string(), caps.name("amount").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(card.to_string(), caps.name("card").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(merchant.to_string(), caps.name("merchant").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(date.to_string(), caps.name("date").expect("CAPTURE TO BE PRESENT").as_str());
        assert_eq!(timezone.to_string(), caps.name("timezone").expect("CAPTURE TO BE PRESENT").as_str());

    }
}

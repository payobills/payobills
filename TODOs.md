# TODOs

## Enhancements

- [ ] **Default currency from user preferences** — `INR` is currently hardcoded in `transaction_parser.rs` as the normalization target. This should come from user preferences instead.

- [ ] **Handle in-progress day rates** — When a transaction's date is today (or a day that hasn't finished), the exchange rates from openexchangerates.org may not be final yet. Use a temporary/cached store for rates in this case rather than persisting them to NocoDB as a permanent record, so they can be refreshed later once the day closes.

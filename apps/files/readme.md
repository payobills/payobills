# payobills.files
Service to store files - uses MinIO as storage

## Events

Rabbit MQ events are published from this app.

### 1. When a new file is uploaded

Example payload published to the queue `payobills.files.uploaded`

```json
{
    "type": "payobills.files.uploaded",
    "args": {
        "id": "58",
        "correlationId": "28",
        "type": "BILL_STATEMENT"
    }
}
```
# RiaMoneyTransferAPI-Project
This is the repository for the API project as well a place to store the console application that can request the calls to the API

## Order tracking (public)

Mirrors the public Ria tracking flow (session JWT → tracking status):

| Method | Route | Description |
|---|---|---|
| GET | `/api/Order/track/{orderNumber}` | Track a transfer by order number / PIN |
| POST | `/api/Order/track` | Track with a full body (`OrderNumber`, `GetEventDetails`, `GetEta`) |

By default the service obtains an anonymous public session token. Ria's
endpoint rejects anonymous tokens for most order lookups (HTTP 403), so to
track an order that requires authorization, forward your own authenticated
Ria token via the `X-Ria-Auth-Token` header (with or without a `Bearer`
prefix); it is used in place of the anonymous token.

## Partner / Wholesale API (rialink.net)

Proxies the Ria Partner API. Requests carry the `ria-AgentId` and
`Ocp-Apim-Subscription-Key` headers, read from configuration.

| Method | Route | Upstream |
|---|---|---|
| POST | `/api/PartnerOrder/cash-pickup` | `POST /PayOrders/Order/CashPickUp` |
| POST | `/api/PartnerOrder/release` | `POST /SendOrders/Order/Release` |
| GET | `/api/PartnerOrder/sc/{scOrderNo}/status` | `GET /SendOrders/Orders/SCOrderNo/{id}/Status` |
| GET | `/api/PartnerOrder/pc/{pcOrderNo}/status` | `GET /SendOrders/Orders/PCOrderNo/{id}/Status` |
| GET | `/api/PartnerOrder/sc/{scOrderNo}/staged` | `GET /SendOrders/Orders/SCOrderNo/{id}?StagedOrdersOnly` |

### Configuration

Set partner credentials outside source control (user-secrets, environment
variables, or a secret store). The `appsettings.json` keys are placeholders:

```json
"RiaPartnerApi": {
  "BaseUrl": "https://stagingapi.rialink.net",
  "AgentId": "",
  "SubscriptionKey": ""
}
```

Environment-variable form (ASP.NET Core double-underscore convention):

```
RiaPartnerApi__AgentId=<agent-id>
RiaPartnerApi__SubscriptionKey=<subscription-key>
```

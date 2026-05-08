# Northwind Order Management System - API Documentation

## Base URL
```
http://localhost:5096/api
```

## Authentication
Currently no authentication required (can be added with JWT tokens).

## Common Response Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Successful deletion |
| 400 | Bad Request - Invalid input data |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error - Server error |

## Orders Endpoints

### 1. Get All Orders

```http
GET /orders
```


**Response (200)**
```json
[
  {
    "orderID": 10248,
    "customerID": "VINET",
    "customerName": "Vins et alcools Chevalier",
    "employeeID": 5,
    "employeeName": "Steven Buchanan",
    "orderDate": "1996-07-04",
    "requiredDate": "1996-08-01",
    "shippedDate": "1996-07-16",
    "shipVia": 3,
    "shipperName": "Federal Shipping",
    "freight": 32.38,
    "shipName": "Vins et alcools Chevalier",
    "shipAddress": "59 rue de l'Abbaye",
    "shipCity": "Reims",
    "shipRegion": "CJ",
    "shipPostalCode": "51100",
    "shipCountry": "France",
    "orderDetails": [
      {
        "orderDetailID": 1,
        "productID": 11,
        "productName": "Queso Cabrales",
        "quantity": 12,
        "unitPrice": 14.00,
        "discount": 0.0,
        "lineTotal": 168.00
      }
    ],
    "totalAmount": 168.00,
    "status": "Delivered"
  }
]
```

### 2. Get Order by ID

```http
GET /orders/{id}
```

**Parameters**
- `id` (integer, required) - Order ID

**Example**
```http
GET /orders/10248
```

**Response (200)**
```json
{
  "orderID": 10248,
  "customerID": "VINET",
  "customerName": "Vins et alcools Chevalier",
  "totalAmount": 168.00,
  "status": "Delivered",
  "orderDetails": [ /* ... */ ]
}
```

**Response (404)**
```json
{
  "error": "Order 99999 not found"
}
```

### 3. Create Order

```http
POST /orders
Content-Type: application/json
```

**Request Body**
```json
{
  "customerID": "ALFKI",
  "employeeID": 1,
  "orderDate": "2026-04-29",
  "requiredDate": "2026-05-29",
  "shippedDate": null,
  "shipVia": 1,
  "freight": 32.38,
  "shipName": "Alfreds Futterkiste",
  "shipAddress": "Obere Str. 57",
  "shipCity": "Berlin",
  "shipRegion": "Berlin",
  "shipPostalCode": "12209",
  "shipCountry": "Germany",
  "orderDetails": [
    {
      "productID": 1,
      "quantity": 10,
      "unitPrice": 18.00,
      "discount": 0.0
    },
    {
      "productID": 2,
      "quantity": 5,
      "unitPrice": 19.00,
      "discount": 0.1
    }
  ]
}
```

**Response (201)**
```json
{
  "orderID": 11078,
  "customerID": "ALFKI",
  "customerName": "Alfreds Futterkiste",
  "employeeID": 1,
  "orderDate": "2026-04-29",
  "totalAmount": 265.50,
  "orderDetails": [ /* ... */ ]
}
```

**Response (400)**
```json
{
  "error": "Product 999 not found"
}
```

### 4. Update Order

```http
PUT /orders/{id}
Content-Type: application/json
```

**Parameters**
- `id` (integer, required) - Order ID

**Request Body**
```json
{
  "shipName": "Updated Ship Name",
  "shipAddress": "New Address 123",
  "shipCity": "New City",
  "shipRegion": "New Region",
  "shipPostalCode": "54321",
  "shipCountry": "New Country"
}
```

**Response (200)**
```json
{
  "orderID": 10248,
  "shipName": "Updated Ship Name",
  "shipCity": "New City",
  "totalAmount": 168.00
}
```

**Response (404)**
```json
{
  "error": "Order 99999 not found"
}
```

### 5. Delete Order

```http
DELETE /orders/{id}
```

**Parameters**
- `id` (integer, required) - Order ID

**Example**
```http
DELETE /orders/10248
```

**Response (204)**
```
No content
```

**Response (404)**
```json
{
  "error": "Order 99999 not found"
}
```

### 6. Get Orders by Customer

```http
GET /orders/customer/{customerId}
```

**Parameters**
- `customerId` (string, required) - Customer ID (e.g., "ALFKI")

**Example**
```http
GET /orders/customer/ALFKI
```

**Response (200)**
```json
[
  {
    "orderID": 10835,
    "customerID": "ALFKI",
    "customerName": "Alfreds Futterkiste",
    "totalAmount": 69.53
  },
  {
    "orderID": 10952,
    "customerID": "ALFKI",
    "customerName": "Alfreds Futterkiste",
    "totalAmount": 61.02
  }
]
```

### 7. Get Orders by Date Range

```http
GET /orders/date-range?startDate={startDate}&endDate={endDate}
```

**Parameters**
- `startDate` (date, required) - Start date (format: YYYY-MM-DD)
- `endDate` (date, required) - End date (format: YYYY-MM-DD)

**Example**
```http
GET /orders/date-range?startDate=2026-04-01&endDate=2026-04-30
```

**Response (200)**
```json
[
  {
    "orderID": 10248,
    "orderDate": "2026-04-15",
    "customerName": "Vins et alcools Chevalier",
    "totalAmount": 168.00
  },
  {
    "orderID": 10249,
    "orderDate": "2026-04-18",
    "customerName": "Toms Spezialkost",
    "totalAmount": 1207.40
  }
]
```

## Address Endpoints

### 1. Validate Address

```http
POST /addresses/validate
Content-Type: application/json
```

**Request Body**
```json
{
  "address": "1600 Amphitheatre Parkway",
  "city": "Mountain View",
  "state": "CA",
  "postalCode": "94043",
  "country": "USA"
}
```

**Response (200)**
```json
{
  "isValid": true,
  "normalizedAddress": "1600 Amphitheater Parkway, Mountain View, CA 94043, USA",
  "latitude": 37.4224764,
  "longitude": -122.0842499,
  "components": {
    "streetNumber": "1600",
    "streetName": "Amphitheater Parkway",
    "city": "Mountain View",
    "state": "CA",
    "postalCode": "94043",
    "country": "United States"
  }
}
```

**Response (400)**
```json
{
  "isValid": false,
  "error": "Address could not be validated. Please check the address details."
}
```

## Lookups Endpoints

### 1. Get Countries

```http
GET /lookups/countries
```

**Response (200)**
```json
[
  {
    "id": "US",
    "name": "United States"
  },
  {
    "id": "CA",
    "name": "Canada"
  },
  {
    "id": "FR",
    "name": "France"
  }
]
```

### 2. Get Shippers

```http
GET /lookups/shippers
```

**Response (200)**
```json
[
  {
    "shipperID": 1,
    "companyName": "Speedy Express",
    "phone": "(503) 555-9831"
  },
  {
    "shipperID": 2,
    "companyName": "United Package",
    "phone": "(503) 555-3199"
  },
  {
    "shipperID": 3,
    "companyName": "Federal Shipping",
    "phone": "(503) 555-9931"
  }
]
```

### 3. Get Employees

```http
GET /lookups/employees
```

**Response (200)**
```json
[
  {
    "employeeID": 1,
    "firstName": "Nancy",
    "lastName": "Davolio",
    "title": "Sales Representative"
  },
  {
    "employeeID": 2,
    "firstName": "Andrew",
    "lastName": "Fuller",
    "title": "Vice President, Sales"
  }
]
```

## Error Handling

### Common Error Responses

**Validation Error (400)**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "0HN1GBGCH4QCH:00000001",
  "errors": {
    "CustomerID": ["The CustomerID field is required."],
    "ShipCity": ["The ShipCity field is required."]
  }
}
```

**Not Found (404)**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Order 99999 not found"
}
```

**Server Error (500)**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "traceId": "0HN1GBGCH4QCH:00000001"
}
```

## Rate Limiting

No rate limiting is currently implemented. For production deployments, consider implementing:
- Request throttling
- IP-based limiting
- API key quotas

## Pagination (Future Enhancement)

Planned pagination parameters:
```http
GET /orders?pageNumber=1&pageSize=20
```

## Sorting (Future Enhancement)

Planned sorting parameters:
```http
GET /orders?sortBy=orderDate&sortOrder=desc
```

## Filtering (Future Enhancement)

Planned filtering parameters:
```http
GET /orders?customerID=ALFKI&minAmount=100&maxAmount=500
```


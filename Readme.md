Invoice System API
--------------------------------------------------
The Invoice System API is a RESTful service built with .NET that manages the lifecycle of invoices. The system supports invoice creation, payment processing, cancellation, overdue handling, and invoice history tracking. It is designed with a modular architecture so that persistence mechanisms can be easily replaced or extended in the future.

The API also includes features such as search, pagination, invoice chaining for overdue processing, and soft deletion.

--------------------------------------------------

Assumptions
--------------------------------------------------
The following assumptions were made during the implementation:

An invoice can be created with a due date earlier than the current date. In real-world systems this would normally be restricted, but it is allowed here for flexibility and testing purposes.

The system exposes only three invoice statuses in the API responses:

paid

pending

void

Internally the system may track additional states (e.g., partially paid), but they are exposed as pending in API responses.

When an invoice becomes overdue, a new invoice is generated:

The new invoice amount is the remaining amount + late fee

The new invoice due date is current date + overdue_days

overdue_days must be greater than 0.

late_fee represents the penalty amount applied when an invoice becomes overdue.

Cancelling an invoice sets its status to void.

GET /invoices returns all invoices except soft-deleted ones.

Invoice deletion uses soft delete, meaning records are retained in the database but excluded from queries.

Core API Features
--------------------------------------------------

1. Create Invoice

Creates a new invoice with an amount and due date.

POST /invoices

--------------------------------------------------
2. Get All Invoices

Retrieves all invoices in the system (excluding deleted invoices).

GET /invoices

--------------------------------------------------
3. Add Payment

Applies a payment to a specific invoice.

POST /{id}/payments

If the invoice becomes fully paid, its status is updated to paid.

--------------------------------------------------
4. Process Overdue Invoices

Processes all overdue invoices and generates new invoices when required.

POST /invoices/process-overdue

Behavior:

Unpaid invoices

Original invoice → marked as void

New invoice → created with amount + late_fee

Partially paid invoices

Original invoice → marked as paid

New invoice → created with remaining_amount + late_fee

Additional Features
--------------------------------------------------

1. Cancel Invoice

Cancels an invoice by updating its status to void.

POST /{id}/cancel

--------------------------------------------------
2. Search Invoices

Allows filtering invoices by status and due date range, with pagination support.

GET /search

Supports:

status filtering

date range filtering

pagination (page, pageSize)

--------------------------------------------------
3. Invoice History

Returns the full history of invoices belonging to the same invoice chain.

This is useful when invoices are regenerated during overdue processing.

GET /{id}/history

--------------------------------------------------
4. Delete Invoice

Deletes an invoice using soft deletion.

DELETE /{id}

Deleted invoices are not returned by API queries.

--------------------------------------------------
5. Get Root Invoice

Retrieves the original invoice in an invoice chain.

GET /{id}/root

--------------------------------------------------
6. Get Latest Active Invoice

Returns the most recent active invoice in an invoice chain.

GET /{id}/current

--------------------------------------------------
7. Add Payment to Latest Invoice

Applies a payment to the latest active invoice, even if the request references an older invoice in the chain.

POST /{id}/payments_new

--------------------------------------------------
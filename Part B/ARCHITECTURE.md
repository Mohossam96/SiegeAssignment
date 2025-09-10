# Architecture Design: Document Metadata & Search System

## 1. Introduction & Goals

This document outlines the architecture for a system designed to ingest, process, version, and provide fast search capabilities for Chemical Safety Data Sheets (SDS) provided as PDF documents.

The primary goals of this system are:

- **Reliable Ingestion:** To provide a fast and reliable mechanism for uploading large PDF documents.
- **Automated Processing:** To automatically extract key metadata from documents using an asynchronous pipeline.
- **High-Performance Search:** To deliver a fast and flexible search experience over the extracted metadata, with strict multi-tenancy.
- **Scalability & Resilience:** To build a system that can scale to handle a growing volume of documents and tenants, and is resilient to component failures.

---

## 2. Requirements Summary

### Functional Requirements

- **F1:** Ingest PDF documents up to 25MB.  
- **F2:** Version documents by a composite key (tenantId, supplier, sku, docType, version/batch).  
- **F3:** Extract and store JSON metadata from PDFs (assuming an external OCR/ML service exists).  
- **F4:** Search for documents by metadata fields (sku, supplier, docType, date ranges).  
- **F5:** Support a "latest-version only" search query.  
- **F6:** Enforce multi-tenancy to ensure data isolation.  
- **F7:** Enforce simple Role-Based Access Control (RBAC).  
- **F8:** Implement a retention policy to keep the last N versions and archive older ones.  

### Non-Functional Requirements (SLAs)

- **NF1:** Ingestion acknowledgment (P95) ≤ 5 seconds.  
- **NF2:** Search query response time (P95) ≤ 200 milliseconds.  
- **NF3:** Search API availability ≥ 99.9%.  

---

## 3. High-Level Architecture

The proposed architecture is an **event-driven, decoupled system** built on microservices principles. This design excels at handling long-running background tasks (like OCR) without blocking the user, while ensuring scalability and resilience.

The system is composed of five major logical areas:

1. **Ingestion Layer:** The public-facing API responsible for handling upload requests.  
2. **Messaging Backbone:** A message queue that acts as a buffer and communication layer between services.  
3. **Data Persistence Layer:** A polyglot persistence model using three specialized data stores for PDFs, metadata, and search indexing.  
4. **Asynchronous Processing Pipeline:** A set of background workers that perform document processing tasks.  
5. **Query Layer:** The public-facing API responsible for handling search requests.  

**Architecture Diagram (C4 - Level 2):** will be provided separately.

---

## 4. Data Flow Deep Dive

### 4.1 Ingestion Flow (Event-Driven)

The ingestion process is asynchronous to meet the < 5-second P95 SLA.

1. Client requests a presigned upload URL from the Ingestion API, providing initial metadata (tenantId, sku, supplier, docType).  
2. The Ingestion API creates a preliminary record in the RDBMS with a `PENDING` status and a new version number. It generates a short-lived, secure presigned URL for direct upload to Object Storage.  
3. The client uploads the PDF file directly to Object Storage, bypassing the API server. This offloads the heavy lifting from our service, making it highly scalable.  
4. Upon successful upload, a trigger on the storage bucket (or a confirmation call from the client to the API) publishes an **`sds.uploaded`** event to the Message Queue. The event message contains the path to the PDF and the document's ID.  
5. An Extractor Worker consumes the message. It downloads the PDF and calls the external OCR/ML Recognizer service.  
6. Once metadata is extracted, the worker publishes an **`sds.extracted`** event containing the document ID and the metadata JSON.  
7. A Metadata Writer Worker consumes this event. It updates the document record in the RDBMS to `PROCESSED` and saves the metadata. It also handles the retention policy logic (archiving older versions). It then publishes a final **`sds.processed`** event.  
8. An Indexer Worker consumes the **`sds.processed`** event and writes/updates the document's metadata in the Search Index to make it searchable.  

**Idempotency & Resilience:**  
- The outbox pattern is used by workers writing to the database to ensure messages are published reliably.  
- Workers are designed to be idempotent.  
- A Dead-Letter Queue (DLQ) captures messages that fail repeatedly for manual inspection.  

### 4.2 Search Flow

The search flow is optimized for speed to meet the < 200ms P95 SLA.

1. The client sends a `GET` request to the Search API with query parameters.  
2. The API's authorization middleware validates the user's JWT, extracting their tenantId and role.  
3. The API constructs a query for the Search Index. Crucially, this query always includes a non-negotiable filter for the user's tenantId to ensure data isolation.  
4. The Search Index executes the query and returns a list of document IDs and metadata.  
5. The Search API formats the response and returns it to the client. For document downloads, it can generate a secure, short-lived presigned URL for the PDF in Object Storage.  

---

## 5. Key Design Decisions & Trade-offs

| Decision | Rationale & Trade-offs |
|----------|-------------------------|
| **Event-Driven Architecture** | Pro: Decouples components, allows independent scaling, provides resilience through retries/DLQs, and enables asynchronous processing to meet the fast ingest SLA.<br/>Con: Increases operational complexity (monitoring queues, distributed tracing is essential). |
| **Polyglot Persistence** | Pro: Uses the best tool for each job: Object Storage for cheap/durable file storage, RDBMS for transactional integrity (source of truth), and a Search Index for sub-200ms query performance.<br/>Con: Introduces data consistency challenges. The Search Index is eventually consistent with the RDBMS, which is an acceptable trade-off for this use case. |
| **Presigned URLs for Uploads** | Pro: Massively improves scalability and reduces load/cost on the API servers by offloading the heavy file transfer to the cloud storage provider.<br/>Con: Requires slightly more complex logic on the client-side to handle the two-step upload process. |

---

## 6. Schema Design

### RDBMS (PostgreSQL)

**`documents` table:**  
- id (PK)  
- tenant_id (FK)  
- sku  
- supplier  
- doc_type  
- version (int)  
- is_latest (bool)  
- status  
- storage_path  
- metadata (JSONB)  
- created_at  
- archived_at  

> A composite index on (tenant_id, sku, supplier, doc_type) is critical for versioning.

**Other tables:**  
- tenants  
- users  
- roles (for multi-tenancy and RBAC).  

### Search Index (Elasticsearch)

- An index mirroring the `documents` table.  
- Fields from the metadata JSON explicitly mapped and indexed for fast searching and faceting (e.g., `properties.chemical_name`, `properties.signal_word`).  
- Each document in the index must contain the tenant_id.  

---

## 7. Cost & Operability

### Cost Drivers

- Object Storage (total GBs stored).  
- Search Index (instance size and data volume).  
- Compute hours for the processing workers.  

### Cost Optimization

- The retention policy is key. Archiving older PDFs to a cheaper storage tier (e.g., Azure Archive, S3 Glacier) will significantly reduce costs.  

### Operability

- **Observability:** Structured logging with a Correlation ID is essential to trace a document through the entire pipeline.  
- **Metrics & Alerts:** Key metrics to monitor are queue depth, message processing time (end-to-end), search latency (P95), and DLQ size. Alerts should be configured for high queue depth or a growing DLQ.  
- **SLOs:** The P95 SLAs for ingest and search will be the primary Service Level Objectives (SLOs) to monitor.  

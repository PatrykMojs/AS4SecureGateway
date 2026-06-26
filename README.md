# AS4 Secure Gateway

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-100%25-239120?style=for-the-badge&logo=csharp)
![Status](https://img.shields.io/badge/status-work%20in%20progress-orange?style=for-the-badge)
![Architecture](https://img.shields.io/badge/architecture-layered-blue?style=for-the-badge)

## Overview

**AS4 Secure Gateway** is a portfolio project that simulates an AS4-style secure message gateway built with **.NET 8**.

The project focuses on preparing, securing and dispatching SOAP/XML business messages using a clean, modular architecture. It separates business logic, application use cases, infrastructure implementations, worker processing and testing into dedicated projects.

The main goal of this project is to demonstrate practical knowledge of enterprise integration patterns, secure message exchange, SOAP/XML processing, certificate-based cryptography and maintainable .NET backend architecture.

> This repository is an educational and portfolio-oriented implementation. It is not a certified production-ready AS4/eDelivery access point.

---

## Main Features

- AS4-style secure message dispatch flow
- SOAP/XML business message preparation
- Support for selected AS4-style actions:
  - `SendMessage`
  - `PeekMessage`
  - `DequeueMessage`
- Configurable security processing:
  - payload compression
  - message encryption
  - digital signature
- X.509 certificate provider abstraction
- XML body signing abstraction
- XML body encryption abstraction
- Attachment signing abstraction
- Compressed attachment encryption abstraction
- HTTP transport client abstraction
- AS4 envelope generation abstraction
- AS4 security pipeline abstraction
- Worker Service foundation
- Local test endpoint for development and integration testing
- Separate test projects for Application, Domain and Infrastructure layers

---

## Technology Stack

- **.NET 8**
- **C#**
- **.NET Worker Service**
- **ASP.NET Core test endpoint**
- **Quartz.NET**
- **Serilog**
- **XML / SOAP**
- **GZip compression**
- **X.509 certificates**
- **Digital signatures**
- **Message encryption**
- **HTTP client abstraction**
- **Layered architecture**
- **Unit and integration testing foundation**

---

## Solution Structure

```text
AS4SecureGateway/
├── src/
│   ├── AS4SecureGateway.Application/
│   │   ├── Abstractions/
│   │   ├── Messaging/
│   │   └── UseCases/
│   │
│   ├── AS4SecureGateway.Contracts/
│   │   └── Shared contracts used across the solution
│   │
│   ├── AS4SecureGateway.Domain/
│   │   └── Core domain models and business concepts
│   │
│   ├── AS4SecureGateway.Infrastructure/
│   │   ├── Certificates/
│   │   ├── Compression/
│   │   ├── Cryptography/
│   │   ├── Diagnostics/
│   │   ├── Ebms/
│   │   ├── Files/
│   │   ├── Http/
│   │   ├── Messaging/
│   │   ├── Mime/
│   │   ├── Soap/
│   │   └── Xml/
│   │
│   ├── AS4SecureGateway.TestEndpoint/
│   │   └── Local endpoint used for development and integration testing
│   │
│   └── AS4SecureGateway.WorkerService/
│       └── Background worker service host
│
├── tests/
│   ├── AS4SecureGateway.Application.Tests/
│   ├── AS4SecureGateway.Domain.Tests/
│   └── AS4SecureGateway.Infrastructure.Tests/
│
├── .gitignore
├── AS4SecureGateway.sln
└── README.md
```

---

## Architecture

The project follows a layered architecture where each layer has a separate responsibility.

| Layer | Responsibility |
|---|---|
| `Domain` | Core business models and domain concepts |
| `Application` | Use cases, application logic, messaging models and abstractions |
| `Infrastructure` | Technical implementations for certificates, compression, cryptography, HTTP, SOAP, MIME and XML processing |
| `Contracts` | Shared contracts used between projects |
| `WorkerService` | Background service host for automated message processing |
| `TestEndpoint` | Local test endpoint for development and integration scenarios |
| `Tests` | Automated tests for the main application layers |

---

## Message Dispatch Flow

The central idea of the application is to build an AS4-style message, apply security processing and send it through a transport client.

```text
Dispatch command
      |
      v
Business body creation
      |
      v
AS4 metadata generation
      |
      v
SOAP envelope creation
      |
      v
Security pipeline
      |
      v
HTTP transport client
      |
      v
Dispatch result
```

This approach keeps the application layer independent from the concrete implementation of cryptography, SOAP generation and HTTP communication.

---

## Supported AS4 Actions

The project currently defines the following AS4-style actions:

| Action | Description |
|---|---|
| `SendMessage` | Prepares and dispatches a business message |
| `PeekMessage` | Prepares a request for checking available messages |
| `DequeueMessage` | Prepares a request for downloading or removing a message from a queue |

---

## Security Processing Options

Security processing can be configured using dedicated options.

Supported processing flags:

```text
EnableCompression
EnableEncryption
EnableSignature
```

The options can be combined, for example:

```text
Compression
Encryption
Signature
Compression + Encryption
Encryption + Signature
Compression + Encryption + Signature
```

These options are used by the security pipeline to determine how the message should be prepared before transport.

---

## Infrastructure Components

The infrastructure layer provides implementations for technical operations such as:

- loading certificates
- compressing payloads
- signing XML bodies
- encrypting XML bodies
- signing attachments
- encrypting compressed attachments
- creating AS4 business bodies
- creating AS4 envelopes
- applying the AS4 security pipeline
- sending messages through HTTP

Registered infrastructure abstractions include:

```text
IAs4CertificateProvider
IPayloadCompressor
IXmlBodySigner
IXmlBodyEncryptor
IAttachmentSignatureBuilder
ICompressedAttachmentEncryptor
IAs4BusinessBodyFactory
IAs4EnvelopeFactory
IAs4SecurityPipeline
IAs4TransportClient
```

---

## Getting Started

### Prerequisites

Make sure you have installed:

- .NET SDK 8.0 or newer
- Git
- Visual Studio 2022, JetBrains Rider or Visual Studio Code

---

### Clone Repository

```bash
git clone https://github.com/PatrykMojs/AS4SecureGateway.git
cd AS4SecureGateway
git checkout develop
```

---

### Restore Dependencies

```bash
dotnet restore AS4SecureGateway.sln
```

---

### Build Solution

```bash
dotnet build AS4SecureGateway.sln
```

---

### Run Tests

```bash
dotnet test AS4SecureGateway.sln
```

---

### Run Worker Service

```bash
dotnet run --project src/AS4SecureGateway.WorkerService/AS4SecureGateway.WorkerService.csproj
```

---

### Run Test Endpoint

```bash
dotnet run --project src/AS4SecureGateway.TestEndpoint/AS4SecureGateway.TestEndpoint.csproj
```

---

## Example Configuration Direction

The project uses configuration sections for transport and certificate-related settings.

Example configuration shape:

```json
{
  "As4Transport": {
    "BaseUrl": "https://localhost:5001",
    "TimeoutSeconds": 30
  },
  "Certificates": {
    "SigningCertificatePath": "certificates/signing.pfx",
    "SigningCertificatePassword": "change-me",
    "EncryptionCertificatePath": "certificates/encryption.cer"
  }
}
```

> Do not commit real production certificates, passwords, private keys or endpoint credentials.

---

## Security Notes

This project touches areas that are sensitive in real enterprise systems. The following files and values should never be committed to the repository:

- private keys
- `.pfx` files
- `.p12` files
- certificate passwords
- production certificates
- real endpoint credentials
- connection strings with secrets
- production AS4 messages
- business XML payloads containing confidential data

For local development, use only test certificates and sample payloads.

---

## Testing

The repository contains dedicated test projects for the main layers:

```text
AS4SecureGateway.Application.Tests
AS4SecureGateway.Domain.Tests
AS4SecureGateway.Infrastructure.Tests
```

Run all tests with:

```bash
dotnet test
```

The test structure helps verify application logic, domain behavior and infrastructure components independently.

---

## Current Status

The project is currently under active development.

Implemented foundation:

- layered solution structure
- message action model
- security processing options
- application abstractions
- infrastructure dependency registration
- worker service project
- test endpoint project
- dedicated test projects

Planned improvements:

- complete end-to-end `SendMessage` scenario
- complete `PeekMessage` scenario
- complete `DequeueMessage` scenario
- add sample XML payloads
- add sample test certificates
- add integration tests against the test endpoint
- improve runtime configuration
- add Docker support
- add GitHub Actions CI pipeline
- improve error handling and retry policies
- add message persistence
- extend documentation with request and response examples

---

## Roadmap

- [ ] Complete full `SendMessage` flow
- [ ] Complete full `PeekMessage` flow
- [ ] Complete full `DequeueMessage` flow
- [ ] Add sample payload files
- [ ] Add local development certificate examples
- [ ] Add integration tests
- [ ] Add Docker configuration
- [ ] Add CI pipeline
- [ ] Add detailed architecture documentation
- [ ] Add production hardening notes

---

## Purpose of the Project

This project was created to demonstrate practical experience with:

- .NET backend development
- enterprise application architecture
- background services
- secure message exchange
- SOAP/XML processing
- AS4-style messaging concepts
- certificate-based security
- compression and encryption pipelines
- digital signatures
- dependency inversion
- testable application design

---

## Author

**Patryk Meus**

.NET Developer focused on backend systems, integrations, secure message exchange and business process automation.

---

## License

No license file is currently included in this repository.

Before using this project outside of private learning or portfolio purposes, add an appropriate license.
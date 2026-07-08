# AS4 Secure Gateway

[![.NET CI](https://github.com/PatrykMojs/AS4SecureGateway/actions/workflows/dotnet-ci.yml/badge.svg?branch=develop)](https://github.com/PatrykMojs/AS4SecureGateway/actions/workflows/dotnet-ci.yml)

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-100%25-239120?style=for-the-badge&logo=csharp)
![Status](https://img.shields.io/badge/status-in%20development-orange?style=for-the-badge)
![Architecture](https://img.shields.io/badge/architecture-layered-blue?style=for-the-badge)
![Tests](https://img.shields.io/badge/tests-unit%20%2B%20integration-green?style=for-the-badge)

---

## Overview

**AS4 Secure Gateway** is a portfolio project that simulates an AS4-style secure message gateway built with **.NET 8**.

The project focuses on preparing, securing and dispatching SOAP/XML business messages using a clean, modular architecture. It separates business logic, application use cases, infrastructure implementations, worker processing, local endpoint simulation and automated testing into dedicated projects.

The main goal of this project is to demonstrate practical knowledge of enterprise integration patterns, secure message exchange, SOAP/XML processing, certificate-based cryptography, background processing and maintainable .NET backend architecture.

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
- X.509 certificate-based security
- XML body signing abstraction
- XML body encryption abstraction
- Attachment signing abstraction
- Compressed attachment encryption abstraction
- AS4 envelope generation abstraction
- AS4 security pipeline abstraction
- HTTP transport client abstraction
- Worker Service foundation
- Quartz.NET scheduled jobs
- Serilog-based logging
- Local ASP.NET Core test endpoint
- Unit tests for Application, Domain and Infrastructure layers
- End-to-end integration test for the AS4 `SendMessage` flow
- GitHub Actions CI workflow for automated build and test validation

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
- **xUnit**
- **FluentAssertions**
- **NSubstitute**
- **GitHub Actions**
- **Layered architecture**

---

## Solution Structure

```text
AS4SecureGateway/
├── .github/
│   └── workflows/
│       └── dotnet-ci.yml
│
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
│   │   ├── Persistence/
│   │   ├── Responses/
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
│       └── Integration/
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
| `Infrastructure` | Technical implementations for certificates, compression, cryptography, HTTP, SOAP, MIME, persistence and XML processing |
| `Contracts` | Shared contracts used between projects |
| `WorkerService` | Background service host for automated message processing |
| `TestEndpoint` | Local endpoint used to simulate AS4 message reception |
| `Tests` | Automated unit and integration tests |

This separation keeps the application layer independent from concrete infrastructure details such as certificate loading, XML signing, encryption, HTTP communication and file-based diagnostics.

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
Test endpoint / external AS4 endpoint
      |
      v
AS4 response processing
      |
      v
Dispatch result
```

This approach makes the flow easier to test, extend and replace with real AS4/eDelivery infrastructure in the future.

---

## End-to-End SendMessage Scenario

The repository includes an end-to-end integration test for the `SendMessage` flow.

The test verifies the following scenario:

```text
Test client / dispatch flow
      |
      v
Create SOAP/AS4 message
      |
      v
Apply compression, encryption and digital signature
      |
      v
Send multipart AS4-style request over HTTP
      |
      v
Receive request in local TestEndpoint
      |
      v
Read multipart payload
      |
      v
Process security layer
      |
      v
Create AS4-style response
      |
      v
Parse and validate dispatch result
```

This test is intended to prove that the main message flow works across multiple layers, not only as isolated unit tests.

---

## Supported AS4 Actions

The project currently defines the following AS4-style actions:

| Action | Description | Status |
|---|---|---|
| `SendMessage` | Prepares and dispatches a business message | Implemented and covered by tests |
| `PeekMessage` | Prepares a request for checking available messages | Foundation implemented |

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
- parsing AS4/SOAP responses
- storing audit information for message processing

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
IAs4MessageAuditRepository
```

---

## Testing

The repository contains dedicated test projects for the main layers:

```text
AS4SecureGateway.Application.Tests
AS4SecureGateway.Domain.Tests
AS4SecureGateway.Infrastructure.Tests
```

The tests cover:

- AS4 message metadata creation
- security processing options
- dispatch use case behavior
- domain models
- payload and message validation
- party and collaboration information models
- GZip payload compression
- AS4 business body generation
- SOAP/AS4 envelope generation
- AS4 response parsing
- infrastructure-level message processing
- end-to-end `SendMessage` flow through the local test endpoint

Run all tests with:

```bash
dotnet test AS4SecureGateway.sln
```

The test suite verifies both isolated components and the higher-level AS4 message flow.

---

## Continuous Integration

This repository uses **GitHub Actions** to automatically validate the project.

The CI workflow runs on the `develop` branch and performs:

```text
dotnet restore
dotnet build
dotnet test
```

This helps ensure that every pushed change can be restored, built and tested in a clean GitHub-hosted environment.

Workflow file:

```text
.github/workflows/dotnet-ci.yml
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
- `.pem` files containing private keys
- certificate passwords
- production certificates
- real endpoint credentials
- connection strings with secrets
- production AS4 messages
- business XML payloads containing confidential data

For local development, use only test certificates and sample payloads.

---

## Current Status

The project is currently under active development.

Implemented:

- layered solution structure
- Domain, Application, Infrastructure, Contracts, WorkerService and TestEndpoint projects
- AS4-style message action model
- configurable security processing options
- application abstractions
- infrastructure dependency registration
- Worker Service foundation
- Quartz-based scheduled jobs
- local ASP.NET Core TestEndpoint
- unit tests for Application, Domain and Infrastructure layers
- end-to-end integration test for the `SendMessage` flow
- GitHub Actions CI workflow

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
- HTTP-based integrations
- dependency inversion
- testable application design
- automated testing
- CI validation with GitHub Actions

---

## Author

**Patryk Meus**

.NET Developer focused on backend systems, integrations, secure message exchange and business process automation.

---

## License

No license file is currently included in this repository.

Before using this project outside of private learning or portfolio purposes, add an appropriate license.
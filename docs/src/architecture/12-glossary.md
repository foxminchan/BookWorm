# 12. Glossary

## A

**Aggregate**
: In Domain-Driven Design, a cluster of domain objects that can be treated as a single unit for data changes. An aggregate has one root entity through which all modifications must be made.

**API Gateway**
: A server that acts as an entry point for clients to access backend services. It handles tasks such as request routing, authentication, rate limiting, and load balancing.

**arc42**
: A template for documentation and communication of software and system architectures. It provides a consistent structure with 12 main sections covering all important aspects of architecture documentation.

**Azure Container Apps (ACA)**
: A managed container service from Microsoft Azure that enables deployment of containerized applications without managing underlying infrastructure.

**AsyncAPI**
: A specification for describing event-driven architectures and asynchronous APIs, similar to OpenAPI but for message-based communication.

## B

**Basket Service**
: A microservice responsible for managing shopping carts and wishlists in the BookWorm e-commerce platform.

**Building Block**
: In architecture documentation, a component or module that encapsulates a specific functionality and can be combined with other building blocks to create larger systems.

**Business Logic**
: The part of a software application that encodes the real-world business rules and workflows that determine how data can be created, stored, and changed.

## C

**Catalog Service**
: A microservice that manages the book inventory, including books, authors, categories, and related metadata in the BookWorm platform.

**Chat Service**
: A microservice providing real-time communication capabilities, including customer support chat and AI-powered chatbot functionality.

**Circuit Breaker**
: A design pattern that prevents cascading failures in distributed systems by monitoring for failures and temporarily disabling failing services.

**Cloud-Native**
: An approach to building and running applications that fully exploits the advantages of cloud computing, including scalability, resilience, and manageability.

**Command Query Responsibility Segregation (CQRS)**
: An architectural pattern that separates read and write operations into different models, optimizing each for their specific use cases.

**Container**
: A lightweight, portable unit that packages an application and its dependencies, enabling consistent deployment across different environments.

**Correlation ID**
: A unique identifier passed through all parts of a transaction or request flow, enabling tracing and debugging across distributed systems.

## D

**Domain-Driven Design (DDD)**
: A software development approach that focuses on modeling the business domain and organizing code around business concepts rather than technical concerns.

**Domain Event**
: An event that represents something significant that happened in the business domain, used to communicate changes between different parts of the system.

**Docker**
: A platform for developing, shipping, and running applications using container technology.

## E

**Event Bus**
: A messaging infrastructure that enables services to communicate through events in an asynchronous, decoupled manner.

**Event-Driven Architecture**
: An architectural pattern where components communicate through the production and consumption of events, enabling loose coupling and scalability.

**Event Sourcing**
: A pattern where state changes are stored as a sequence of events, providing a complete audit trail and enabling temporal queries.

**Event Store**
: A database optimized for storing and retrieving events in an event sourcing system.

**Eventual Consistency**
: A consistency model where the system will become consistent over time, often used in distributed systems to achieve better availability and partition tolerance.

## G

**Gemma 3**
: An AI language model used in BookWorm for generating intelligent chatbot responses and natural language processing tasks.

**gRPC**
: A high-performance, language-agnostic remote procedure call (RPC) framework that uses HTTP/2 and Protocol Buffers.

## H

**Health Check**
: An endpoint or mechanism that reports the operational status of a service, used for monitoring and load balancing decisions.

**HybridCache**
: A caching solution in .NET that combines multiple caching strategies for optimal performance and flexibility.

## I

**Inbox Pattern**
: A messaging pattern that ensures exactly-once processing of incoming messages by storing them in a database before processing.

**Infrastructure as Code (IaC)**
: The practice of managing and provisioning infrastructure through machine-readable configuration files rather than manual processes.

**Integration Test**
: Testing that verifies the interaction between different components or services to ensure they work correctly together.

## J

**JWT (JSON Web Token)**
: A compact, URL-safe token format used for securely transmitting information between parties as a JSON object.

## K

**Keycloak**
: An open-source identity and access management solution that provides authentication and authorization services.

## L

**Load Balancer**
: A component that distributes incoming requests across multiple server instances to ensure optimal resource utilization and high availability.

## M

**Microservices**
: An architectural style where applications are built as a collection of loosely coupled, independently deployable services.

**Message Broker**
: A software component that enables communication between different applications or services through message passing.

**MongoDB**
: A document-oriented NoSQL database used in BookWorm for storing chat messages and conversations.

## N

**.NET Aspire**
: Microsoft's cloud-native development framework that provides tools and libraries for building distributed applications.

**Nomic Embed Text**
: An AI service used in BookWorm for generating text embeddings to enable semantic search capabilities.

## O

**OAuth 2.0**
: An authorization framework that enables applications to obtain limited access to user accounts on behalf of the user.

**OpenAPI**
: A specification for describing REST APIs, providing a standard way to document API endpoints, parameters, and responses.

**OpenID Connect (OIDC)**
: An identity layer built on top of OAuth 2.0 that provides authentication services.

**Ordering Service**
: A microservice responsible for managing the complete order lifecycle, from creation to fulfillment, in the BookWorm platform.

**Outbox Pattern**
: A messaging pattern that ensures reliable event publishing by storing events in the same database transaction as business data changes.

## P

**PostgreSQL**
: An open-source relational database management system used as the primary database for most BookWorm services.

## Q

**Quality Attribute**
: A measurable characteristic of a system that indicates how well it satisfies stakeholder needs, such as performance, security, or maintainability.

**Quality Scenario**
: A structured description of a quality requirement that includes stimulus, environment, artifact, response, and response measure.

## R

**Rating Service**
: A microservice that handles book reviews, ratings, and recommendation algorithms in the BookWorm platform.

**Redis**
: An in-memory data structure store used for caching and session management in BookWorm.

**Resilience**
: The ability of a system to handle and recover from failures, continuing to provide service even when some components fail.

## S

**Saga Pattern**
: A design pattern for managing distributed transactions across multiple services, providing consistency without requiring distributed locks.

**Scalability**
: The ability of a system to handle increased load by adding resources to the system, either horizontally (more instances) or vertically (more powerful instances).

**SendGrid**
: A cloud-based email delivery service used by BookWorm for sending notifications and communications.

**Service Discovery**
: A mechanism that enables services in a distributed system to find and communicate with each other without hardcoded addresses.

**SignalR**
: A library for adding real-time web functionality to applications, enabling server-side code to push content to clients instantly.

## T

**Telemetry**
: The automatic collection and transmission of data from remote sources for monitoring and analysis purposes.

**Technical Debt**
: The cost of additional rework caused by choosing easy solutions now instead of better approaches that would take longer.

## U

**Ubiquitous Language**
: A common language shared by domain experts and developers within a specific bounded context in Domain-Driven Design.

**Unit Test**
: Testing individual components or modules in isolation to verify they work correctly according to their specifications.

## V

**Vertical Slice Architecture**
: An architectural approach that organizes code by feature rather than by technical layer, grouping all code for a feature together.

**VuePress**
: A static site generator optimized for creating documentation websites, used for generating BookWorm's architecture documentation.

## W

**WebSocket**
: A communication protocol that provides full-duplex communication channels over a single TCP connection, used for real-time features.

## Y

**YARP (Yet Another Reverse Proxy)**
: A Microsoft library for creating reverse proxy and API gateway solutions in .NET applications.

---

## Domain-Specific Terms

### BookWorm Business Domain

**Author**
: A person who writes books available in the BookWorm catalog. Authors can have multiple books and biographical information.

**Book**
: A product in the BookWorm catalog with properties like title, description, price, category, and availability status.

**Category**
: A classification system for organizing books by genre, subject, or other criteria in the catalog.

**Conversation**
: A chat session between customers and support agents or AI chatbots in the communication system.

**Customer**
: A user of the BookWorm platform who can browse, purchase, and review books.

**Inventory**
: The available stock quantity for each book in the catalog, managed through inventory reservation and updates.

**Order**
: A customer's request to purchase one or more books, including shipping and payment information.

**Participant**
: An entity (customer, agent, or bot) that can participate in chat conversations.

**Review**
: Customer feedback and rating for a book, contributing to the overall book rating and recommendations.

**Shopping Cart**
: A temporary collection of books a customer intends to purchase, managed by the basket service.

---

*This glossary provides definitions for terms used throughout the BookWorm architecture documentation. It serves as a reference for developers, stakeholders, and anyone working with or studying the system.*
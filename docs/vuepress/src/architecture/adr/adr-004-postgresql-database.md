---
category:
  - Architecture Decisions Records
tag:
  - ADR
---

# ADR-004: PostgreSQL as Primary Database

## Status

**Accepted** - December 2024

## Context

The system needs a reliable, ACID-compliant database for transactional data with support for complex queries and JSON documents. Different services have varying data requirements:

- **Transactional Consistency**: Orders, payments, and financial data
- **Complex Relationships**: Books, authors, categories, and ratings
- **JSON Flexibility**: Configuration, metadata, and dynamic schemas
- **Search Capabilities**: Full-text search across book content
- **Performance**: High-read workloads for catalog browsing
- **Scalability**: Support for growing data volumes

## Decision

Use PostgreSQL as the primary relational database for most services, with Redis for high-performance caching scenarios.

### Service-Specific Database Decisions

| Service          | Primary Database | Rationale                                             |
| ---------------- | ---------------- | ----------------------------------------------------- |
| **Catalog**      | PostgreSQL       | Complex relationships, full-text search, JSON support |
| **Ordering**     | PostgreSQL       | ACID compliance for financial transactions            |
| **Rating**       | PostgreSQL       | Relational data with complex aggregations             |
| **Chat**         | PostgreSQL       | Structured messaging with relational integrity        |
| **Basket**       | Redis            | High-performance caching, session-based data          |
| **Notification** | PostgreSQL       | Message queuing and delivery tracking                 |

## Rationale

### Why PostgreSQL?

1. **ACID Compliance**: Strong consistency guarantees for critical business data
2. **JSON Support**: Native JSONB for flexible schemas and metadata
3. **Performance**: Excellent query performance with proper indexing
4. **Full-Text Search**: Built-in text search capabilities
5. **Azure Integration**: Azure Database for PostgreSQL provides managed service
6. **Ecosystem**: Mature .NET integration with Entity Framework Core
7. **Extensions**: Rich extension ecosystem (PostGIS, pg_stat_statements, etc.)

### Database Schema Strategy

```sql
-- Example: Catalog Service Schema
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Authors table
CREATE TABLE authors (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    biography TEXT,
    metadata JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Books table
CREATE TABLE books (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(500) NOT NULL,
    description TEXT,
    price DECIMAL(10,2),
    author_id UUID REFERENCES authors(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Full-text search index
CREATE INDEX idx_books_search ON books USING GIN(search_vector);
CREATE INDEX idx_books_metadata ON books USING GIN(metadata);
```

## Implementation

### Entity Framework Core Configuration

```csharp
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
```

### Repository Pattern Implementation

```csharp
public interface IAuthorRepository : IRepository<Author>
{
    Task<Author> AddAsync(Author author, CancellationToken cancellationToken);
    Task<IReadOnlyList<Author>> ListAsync(CancellationToken cancellationToken);
    void Delete(Author author);
    Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Author?> FirstOrDefaultAsync(
        ISpecification<Author> spec,
        CancellationToken cancellationToken = default
    );
}

public sealed class AuthorRepository(CatalogDbContext context) : IAuthorRepository
{
    private readonly CatalogDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Author> AddAsync(Author author, CancellationToken cancellationToken)
    {
        var entry = await _context.Authors.AddAsync(author, cancellationToken);
        return entry.Entity;
    }

    public async Task<IReadOnlyList<Author>> ListAsync(CancellationToken cancellationToken)
    {
        return await _context.Authors.ToListAsync(cancellationToken);
    }

    public void Delete(Author author)
    {
        _context.Authors.Remove(author);
    }

    public async Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Authors.FindAsync([id], cancellationToken);
    }

    public async Task<Author?> FirstOrDefaultAsync(
        ISpecification<Author> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(_context.Authors.AsQueryable(), spec)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

## Consequences

### Positive Outcomes

- **Data Integrity**: ACID transactions ensure consistent state
- **Query Flexibility**: Complex joins and aggregations supported
- **JSON Capabilities**: Flexible schema evolution with JSONB
- **Performance**: Excellent read/write performance with proper indexing
- **Full-Text Search**: Built-in search eliminates need for external search engine
- **Tooling**: Rich ecosystem of administration and monitoring tools
- **Managed Service**: Azure Database for PostgreSQL handles maintenance

### Negative Outcomes

- **Scaling Limitations**: Vertical scaling limits compared to NoSQL
- **Write Performance**: High-write scenarios may require optimization
- **Complexity**: Advanced features require PostgreSQL expertise
- **Cost**: Managed service costs can increase with scale
- **Single Point of Failure**: Database becomes critical system component

### Mitigation Strategies

- **Read Replicas**: Use read replicas for high-read workloads
- **Connection Pooling**: Implement connection pooling for better resource utilization
- **Monitoring**: Comprehensive database monitoring and alerting
- **Backup Strategy**: Regular automated backups and point-in-time recovery
- **Performance Tuning**: Regular query optimization and index maintenance

## Monitoring and Observability

### Query Performance

```sql
-- Enable query statistics
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

-- Monitor slow queries
SELECT query, calls, total_time, mean_time, rows
FROM pg_stat_statements
WHERE mean_time > 100  -- queries taking more than 100ms
ORDER BY mean_time DESC
LIMIT 10;
```

## Related Decisions

- [ADR-002: Event-Driven Architecture with CQRS](adr-002-event-driven-cqrs.md)
- [ADR-003: .NET Aspire for Cloud-Native Development](adr-003-aspire-cloud-native.md)
- [ADR-007: Container-First Deployment Strategy](adr-007-container-deployment.md)

## Future Considerations

- **Sharding Strategy**: Evaluate horizontal partitioning for scale
- **Multi-Region**: Consider cross-region replication for global deployment
- **Advanced Indexing**: Implement specialized indexes for performance optimization

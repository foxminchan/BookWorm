---
category:
  - Architecture Decisions Records
tag:
  - ADR
---

# ADR-006: SignalR for Real-time Communication

## Status

**Accepted** - December 2024

## Context

The chat functionality requires real-time, bidirectional communication between clients and servers with support for multiple transport protocols and horizontal scaling.

## Decision

Use ASP.NET Core SignalR for real-time communication in the chat service.

## Rationale

- **Transport Flexibility**: WebSockets, Server-Sent Events, Long Polling fallback
- **Scaling Support**: Azure SignalR Service for horizontal scaling
- **Type Safety**: Strongly-typed hub methods and clients
- **Integration**: Native ASP.NET Core integration
- **Authentication**: Works with existing authentication system

## Implementation

```cs
public sealed class ChatStreamHub : Hub
{
    public IAsyncEnumerable<ClientMessageFragment> Stream(
        Guid id,
        StreamContext streamContext,
        IChatStreaming streaming,
        CancellationToken token
    )
    {
        return StreamAsync();

        async IAsyncEnumerable<ClientMessageFragment> StreamAsync()
        {
            await foreach (
                var message in streaming.GetMessageStream(
                    id,
                    streamContext.LastMessageId,
                    streamContext.LastFragmentId,
                    token
                )
            )
            {
                yield return message;
            }
        }
    }
}
```

## Consequences

### Positive

- Real-time user experience
- Automatic transport fallback
- Easy horizontal scaling with Azure SignalR
- Built-in connection management

### Negative

- Stateful connections require careful scaling
- Additional complexity in client applications
- WebSocket compatibility considerations

## Related Decisions

- [ADR-003: .NET Aspire for Cloud-Native Development](adr-003-aspire-cloud-native.md)

using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.McpTools.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddQdrantClient(Components.VectorDb);

        builder.AddOllamaApiClient(Components.Ollama.Embedding).AddEmbeddingGenerator();

        // Add exception handlers
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddScoped<ISearch, HybridSearch>();
        services.AddSingleton<IVectorStore, QdrantVectorStore>();

        // Configure gRPC
        services.AddGrpc();
        services.AddGrpcServiceReference<BookGrpcServiceClient>(
            $"https://{Application.Catalog}",
            HealthStatus.Degraded
        );
        services.AddSingleton<IBookService, BookService>();

        services.AddMcpServer().WithHttpTransport().WithTools<Product>();
    }
}

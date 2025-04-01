namespace BookWorm.Catalog.Features.Books.Get;

public sealed record GetBookQuery(Guid Id) : IQuery<BookDto>;

public sealed class GetBookHandler(IBookRepository repository, IMapper<Book, BookDto> mapper)
    : IQueryHandler<GetBookQuery, BookDto>
{
    public async Task<BookDto> Handle(GetBookQuery request, CancellationToken cancellationToken)
    {
        var book = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(book, $"Book with id {request.Id} not found.");

        return mapper.MapToDto(book);
    }
}

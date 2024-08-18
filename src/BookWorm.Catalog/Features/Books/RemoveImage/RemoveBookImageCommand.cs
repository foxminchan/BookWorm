﻿using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.Features.Books.RemoveImage;

public sealed record RemoveBookImageCommand(Guid Id) : ICommand<Result>;

public class RemoveBookImageHandler(IRepository<Book> repository, IAzuriteService azurite)
    : ICommandHandler<RemoveBookImageCommand, Result>
{
    public async Task<Result> Handle(RemoveBookImageCommand request, CancellationToken cancellationToken)
    {
        var book = await repository.GetByIdAsync(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, book);

        await azurite.DeleteFileAsync(book.ImageUrl!, cancellationToken);
        book.RemoveImage();
        await repository.UpdateAsync(book, cancellationToken);

        return Result.Success();
    }
}

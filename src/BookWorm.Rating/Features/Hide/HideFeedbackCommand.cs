﻿using Ardalis.Result;
using Ardalis.GuardClauses;
using BookWorm.Core.SharedKernel;
using BookWorm.Rating.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookWorm.Rating.Features.Hide;

public sealed record HideFeedbackCommand(ObjectId Id) : ICommand<Result>;

public sealed class HideFeedbackHandler(IMongoCollection<Feedback> collection) : ICommandHandler<HideFeedbackCommand, Result>
{
    public async Task<Result> Handle(HideFeedbackCommand request, CancellationToken cancellationToken)
    {
        var filter = Builders<Feedback>.Filter.Eq(x => x.Id, request.Id);

        var feedback = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, feedback);

        feedback.Hide();

        await collection.ReplaceOneAsync(filter, feedback, cancellationToken: cancellationToken);

        return Result.Success();
    }
}

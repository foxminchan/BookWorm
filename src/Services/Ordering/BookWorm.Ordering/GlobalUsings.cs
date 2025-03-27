﻿global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Security.Claims;
global using System.Text.Json.Serialization;
global using BookWorm.Basket.Grpc.Services;
global using BookWorm.Constants;
global using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
global using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
global using BookWorm.Ordering.Domain.Events;
global using BookWorm.Ordering.Domain.Exceptions;
global using BookWorm.Ordering.Domain.Projections;
global using BookWorm.Ordering.Grpc;
global using BookWorm.Ordering.Grpc.Services.Basket;
global using BookWorm.Ordering.Grpc.Services.Book;
global using BookWorm.Ordering.Infrastructure;
global using BookWorm.Ordering.Infrastructure.EventStore;
global using BookWorm.Ordering.Infrastructure.Filters;
global using BookWorm.Ordering.Infrastructure.Headers;
global using BookWorm.Ordering.Infrastructure.Idempotency;
global using BookWorm.Ordering.Infrastructure.Services;
global using BookWorm.ServiceDefaults;
global using BookWorm.ServiceDefaults.Keycloak;
global using BookWorm.SharedKernel.ActivityScope;
global using BookWorm.SharedKernel.Command;
global using BookWorm.SharedKernel.EF;
global using BookWorm.SharedKernel.Endpoints;
global using BookWorm.SharedKernel.EventBus;
global using BookWorm.SharedKernel.Exceptions;
global using BookWorm.SharedKernel.Pipelines;
global using BookWorm.SharedKernel.Query;
global using BookWorm.SharedKernel.Repository;
global using BookWorm.SharedKernel.SeedWork;
global using BookWorm.SharedKernel.SeedWork.Event;
global using BookWorm.SharedKernel.SeedWork.Model;
global using BookWorm.SharedKernel.Versioning;
global using FluentValidation;
global using Marten;
global using Marten.Events.Projections;
global using MassTransit;
global using Medallion.Threading;
global using Medallion.Threading.Redis;
global using MediatR;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.FeatureManagement;

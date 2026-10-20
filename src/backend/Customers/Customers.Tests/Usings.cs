// Global using directives

global using System.Security.Claims;
global using System.Text.Json;
global using Customers.CommandHandlers.Profiles;
global using Customers.CommandHandlers.Purchases;
global using Customers.Domain.Customers;
global using Customers.Infrastructure.Consumers;
global using Customers.Infrastructure.Messaging;
global using Customers.Web.Api.Authentication;
global using Customers.Web.Api.WebApplicationExtensions;
global using FluentAssertions;
global using HotChocolate;
global using HotChocolate.Execution;
global using MassTransit;
global using MassTransit.Testing;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging.Abstractions;
global using Moq;
global using NUnit.Framework;
global using Shared.Code;
global using Shared.Code.Commands;
global using Shared.Code.IntegrationEvents;
global using Shared.Code.Models;
global using Shared.Code.Validation;
global using static Customers.Web.Api.WebApplicationExtensions.WebApplicationExtensions;

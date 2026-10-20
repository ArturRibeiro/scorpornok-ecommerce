// Global using directives

global using System.Diagnostics.CodeAnalysis;
global using Customers.CommandHandlers.Profiles;
global using Customers.CommandHandlers.Purchases;
global using Customers.Domain.Customers;
global using Customers.Infrastructure.Consumers;
global using Customers.Infrastructure.EntityConfigurations;
global using Customers.Infrastructure.Messaging;
global using Customers.Infrastructure.Repositories;
global using MassTransit;
global using MediatR;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Shared.Code;
global using Shared.Code.Imp.Bus;
global using Shared.Code.IntegrationEvents;
global using Shared.Code.Models;
global using Shared.Code.Notifications;

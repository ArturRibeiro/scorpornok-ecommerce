// Global using directives

global using System;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Threading;
global using System.Threading.Tasks;
global using MediatR;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Orders.CommandHandlers.Orders;
global using Orders.CommandHandlers.Orders.Imp.CommandHandlers;
global using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create;
global using Orders.CommandHandlers.Orders.Imp.Payments;
global using Orders.Domain.Order.Orders;
global using Orders.Infrastructure.EntityConfigurations;
global using Orders.Infrastructure.Repositories;
global using Shared.Code;
global using Shared.Code.Imp.Bus;
global using Shared.Code.Models;
global using Shared.Code.Notifications;
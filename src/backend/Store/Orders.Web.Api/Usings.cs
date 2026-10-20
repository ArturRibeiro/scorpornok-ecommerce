// Global using directives

global using System.Security.Claims;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Orders.CommandHandlers.Orders;
global using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create;
global using Orders.CommandHandlers.Orders.Imp.CommandHandlers.History;
global using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
global using Orders.Domain.Order.Orders;
global using Orders.Infrastructure;
global using Orders.Infrastructure.Extensions;
global using Orders.Infrastructure.Seeds;
global using Orders.Web.Api;
global using Orders.Web.Api.Authentication;
global using Orders.Web.Api.Hubs;
global using Orders.Web.Api.WebApplicationExtensions;
global using Shared.Code;

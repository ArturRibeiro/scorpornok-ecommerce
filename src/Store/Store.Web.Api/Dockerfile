FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Store/Store.Web.Api/Store.Web.Api.csproj", "Store.Web.Api/"]
RUN dotnet restore "src/Store/Store.Web.Api/Store.Web.Api.csproj"
COPY . .
WORKDIR "/src/Store.Web.Api"
RUN dotnet build "Store.Web.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Store.Web.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.Web.Api.dll"]

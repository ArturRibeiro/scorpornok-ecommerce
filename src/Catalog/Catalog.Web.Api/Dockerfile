FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Catalog/Catalog.Web.Api/Catalog.Web.Api.csproj", "Catalog.Web.Api/"]
RUN dotnet restore "src/Catalog/Catalog.Web.Api/Catalog.Web.Api.csproj"
COPY . .
WORKDIR "/src/Catalog.Web.Api"
RUN dotnet build "Catalog.Web.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalog.Web.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.Web.Api.dll"]

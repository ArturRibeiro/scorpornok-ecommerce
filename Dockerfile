# Imagem genérica para as Web APIs. O projeto é escolhido pelos build args:
#   PROJECT  caminho do .csproj a partir da raiz do repositório
#   ASSEMBLY nome da dll gerada (sem extensão)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT
WORKDIR /source

COPY Directory.Build.props ./
COPY src/ src/
RUN dotnet publish "$PROJECT" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
ARG ASSEMBLY
ENV ASSEMBLY=$ASSEMBLY
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "exec dotnet \"$ASSEMBLY.dll\""]

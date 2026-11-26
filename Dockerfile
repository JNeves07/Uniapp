# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY UniApp.Backend/UniApp.Api.csproj UniApp.Backend/
RUN dotnet restore UniApp.Backend/UniApp.Api.csproj

COPY UniApp.Backend/ UniApp.Backend/
RUN dotnet publish UniApp.Backend/UniApp.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Usuario nao-root (seguranca)
RUN groupadd -r uniapp && useradd -r -g uniapp uniapp && chown -R uniapp:uniapp /app
USER uniapp

COPY --from=build --chown=uniapp:uniapp /app/publish .
ENTRYPOINT ["dotnet", "UniApp.Api.dll"]

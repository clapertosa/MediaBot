FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY Domain/*.csproj Domain/
COPY Application/*.csproj Application/
COPY Infrastructure/*.csproj Infrastructure/
COPY DiscordConsoleApp/*.csproj DiscordConsoleApp/
RUN dotnet restore "./DiscordConsoleApp/DiscordConsoleApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./DiscordConsoleApp/DiscordConsoleApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./DiscordConsoleApp/DiscordConsoleApp.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet",  "DiscordConsoleApp.dll"]
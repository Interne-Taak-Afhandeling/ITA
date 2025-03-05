FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InterneTaakAfhandeling.Poller/InterneTaakAfhandeling.Poller.csproj", "InterneTaakAfhandeling.Poller/"]
RUN dotnet restore "InterneTaakAfhandeling.Poller/InterneTaakAfhandeling.Poller.csproj"
COPY . .
WORKDIR "/src/InterneTaakAfhandeling.Poller"
RUN dotnet publish "InterneTaakAfhandeling.Poller.csproj" -c Release -o /app/publish
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InterneTaakAfhandeling.Poller.dll"]
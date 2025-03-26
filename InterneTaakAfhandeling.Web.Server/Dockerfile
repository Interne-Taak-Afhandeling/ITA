FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Install Node.js
RUN apt-get update && \
    apt-get install -y curl && \
    curl -sL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

WORKDIR /src
COPY ["InterneTaakAfhandeling.Web.Server/InterneTaakAfhandeling.Web.Server.csproj", "InterneTaakAfhandeling.Web.Server/"]
COPY ["InterneTaakAfhandeling.Web.Client/InterneTaakAfhandeling.Web.Client.esproj", "InterneTaakAfhandeling.Web.Client/"]
RUN dotnet restore "InterneTaakAfhandeling.Web.Server/InterneTaakAfhandeling.Web.Server.csproj"

COPY . .
WORKDIR "/src/InterneTaakAfhandeling.Web.Client"
RUN npm install
RUN npm run build

WORKDIR "/src/InterneTaakAfhandeling.Web.Server"
RUN dotnet build "InterneTaakAfhandeling.Web.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InterneTaakAfhandeling.Web.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InterneTaakAfhandeling.Web.Server.dll"]
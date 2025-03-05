FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InterneTaakAfhandeling.Web/InterneTaakAfhandeling.Web.csproj", "InterneTaakAfhandeling.Web/"]
RUN dotnet restore "InterneTaakAfhandeling.Web/InterneTaakAfhandeling.Web.csproj"
COPY . .
WORKDIR "/src/InterneTaakAfhandeling.Web"
RUN dotnet build "InterneTaakAfhandeling.Web.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "InterneTaakAfhandeling.Web.csproj" -c Release -o /app/publish
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InterneTaakAfhandeling.Web.dll"]
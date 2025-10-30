# ---- Runtime (small) ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# ---- Build (SDK) ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Backend.csproj", "./"]

RUN dotnet restore "Backend.csproj"

COPY . .

RUN dotnet publish "Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Final ----
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Backend.dll"]

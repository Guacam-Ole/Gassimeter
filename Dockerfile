# Use the official .NET 9.0 runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Use the .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file
COPY GassiMeter.csproj .
RUN dotnet restore "GassiMeter.csproj"

# Copy the source code
COPY . .
RUN dotnet build "GassiMeter.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "GassiMeter.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .


ENV TZ=Europe/Berlin
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Set the entry point
ENTRYPOINT ["dotnet", "GassiMeter.dll"]

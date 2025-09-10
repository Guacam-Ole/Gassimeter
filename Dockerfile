# Use the .NET 9.0 SDK for publishing
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS publish
WORKDIR /src

# Copy the project file and restore dependencies
COPY GassiMeter.csproj .
RUN dotnet restore

# Copy the source code and publish the application
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Final stage with runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENV TZ=Europe/Berlin
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Set the entry point
ENTRYPOINT ["dotnet", "GassiMeter.dll"]

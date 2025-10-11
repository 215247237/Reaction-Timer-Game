# Use .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ReactionMachineProject.sln .
COPY ReactionTimer/ ReactionTimer/

# Restore dependencies
RUN dotnet restore ReactionMachineProject.sln

# Build and publish
RUN dotnet publish ReactionTimer/timer.csproj -c Release -o /app

# Use runtime-only image for final container
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "timer.dll"]

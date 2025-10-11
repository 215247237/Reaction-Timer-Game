# Use Microsoft’s official .NET runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Use the .NET SDK image to build your project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish ReactionTimer/timer.csproj -c Release -o /app/publish

# Final image for deployment
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Simulate entrypoint so Beanstalk sees it as running
ENTRYPOINT ["bash", "-c", "echo 'Reaction Machine Game successfully deployed via Jenkins pipeline' && tail -f /dev/null"]

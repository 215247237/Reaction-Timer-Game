# Use Microsoft’s official .NET runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Use the .NET SDK image to build project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish ReactionTimer/timer.csproj -c Release -o /app/publish

# Final image for deployment
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Install Python to fake a web server
RUN apt-get update && apt-get install -y python3

# Simulate web app so Beanstalk sees the container as running
EXPOSE 80
ENTRYPOINT ["bash", "-c", "echo '<h1>Reaction Machine successfully deployed via Jenkins pipeline!</h1>' > index.html && python3 -m http.server 80"]


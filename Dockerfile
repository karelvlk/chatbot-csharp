# First stage: Use the .NET SDK image to build the app
# Adjust the version as necessary for your project
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the csproj file and restore dependencies
COPY chatbot/*.csproj ./chatbot/
RUN dotnet restore chatbot/chatbot.csproj

# Copy the rest of the app's source code
COPY chatbot/ ./chatbot/

# Publish the application to the out directory
RUN dotnet publish chatbot/chatbot.csproj -c Release -o out

# Second stage: Use the .NET runtime image to run the app
# Adjust the runtime version as necessary
FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app

# Copy the published app from the first stage
COPY --from=build /app/out .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "chatbot.dll"]

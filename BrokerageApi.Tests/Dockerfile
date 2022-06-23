FROM mcr.microsoft.com/dotnet/sdk:6.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Install dotnet-ef tool to run migrations
RUN dotnet tool install --global dotnet-ef --version 6.0.5

# Ensure dotnet tools are in the path
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy csproj and nuget config and restore as distinct layers
COPY ./BrokerageApi.sln ./
COPY ./BrokerageApi/BrokerageApi.csproj ./BrokerageApi/
COPY ./BrokerageApi.Tests/BrokerageApi.Tests.csproj ./BrokerageApi.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./BrokerageApi/BrokerageApi.csproj
RUN dotnet restore ./BrokerageApi.Tests/BrokerageApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out BrokerageApi.Tests/BrokerageApi.Tests.csproj

CMD dotnet test

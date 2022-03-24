FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Install dotnet-ef tool to run migrations
RUN dotnet tool install --global dotnet-ef

# Ensure dotnet tools are in the path
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy csproj and nuget config and restore as distinct layers
COPY ./BrokerageApi.sln ./
COPY ./BrokerageApi/BrokerageApi.csproj ./BrokerageApi/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./BrokerageApi/BrokerageApi.csproj

# Copy everything else and build
COPY . ./
RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

EXPOSE ${PORT:-5100}
CMD ASPNETCORE_URLS=http://+:${PORT:-5100} dotnet ./out/BrokerageApi.dll

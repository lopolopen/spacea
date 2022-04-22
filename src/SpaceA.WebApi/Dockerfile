#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["SpaceA.WebApi/SpaceA.WebApi.csproj", "SpaceA.WebApi/"]
COPY ["SpaceA.Model/SpaceA.Model.csproj", "SpaceA.Model/"]
COPY ["SpaceA.Repository/SpaceA.Repository.csproj", "SpaceA.Repository/"]
COPY ["SpaceA.Common/SpaceA.Common.csproj", "SpaceA.Common/"]
COPY ["Tests/SpaceA.Service.Tests/SpaceA.Service.Tests.csproj", "Tests/SpaceA.Service.Tests/"]
COPY ["SpaceA.sln", "./"]
COPY ["NuGet.config", "./"]
RUN dotnet restore "SpaceA.sln"

COPY . .
WORKDIR "/src/SpaceA.WebApi"
RUN dotnet publish --no-restore -c Release -o /app/publish

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpaceA.WebApi.dll"]
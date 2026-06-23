FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PortfolioWeb.sln", "./"]
COPY ["PortfolioWeb.Api/PortfolioWeb.Api.csproj", "PortfolioWeb.Api/"]
COPY ["PortfolioWeb.Application/PortfolioWeb.Application.csproj", "PortfolioWeb.Application/"]
COPY ["PortfolioWeb.Application.Contract/PortfolioWeb.Application.Contract.csproj", "PortfolioWeb.Application.Contract/"]
COPY ["PortfolioWeb.Domain/PortfolioWeb.Domain.csproj", "PortfolioWeb.Domain/"]
COPY ["PortfolioWeb.Core.Contracts/PortfolioWeb.Core.Contracts.csproj", "PortfolioWeb.Core.Contracts/"]
COPY ["PortfolioWeb.Infrastructure/PortfolioWeb.Infrastructure.csproj", "PortfolioWeb.Infrastructure/"]

RUN dotnet restore "PortfolioWeb.Api/PortfolioWeb.Api.csproj"

COPY . .

RUN dotnet publish "PortfolioWeb.Api/PortfolioWeb.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PortfolioWeb.Api.dll"]

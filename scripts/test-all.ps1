param(
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent

$testProjects = @(
    "PortfolioWeb.Application.Tests\PortfolioWeb.Application.Tests.csproj",
    "PortfolioWeb.Core.Contracts.Tests\PortfolioWeb.Core.Contracts.Tests.csproj",
    "PortfolioWeb.Infrastructure.Tests\PortfolioWeb.Infrastructure.Tests.csproj",
    "PortfolioWeb.Api.Tests\PortfolioWeb.Api.Tests.csproj"
)

Push-Location $repositoryRoot

try {
    foreach ($project in $testProjects) {
        Write-Host "Running tests for $project" -ForegroundColor Cyan

        $arguments = @("test", $project, "--verbosity", "minimal")

        if ($NoRestore) {
            $arguments += "--no-restore"
        }

        & dotnet @arguments

        if ($LASTEXITCODE -ne 0) {
            throw "Tests failed for $project"
        }
    }

    Write-Host "All test projects completed successfully." -ForegroundColor Green
}
finally {
    Pop-Location
}

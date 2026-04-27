FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CairoPaymentEngine.Api/CairoPaymentEngine.Api.csproj", "CairoPaymentEngine.Api/"]
COPY ["CairoPaymentEngine.Application/CairoPaymentEngine.Application.csproj", "CairoPaymentEngine.Application/"]
COPY ["CairoPaymentEngine.Domain/CairoPaymentEngine.Domain.csproj", "CairoPaymentEngine.Domain/"]
COPY ["CairoPaymentEngine.Infrastructure/CairoPaymentEngine.Infrastructure.csproj", "CairoPaymentEngine.Infrastructure/"]
COPY ["CairoPaymentEngine.Persistence/CairoPaymentEngine.Persistence.csproj", "CairoPaymentEngine.Persistence/"]
RUN dotnet restore "CairoPaymentEngine.Api/CairoPaymentEngine.Api.csproj"

COPY . .
WORKDIR "/src/CairoPaymentEngine.Api"
RUN dotnet publish "CairoPaymentEngine.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "CairoPaymentEngine.Api.dll"]

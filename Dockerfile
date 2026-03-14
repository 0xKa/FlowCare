FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FlowCare.slnx ./
COPY src/FlowCare.Domain/FlowCare.Domain.csproj src/FlowCare.Domain/
COPY src/FlowCare.Application/FlowCare.Application.csproj src/FlowCare.Application/
COPY src/FlowCare.Infrastructure/FlowCare.Infrastructure.csproj src/FlowCare.Infrastructure/
COPY src/FlowCare.Api/FlowCare.Api.csproj src/FlowCare.Api/

RUN dotnet restore src/FlowCare.Api/FlowCare.Api.csproj

COPY . .
RUN dotnet publish src/FlowCare.Api/FlowCare.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y --no-install-recommends libgssapi-krb5-2 \
	&& rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .
COPY src/FlowCare.Infrastructure/Data/Seed/example.json /app/seed/example.json

ENTRYPOINT ["dotnet", "FlowCare.Api.dll"]

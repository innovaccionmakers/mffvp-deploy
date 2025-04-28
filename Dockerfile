FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY MFFVP.sln ./

COPY src/ ./src/
COPY test/ ./test/

RUN dotnet restore MFFVP.sln

RUN dotnet publish src/API/MFFVP.Api/MFFVP.Api.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "MFFVP.Api.dll"]
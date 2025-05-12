FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY . .

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:5127
EXPOSE 5127

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:$PORT/ping || exit 1

ENTRYPOINT ["dotnet", "MFFVP.Api.dll"]

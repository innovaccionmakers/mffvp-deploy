FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY . .

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:5127
EXPOSE 5127

ENTRYPOINT ["dotnet", "MFFVP.Api.dll"]

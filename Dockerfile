FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Api_Eden/Api_Eden.csproj Api_Eden/
RUN dotnet restore Api_Eden/Api_Eden.csproj
COPY Api_Eden/ Api_Eden/
RUN dotnet publish Api_Eden/Api_Eden.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["sh","-c","ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet Api_Eden.dll"]

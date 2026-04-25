FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Hems.Api/Hems.Api.csproj Hems.Api/
RUN dotnet restore Hems.Api/Hems.Api.csproj
COPY Hems.Api/ Hems.Api/
RUN dotnet publish Hems.Api/Hems.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y libsqlite3-dev && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
VOLUME ["/app/data"]
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/hems.db"
EXPOSE 8080
ENTRYPOINT ["dotnet", "Hems.Api.dll"]

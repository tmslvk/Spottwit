FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Spottwit.Server.csproj", "./"]
RUN dotnet restore "Spottwit.Server.csproj"

COPY . .
RUN dotnet publish "Spottwit.Server.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Spottwit.Server.dll"]
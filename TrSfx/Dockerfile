FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TrSfx/TrSfx.csproj", "TrSfx/"]
RUN dotnet restore "TrSfx/TrSfx.csproj"
COPY . .
WORKDIR "/src/TrSfx"
RUN dotnet build "TrSfx.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrSfx.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrSfx.dll"]

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TrSfxLib/TrSfxLib.csproj", "TrSfxLib/"]
RUN dotnet restore "TrSfxLib/TrSfxLib.csproj"
COPY . .
WORKDIR "/src/TrSfxLib"
RUN dotnet build "TrSfxLib.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrSfxLib.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrSfxLib.dll"]

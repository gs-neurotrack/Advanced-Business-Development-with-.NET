FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# app ouvindo na porta 5162
ENV ASPNETCORE_URLS=http://0.0.0.0:5162
EXPOSE 5162

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["NeuroTrack/NeuroTrack.csproj", "NeuroTrack/"]
RUN dotnet restore "NeuroTrack/NeuroTrack.csproj"

COPY . .
WORKDIR "/src/NeuroTrack"
RUN dotnet build "./NeuroTrack.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NeuroTrack.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NeuroTrack.dll"]

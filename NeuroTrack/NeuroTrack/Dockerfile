# Etapa 1 — Imagem base (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Força o Kestrel a escutar na porta 5162
ENV ASPNETCORE_URLS=http://0.0.0.0:5162

# Porta exposta pelo container
EXPOSE 5162

# Etapa 2 — Build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia somente o csproj
COPY ["NeuroTrack.csproj", "./"]

# Restore
RUN dotnet restore "NeuroTrack.csproj"

# Copia o resto da solução
COPY . .

# Compila
RUN dotnet build "NeuroTrack.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Etapa 3 — Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NeuroTrack.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Etapa 4 — Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NeuroTrack.dll"]

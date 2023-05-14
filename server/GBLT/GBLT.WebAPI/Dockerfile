#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GBLT.WebAPI/GBLT.WebAPI.csproj", "GBLT.WebAPI/"]
COPY ["GBLT.Core/GBLT.Core.csproj", "GBLT.Core/"]
COPY ["GBLT.Shared/GBLT.Shared.csproj", "GBLT.Shared/"]
COPY ["GBLT.Infrastructure/GBLT.Infrastructure.csproj", "GBLT.Infrastructure/"]
RUN dotnet restore "GBLT.WebAPI/GBLT.WebAPI.csproj"
COPY . .
WORKDIR "/src/GBLT.WebAPI"
RUN dotnet build "GBLT.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GBLT.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GBLT.WebAPI.dll"]
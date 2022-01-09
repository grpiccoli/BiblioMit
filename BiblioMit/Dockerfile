#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BiblioMit/BiblioMit.csproj", "BiblioMit/"]
RUN dotnet restore "BiblioMit/BiblioMit.csproj"
COPY . .
WORKDIR "/src/BiblioMit"
RUN dotnet build "BiblioMit.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BiblioMit.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BiblioMit.dll"]
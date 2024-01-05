#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Enlap_Voting_Backend/Enlap_Voting_Backend.csproj", "Enlap_Voting_Backend/"]
RUN dotnet restore "Enlap_Voting_Backend/Enlap_Voting_Backend.csproj"
COPY . .
WORKDIR "/src/Enlap_Voting_Backend"
RUN dotnet build "Enlap_Voting_Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Enlap_Voting_Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Enlap_Voting_Backend.dll"]
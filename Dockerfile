FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SocialMediaPostAPI.csproj", "."]
RUN dotnet restore "./SocialMediaPostAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SocialMediaPostAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SocialMediaPostAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocialMediaPostAPI.dll"]
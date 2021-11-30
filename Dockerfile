FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["RemoveYZeroBedrock.csproj", "./"]
RUN dotnet restore "RemoveYZeroBedrock.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "RemoveYZeroBedrock.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RemoveYZeroBedrock.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RemoveYZeroBedrock.dll"]

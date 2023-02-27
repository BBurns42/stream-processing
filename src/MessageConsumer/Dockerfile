#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/MessageConsumer/MessageConsumer.csproj", "src/MessageConsumer/"]
RUN dotnet restore "src/MessageConsumer/MessageConsumer.csproj"
COPY . .
WORKDIR "/src/src/MessageConsumer"
RUN dotnet build "MessageConsumer.csproj" -c Release -o /app/build

# Install dotnet debug tools
RUN dotnet tool install --tool-path /tools dotnet-trace \
 && dotnet tool install --tool-path /tools dotnet-counters \
 && dotnet tool install --tool-path /tools dotnet-dump \
 && dotnet tool install --tool-path /tools dotnet-gcdump

FROM build AS publish
RUN dotnet publish "MessageConsumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
# Copy dotnet-tools
WORKDIR /tools
COPY --from=build /tools .

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessageConsumer.dll"]
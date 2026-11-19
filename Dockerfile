# Builds the DeliveryNoteService Web API.
# Build context: this folder, because WebApp references projects in both src/ and service/src/.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/ src/
COPY service/src/ service/src/
RUN dotnet publish service/src/WebApp/WebApp.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# The SQLite database lives in /data so a volume can keep it between container runs.
RUN mkdir /data && chown app /data
USER app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    Persistence__ConnectionStrings__AppDb="Data Source=/data/app.db"
EXPOSE 8080
ENTRYPOINT ["dotnet", "DeliveryNoteService.WebApp.dll"]

# ---- 1) build React ----
FROM node:20 AS frontend
WORKDIR /fe
COPY frontend/package*.json ./
RUN npm install
COPY frontend/ ./
# relative URL: the React app is served by the same ASP.NET service
ENV VITE_API_URL=/api
RUN npm run build

# ---- 2) build ASP.NET ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /src
COPY backend/ ./
RUN dotnet publish hw1aspnet.csproj -c Release -o /out
COPY --from=frontend /fe/dist/ /out/wwwroot/

# ---- 3) run ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend /out ./
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "hw1aspnet.dll"]

FROM node:lts AS node_builder

WORKDIR /app
COPY ./FileSystemWeb/ClientApp .
RUN npm install
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder

WORKDIR /app
COPY ./StdOttLib ./StdOttLib
COPY ./FileSystemCommon ./FileSystemCommon
COPY ./FileSystemWeb ./FileSystemWeb

WORKDIR /app/FileSystemWeb

# install entity framework tools
# RUN dotnet tool install --global --version 5.0.17 dotnet-ef
# ENV PATH $PATH:/root/.dotnet/tools
# RUN cp ./template.db ./auth.db
# RUN dotnet ef database update

COPY --from=node_builder /app/build ./ClientApp/build
RUN dotnet publish "FileSystemWeb.csproj" -p:BuildClientApp=false -c Release -o /app/publish
RUN cp ./template.db /app/publish/auth.db


FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runner

WORKDIR /app
COPY --from=builder /app/publish .

ENTRYPOINT ["dotnet", "FileSystemWeb.dll"]

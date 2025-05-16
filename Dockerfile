FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln ./
COPY GamesFinder.API/GamesFinder.API.csproj GamesFinder.API/
COPY GamesFinder.Application/GamesFinder.Application.csproj GamesFinder.Application/
COPY GamesFinder.DAL/GamesFinder.DAL.csproj GamesFinder.DAL/
COPY GamesFinder.Domain/GamesFinder.Domain.csproj GamesFinder.Domain/

RUN dotnet nuget locals all --clear
RUN dotnet restore
COPY . .

RUN dotnet publish GamesFinder.API/GamesFinder.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "GamesFinder.API.dll"]

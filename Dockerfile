# Используем официальный .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Копируем файл решения и проекты
COPY *.sln ./
COPY API/API.csproj API/
COPY Application/Application.csproj Application/
COPY DAL/DAL.csproj DAL/
COPY Domain/Domain.csproj Domain/

# Восстанавливаем зависимости
RUN dotnet restore

# Копируем всё остальное
COPY . .

# Сборка решения в релиз режиме
RUN dotnet publish API/API.csproj -c Release -o /app/publish

# Финальный этап - минимальный runtime образ
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Указываем порт, который будет слушать контейнер
EXPOSE 80

# Запускаем приложение
ENTRYPOINT ["dotnet", "API.dll"]

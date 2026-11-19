using PolesTaxi.UserService.app;

var builder = WebApplication.CreateBuilder(args);

// Настраиваем сервисы (MongoDB, контроллеры, репозитории)
Run.ConfigureServices(builder);

var app = builder.Build();

// Настраиваем конвейер (маршрутизация, Swagger)
Run.Configure(app);

// ОБЯЗАТЕЛЬНАЯ СТРОКА: заставляет приложение работать и ждать запросы
app.Run();
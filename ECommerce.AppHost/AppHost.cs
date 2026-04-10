var builder = DistributedApplication.CreateBuilder(args);

// Altyapı Bileşenleri
var rabbitmq = builder.AddRabbitMQ("messaging");

// JWT Secret Key - tek noktadan yönetilir, servisler env variable olarak alır
var jwtKey = builder.AddParameter("jwt-key", secret: true);

// Servisleri tanımla
var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(rabbitmq)
    .WithEnvironment("Jwt__Key", jwtKey);
var discountApi = builder.AddProject<Projects.Discount_API>("discount-api")
    .WithEnvironment("Jwt__Key", jwtKey);
var identityApi = builder.AddProject<Projects.Identity_API>("identity-api")
    .WithEnvironment("Jwt__Key", jwtKey);
var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithEnvironment("Jwt__Key", jwtKey);
var paymentApi = builder.AddProject<Projects.Payment_API>("payment-api")
    .WithEnvironment("Jwt__Key", jwtKey);
var orderApi = builder.AddProject<Projects.Order_API>("order-api")
    .WithReference(rabbitmq)
    .WithReference(paymentApi)
    .WithEnvironment("Jwt__Key", jwtKey);
var inventoryApi = builder.AddProject<Projects.Inventory_API>("inventory-api")
    .WithReference(rabbitmq)
    .WithEnvironment("Jwt__Key", jwtKey);

//Gatewayi tanımla
var gateway = builder.AddProject<Projects.YarpGateway_API>("gateway")
    .WithReference(catalogApi)
    .WithReference(discountApi)
    .WithReference(basketApi)
    .WithReference(identityApi)
    .WithReference(orderApi)
    .WithReference(paymentApi)
    .WithReference(inventoryApi);

//WebUIı tanımla
builder.AddProject<Projects.WebUI>("web-ui")
    .WithReference(gateway)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(orderApi);

builder.Build().Run();

var builder = DistributedApplication.CreateBuilder(args);

// Altyapı Bileşenleri
var rabbitmq = builder.AddRabbitMQ("messaging");

// Servisleri tanımla
var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api");
var discountApi = builder.AddProject<Projects.Discount_API>("discount-api");
var identityApi = builder.AddProject<Projects.Identity_API>("identity-api");
var basketApi = builder.AddProject<Projects.Basket_API>("basket-api");
var paymentApi = builder.AddProject<Projects.Payment_API>("payment-api");
var orderApi = builder.AddProject<Projects.Order_API>("order-api")
    .WithReference(rabbitmq)
    .WithReference(paymentApi);
var inventoryApi = builder.AddProject<Projects.Inventory_API>("inventory-api")
    .WithReference(rabbitmq);

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
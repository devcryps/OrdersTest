# OrdersManagement

API de pedidos en .NET 8 con vista en Blazor Server. Persiste en Oracle, cachea el listado en Redis y publica el evento `OrderCreated` en RabbitMQ; un consumidor confirma el pedido.

## Ejecución

Requisitos: .NET 8 SDK y Docker.

    docker compose up -d
    dotnet run --project src/OrdersManagement.WebApi
    dotnet run --project src/OrdersManagement.Web

- Antes de la API, ejecutar `database/scripts/01_create_schema.sql` en Oracle.
- Copiar `appsettings.Development.example.json` como `appsettings.Development.json` y completar la cadena de conexión.
- API: http://localhost:5100. Web: http://localhost:5200.
- Pruebas: `dotnet test`.

## Endpoints

- `POST /api/pedidos`
- `GET /api/pedidos?pageNumber=1&pageSize=10`

## Diseño

- Capas: Domain, Application, Infrastructure, WebApi y Web.
- Las validaciones están en la entidad `Order`.
- La caché del listado se invalida al crear o confirmar un pedido.
- Sin configuración, la app usa implementaciones en memoria.

## Pendiente

- Outbox para publicar el evento junto con el guardado.
- Dead-letter queue, reintentos y health checks.

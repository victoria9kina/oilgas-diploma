# OilGasApi: серверная часть (лаба 2)

Web API на ASP.NET Core (.NET 10) по данным **Argentina Oil and Gas Dataset** (Kaggle, `pedronaon/argentina-oil-and-gas-dataset`).
Часть дипломного проекта «Разработка автоматизированной системы прогнозирования и геовизуализации показателей добычи нефти и газа».

## Структура

| Папка | Содержимое |
|---|---|
| `Models/` | `Production`, `BasinProduction`, `DrillingActivity` (данные + бизнес-логика), справочник `Basins`, `Percent`, `User` |
| `Controllers/` | CRUD для трёх сущностей, 3 сводных отчёта, вход |
| `Data/` | `AppDbContext` (EF Core + SQLite), загрузка CSV в БД при первом запуске |
| `Auth/` | `AuthOptions`: параметры и выдача JWT-токена |
| `postman/` | Коллекция Postman (33 запроса с проверкой статуса) |

Одна строка CSV (месяц) раскладывается на три сущности:

- **Production**: национальная добыча нефти и газа (традиционная, сланцевая, tight, итог), доля нетрадиционной добычи.
- **BasinProduction**: добыча нефти по бассейну за месяц, доля бассейна.
- **DrillingActivity**: цена Brent, метры бурения, скважины, проходка на скважину, категория цены (Low / Medium / High).

## Права доступа

| Действие | Без токена | `user` | `admin` |
|---|---|---|---|
| GET `/api/production`, `/api/basins`, `/api/drilling` | да | да | да |
| Сводные отчёты `/api/reports/*` | **401** | да | да |
| POST / PUT / DELETE | **401** | **403** | да |

Пользователи: `admin / admin123`, `user / user123`. Вход: `POST /api/auth/login` с телом `{ "username": "...", "password": "..." }`, в ответе токен. Токен передаётся в заголовке `Authorization: Bearer <токен>`.

## Запросы

| Запрос | Что делает |
|---|---|
| `GET /api/production?from=&to=` | национальная добыча за период |
| `GET /api/basins?basin=&from=&to=` | добыча по бассейнам |
| `GET /api/basins/info` | справочник бассейнов с координатами |
| `GET /api/drilling?from=&to=&priceCategory=` | бурение и цена Brent |
| `GET/POST/PUT/DELETE /api/{production,basins,drilling}/{id}` | CRUD |
| `GET /api/reports/monthly-summary?from=&to=` | Production + DrillingActivity по месяцам |
| `GET /api/reports/basin-shares?date=2023-06-01` | доли бассейнов за месяц + национальная добыча нефти |
| `GET /api/reports/top-basins?year=2023` | рейтинг бассейнов за год с координатами |

Даты передаются в формате `yyyy-MM-dd`, месяц обозначается первым числом.

## Запуск

```bash
dotnet run
```

API работает на `http://localhost:5080`. При первом запуске создаётся `oilgas.db` и загружаются 203 месяца из CSV. Чтобы загрузить данные заново, удалить `oilgas.db`.

Postman: Import → `postman/OilGasApi.postman_collection.json` → Run collection.

## Релиз

```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -o publish
./publish/OilGasApi --urls http://localhost:5080
```

Self-contained релиз запускается без установленного .NET (macOS на Apple Silicon).

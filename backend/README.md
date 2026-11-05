# OilGasApi: серверная часть дипломного проекта

Web API на ASP.NET Core (.NET 10) по данным **Argentina Oil and Gas Dataset** (Kaggle, `pedronaon/argentina-oil-and-gas-dataset`) и рыночным данным для прогноза.
Часть дипломного проекта «Разработка автоматизированной системы прогнозирования и геовизуализации показателей добычи нефти и газа».

## Структура

| Папка | Содержимое |
|---|---|
| `Models/` | `Production`, `Basin`, `BasinProduction`, `DrillingActivity`, `MarketFactor`, `MarketEvent` (данные + бизнес-логика), `Percent`, `User` |
| `Controllers/` | CRUD для пяти сущностей, 4 сводных отчёта, вход |
| `Data/` | `AppDbContext` (EF Core + SQLite), три CSV и их загрузка в БД при первом запуске |
| `Auth/` | `AuthOptions`: параметры и выдача JWT-токена |
| `postman/` | Коллекция Postman (51 запрос с проверкой статуса) |

## Таблицы

| Таблица | Что хранит | Источник |
|---|---|---|
| **Production** | национальная добыча нефти и газа (традиционная, сланцевая, tight, итог), доля нетрадиционной добычи | Kaggle |
| **Basins** | 5 бассейнов: код, название, координаты для карты | справочник |
| **BasinProductions** | добыча нефти по бассейну за месяц, внешний ключ `BasinId` на `Basins` | Kaggle, колонки `cuenca_*` |
| **DrillingActivities** | цена Brent, метры бурения, скважины, проходка на скважину, категория цены | Kaggle |
| **MarketFactors** | экспорт нефти и газа (млн $), курс песо к доллару, мировой спрос на нефть | datos.gob.ar (INDEC, BCRA), EIA STEO `PATC_WORLD` |
| **MarketEvents** | ключевые события рынка: кризисы, решения ОПЕК, законы, трубопроводы | собрано вручную со ссылками |

Все таблицы, кроме `Basins`, связаны по полю `Date` (первое число месяца). Везде одни и те же 203 месяца: 2009-01 .. 2025-11. В экспорте пропуски означают, что экспорта не было (газ до 2018-05, нефть 2017-04, 2017-06, 2017-07), в CSV для них стоит 0. В одном месяце может быть несколько событий.

## Права доступа

| Действие | Без токена | `user` | `admin` |
|---|---|---|---|
| GET `/api/production`, `/api/basins`, `/api/drilling`, `/api/market-factors`, `/api/events` | да | да | да |
| Сводные отчёты `/api/reports/*` | **401** | да | да |
| POST / PUT / DELETE | **401** | **403** | да |

Пользователи: `admin / admin123`, `user / user123`. Вход: `POST /api/auth/login` с телом `{ "username": "...", "password": "..." }`, в ответе токен. Токен передаётся в заголовке `Authorization: Bearer <токен>`.

## Запросы

| Запрос | Что делает |
|---|---|
| `GET /api/production?from=&to=` | национальная добыча за период |
| `GET /api/production/max-oil` | месяц с максимальной добычей нефти |
| `GET /api/basins?basinId=&from=&to=` | добыча по бассейнам (бассейн подгружается через `Include`) |
| `GET /api/basins/info` | таблица бассейнов с координатами |
| `GET /api/drilling?from=&to=&priceCategory=` | бурение и цена Brent |
| `GET /api/market-factors?from=&to=` | экспорт, курс песо, мировой спрос |
| `GET /api/events?from=&to=&category=` | события рынка |
| `GET/POST/PUT/DELETE /api/{production,basins,drilling,market-factors,events}/{id}` | CRUD |
| `GET /api/reports/monthly-summary?from=&to=` | Production + DrillingActivity по месяцам |
| `GET /api/reports/basin-shares?date=2023-06-01` | доли бассейнов за месяц + национальная добыча нефти |
| `GET /api/reports/top-basins?year=2023` | рейтинг бассейнов за год с координатами |
| `GET /api/reports/forecast-dataset?from=&to=` | датасет для прогноза: добыча, бурение, рынок и события в одной строке на месяц |

Даты передаются в формате `yyyy-MM-dd`, месяц обозначается первым числом.

## Запуск

```bash
dotnet run
```

API работает на `http://localhost:5080`. При первом запуске создаётся `oilgas.db` и загружаются данные из трёх CSV. После изменения моделей или CSV удалить `oilgas.db`, чтобы база создалась заново.

Postman: Import → `postman/OilGasApi.postman_collection.json` → Run collection.

## Релиз

```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -o publish
./publish/OilGasApi --urls http://localhost:5080
```

Self-contained релиз запускается без установленного .NET (macOS на Apple Silicon).

# OilGasApi: серверная часть (лаба 2)

Web API на ASP.NET Core (.NET 10) по данным **Argentina Oil and Gas Dataset** (Kaggle).
Часть дипломного проекта «Прогнозирование и геовизуализация показателей добычи нефти и газа».

## Что внутри

| Папка | Содержимое |
|---|---|
| `Models/` | 3 класса с бизнес-логикой: `Production`, `BasinProduction`, `DrillingActivity` + справочник `Basins` (координаты бассейнов) |
| `Controllers/` | CRUD для каждой сущности, 5 сводных отчётов, авторизация |
| `Services/ReportService.cs` | Запросы на объединение данных (LINQ: `Where`, `Select`, `Join`, `GroupJoin`, `GroupBy`) |
| `Data/` | `AppDbContext` (EF Core + SQLite), разбор CSV, первичная загрузка |
| `Dtos/` | Входные модели с валидацией |
| `Auth/` | JWT-токены, роли `admin` / `user` |
| `postman/` | Коллекция Postman: 34 запроса с автотестами |

### Модель данных

Одна строка CSV (месяц) раскладывается на три сущности:

- **Production**: национальная добыча нефти и газа (`petroleo_*`, `gas_*`).
  Методы: доля нетрадиционной добычи, пересчёт итогов, проверка согласованности, прирост.
- **BasinProduction**: добыча по бассейну (колонки `cuenca_*` развёрнуты в формат «дата / бассейн / значение»).
  Методы: доля бассейна, изменение к прошлому периоду, признак доминирующего бассейна, справка с координатами.
- **DrillingActivity**: цена Brent, метры бурения, законченные и активные скважины.
  Методы: проходка на скважину, категория цены (Low / Medium / High), наличие бурения.

### Права доступа

| Действие | Без токена | `user` | `admin` |
|---|---|---|---|
| GET данных (`/api/production`, `/api/basins`, `/api/drilling`) | да | да | да |
| Сводные отчёты (`/api/reports/*`) | **401** | да | да |
| POST / PUT / DELETE | **401** | **403** | да |

Демо-пользователи (в `appsettings.json`): `admin / admin123`, `user / user123`.

### Сводные отчёты (объединение классов)

| Запрос | Что объединяет |
|---|---|
| `GET /api/reports/monthly-summary?from=&to=` | Production + DrillingActivity по месяцу |
| `GET /api/reports/basin-shares?date=2023-06-01` | BasinProduction + Production + справочник бассейнов |
| `GET /api/reports/yearly` | Годовые группы Production + DrillingActivity, прирост год к году |
| `GET /api/reports/price-impact` | Production + DrillingActivity, сгруппировано по категории цены Brent |
| `GET /api/reports/top-basins?year=2023` | Рейтинг бассейнов за год с координатами (основа для карты) |

## Запуск на macOS

1. Установить **.NET 10 SDK**: https://dotnet.microsoft.com/download (pkg-установщик для macOS, для M-чипов выбрать Arm64).
   Проверка: `dotnet --version`.
2. В VS Code поставить расширение **C# Dev Kit**.
3. Скачать датасет с Kaggle (pedronaon/argentina-oil-and-gas-dataset), положить CSV в `Data/` и назвать `argentina_oil_gas.csv`.
4. В терминале в папке проекта:
   ```bash
   dotnet restore
   dotnet run
   ```
   API поднимется на `http://localhost:5080`. При первом запуске создастся `oilgas.db` и загрузятся данные из CSV
   (в логе будет строка «Импортировано: ...»).
5. Postman: Import → `postman/OilGasApi.postman_collection.json`. Сначала выполнить папку **1. Auth**
   (токены сохранятся в переменные коллекции), потом остальное. Либо Run collection целиком.

Чтобы перезагрузить данные из CSV, удали `oilgas.db` и запусти заново.

## Релиз-версия

```bash
dotnet publish -c Release -o publish
cd publish
dotnet OilGasApi.dll --urls http://localhost:5080
```

CSV из `Data/` копируется в релиз автоматически.

## Релиз на GitHub

```bash
git tag v1.0
git push origin v1.0
```
Затем на GitHub: Releases → Draft a new release → выбрать тег `v1.0` → приложить архив папки `publish`.

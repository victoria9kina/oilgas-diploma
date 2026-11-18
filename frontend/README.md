# OilGas Diploma Frontend

Клиентская часть учебного проекта для просмотра и изменения данных о добыче нефти и газа в Аргентине. Frontend разработан на Angular и работает с готовым ASP.NET Core Web API.

## Требования

- Node.js 22 или новее
- npm
- .NET SDK для запуска backend

## Запуск backend

Из корня проекта выполни:

```bash
cd backend
dotnet run
```

Backend API будет доступен по адресу:

```text
http://localhost:5080
```

## Установка frontend

В другом окне терминала из корня проекта выполни:

```bash
cd frontend
npm ci
```

## Запуск frontend

Frontend запускается в отдельном терминале после запуска backend. Во время работы frontend backend должен оставаться запущенным.

```bash
npm start
```

После запуска открой в браузере:

```text
http://localhost:4200
```

## Тестовые учётные записи

Администратор:

```text
Логин: admin
Пароль: admin123
```

Обычный пользователь:

```text
Логин: user
Пароль: user123
```

Пользователь `admin` может просматривать, добавлять и редактировать данные. Пользователь `user` может только просматривать данные.

## Автоматические тесты

```bash
npm test -- --watch=false
```

## Production build

```bash
npm run build
```

Результат production build создаётся в папке `dist`.

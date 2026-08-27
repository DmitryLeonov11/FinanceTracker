# Finance Tracker

Личный финансовый трекер: учёт счетов, операций, бюджетов и целей в нескольких валютах (BYN, USD, EUR, RUB). Интерфейс на русском, обновления приходят в реальном времени через SignalR, работает как PWA и не разваливается без интернета.

## Архитектура

Backend на .NET 8, собран по Clean Architecture: Domain, Application, Infrastructure и API разложены по отдельным слоям. Бизнес-логика идёт через CQRS на MediatR, данные хранятся в PostgreSQL через EF Core 8. Авторизация на JWT с ротацией refresh-токенов, события пользователю прилетают через SignalR-хаб, валидация на FluentValidation, а ошибки в формате ProblemDetails возвращаются по-русски.

Frontend на Vue 3 и TypeScript, собран Vite 5 по методологии Feature-Sliced Design. Стили на Tailwind v4 с OKLCH-токенами и тремя темами: светлой, тёмной и системной. Состояние живёт в Pinia и TanStack Query, а axios сам обновляет токен и подставляет Idempotency-Key в каждый запрос. PWA собран через Workbox, иконки свои, шрифты Inter и JetBrains Mono подключены через Google Fonts.

```text
Browser ──► nginx :5173 ──┬─► /api    → api :8080
                          ├─► /hubs/* → api :8080 (WebSocket)
                          └─► / → SPA bundle (Vue)
                                                 │
                              api :8080 ──► postgres :5432
```

## Структура репозитория

```text
FinanceTracker/
├── docker-compose.yml              postgres + migrator + api + web
├── .env / .env.example             все переменные окружения
├── global.json                     SDK pin для локальной разработки
├── FinanceTracker.slnx             solution в новом slnx-формате
├── src/                            backend
│   ├── FinanceTracker.Domain/      сущности, value objects, события
│   ├── FinanceTracker.Application/ CQRS (commands · queries · handlers · validators)
│   ├── FinanceTracker.Infrastructure/  EF Core, Identity, JWT, миграции
│   └── FinanceTracker.Api/         контроллеры, SignalR-хаб, middleware, Program.cs
└── web/                            frontend
    ├── Dockerfile + nginx.conf     production-образ (nginx static + proxy)
    ├── public/                     иконки, манифест PWA
    └── src/
        ├── app/                    bootstrap, router, layouts, plugins, styles
        ├── pages/                  view-страницы по роутам
        ├── widgets/                композитные блоки (Dashboard, Accounts grid)
        ├── features/               действия пользователя (create-account и т.д.)
        ├── entities/               account, transaction, dashboard, money, user
        └── shared/                 ui-kit, api-клиент, stores, lib, i18n
```

## Стек

| Слой | Технологии |
| --- | --- |
| Backend | .NET 8 · ASP.NET Core · MediatR · FluentValidation · EF Core · Npgsql · BCrypt · JWT · SignalR |
| База | PostgreSQL 16 |
| Frontend | Vue 3 · TypeScript · Vite · Tailwind v4 · Pinia · Vue Router · TanStack Query · axios · Zod · Vee-Validate · Radix Vue · ECharts · motion-v · vue-sonner |
| Infra | Docker · docker-compose · nginx · Workbox PWA |
| Realtime | @microsoft/signalr (WebSocket) |
| Observability (опц.) | Sentry (через `VITE_SENTRY_DSN`) |

## Быстрый старт

Нужны только Docker Desktop и Compose v2.

```powershell
# 1. Скопировать env-шаблон (можно пропустить: .env уже лежит с dev-значениями)
Copy-Item .env.example .env

# 2. Поднять весь стек
docker compose up -d

# 3. Открыть приложение
start http://localhost:5173
```

Дальше происходит вот что:

1. `postgres` стартует и проходит healthcheck.
2. `migrator` (самодостаточный EF-бандл) накатывает миграции и завершает работу.
3. `api` стартует, дождавшись здорового postgres и завершённого migrator.
4. `web` (nginx со статикой) стартует и начинает проксировать `/api` и `/hubs` на api.

Остановить:

```powershell
docker compose down            # данные останутся
docker compose down -v         # удалит том postgres вместе с данными
```

## Доступные адреса

| URL | Что |
| --- | --- |
| `http://localhost:5173/` | Frontend SPA |
| `http://localhost:5173/api/...` | API через nginx-прокси |
| `http://localhost:5173/hubs/user` | SignalR-хаб |
| `http://localhost:5050/swagger` | Swagger UI (dev) |
| `http://localhost:5050/api/...` | API напрямую |
| `postgres://localhost:5433` | БД (postgres / postgres) |

## Переменные окружения

Все настройки лежат в `.env`. В репозитории уже есть dev-значения, для продакшена их нужно поменять:

| Переменная | По умолчанию | Назначение |
| --- | --- | --- |
| `POSTGRES_DB` / `_USER` / `_PASSWORD` | financetracker / postgres / postgres | креды БД |
| `POSTGRES_HOST_PORT` | 5433 | внешний порт БД (внутренний всегда 5432) |
| `API_HOST_PORT` | 5050 | внешний порт API |
| `WEB_HOST_PORT` | 5173 | внешний порт фронта |
| `JWT_SIGNING_KEY` | (dev-ключ) | **обязательно ≥32 символов**, в проде заменить |
| `JWT_ACCESS_MINUTES` / `_REFRESH_DAYS` | 15 / 14 | TTL токенов |
| `CORS_ORIGIN` | `http://localhost:5173` | разрешённый origin для cross-origin вызовов |
| `ALLOWED_HOSTS` | `localhost;127.0.0.1;api;web` | Host-header allowlist |
| `VITE_API_BASE_URL` | `/api` | base URL, запекается в SPA-бандл |
| `VITE_SENTRY_DSN` | (пусто) | если задать, включит `@sentry/vue` |

## Локальная разработка (без docker)

Нужен hot-reload и для бэка, и для фронта сразу? Поднимите БД в docker, а остальное запускайте локально:

```powershell
# 1. Только БД
docker compose up -d postgres

# 2. Применить миграции
dotnet ef database update -p src/FinanceTracker.Infrastructure -s src/FinanceTracker.Api

# 3. Запустить API
dotnet run --project src/FinanceTracker.Api

# 4. Отдельное окно: фронт
cd web
npm install
npm run dev
```

Vite здесь проксирует `/api` и `/hubs` на `localhost:5050`, настройка лежит в `web/vite.config.ts`.

## Миграции

```powershell
# Создать новую миграцию
dotnet ef migrations add <Name> -p src/FinanceTracker.Infrastructure -s src/FinanceTracker.Api -o Persistence/Migrations

# Применить (вне docker)
dotnet ef database update -p src/FinanceTracker.Infrastructure -s src/FinanceTracker.Api

# Откатить на N миграций назад
dotnet ef database update <PreviousMigrationName> -p src/FinanceTracker.Infrastructure -s src/FinanceTracker.Api

# Снять все
dotnet ef database update 0 -p src/FinanceTracker.Infrastructure -s src/FinanceTracker.Api
```

В docker-compose миграции автоматически накатывает сервис `migrator`, самодостаточный EF-бандл. Его сборка описана в `src/FinanceTracker.Api/Dockerfile.migrator`.

## Тесты

```powershell
# Backend (когда тесты появятся в tests/)
dotnet test

# Frontend
cd web
npm run typecheck      # vue-tsc strict
npm run test           # Vitest (jsdom)
```

## Безопасность

- Пароли хранятся через BCrypt с work factor 12.
- JWT подписан HS256, ключ не короче 32 символов, проверяются iss/aud/exp/nbf.
- Refresh-токены ротируются на каждый refresh, в базе хранится только их SHA-256 хэш.
- Все мутации идут с `Idempotency-Key`, чтобы случайный повторный POST не выполнился дважды.
- Ответы ProblemDetails локализованы на русский.
- На фронте: silent refresh запускается только одним потоком за раз, ответы проходят через Zod-парсинг, JWT живёт в памяти и в localStorage с автовосстановлением.
- В прод-сборке web-контейнера nginx отдаёт заголовки `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`.

## Лицензия

Личный проект. Лицензия на усмотрение автора.

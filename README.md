# Finance Tracker

Личный финансовый трекер: учёт счетов, операций, бюджетов и целей в нескольких валютах (BYN / USD / EUR / RUB). Интерфейс на русском, real-time обновления через SignalR, PWA с офлайн-режимом.

## Архитектура

**Backend** — .NET 8, Clean Architecture (Domain · Application · Infrastructure · API), CQRS на MediatR, EF Core 8 + PostgreSQL, JWT с rotation refresh-токенов, SignalR-хаб для пользовательских событий, FluentValidation, локализация ProblemDetails на русский.

**Frontend** — Vue 3 + TypeScript, Vite 5, Feature-Sliced Design, Tailwind v4 c OKLCH-токенами и тройной темой (light / dark / system), Pinia + TanStack Query, axios с silent refresh и Idempotency-Key, PWA через Workbox, кастомный SVG icon-set, шрифты Inter + JetBrains Mono через Google Fonts CDN.

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
# 1. Скопировать env-шаблон (можно пропустить — .env уже лежит с дев-значениями)
Copy-Item .env.example .env

# 2. Поднять весь стек
docker compose up -d

# 3. Открыть приложение
start http://localhost:5173
```

Что произойдёт:

1. `postgres` стартует и проходит healthcheck.
2. `migrator` (self-contained EF bundle) применяет миграции и завершается.
3. `api` стартует, ждёт healthy postgres + completed migrator.
4. `web` (nginx + статика) стартует и начинает проксировать `/api` и `/hubs` на api.

Остановить:

```powershell
docker compose down            # сохранит данные
docker compose down -v         # сотрёт том postgres
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

Все настройки — в `.env` (в репозитории дев-значения; для прода переопределить):

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
| `VITE_SENTRY_DSN` | (пусто) | если задан — включается `@sentry/vue` |

## Локальная разработка (без docker)

Если хочется hot-reload бэка и фронта одновременно — поднимаем БД в docker, остальное локально:

```powershell
# 1. Только БД
docker compose up -d postgres

# 2. Применить миграции
dotnet ef database update -p src/FinanceTracker.Infrastructure -s src/FinanceTracker.Api

# 3. Запустить API
dotnet run --project src/FinanceTracker.Api

# 4. В отдельном окне — фронт
cd web
npm install
npm run dev
```

Vite при этом проксирует `/api` и `/hubs` на `localhost:5050` (см. `web/vite.config.ts`).

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

В docker-compose миграции применяются автоматически сервисом `migrator` (self-contained EF bundle, см. `src/FinanceTracker.Api/Dockerfile.migrator`).

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

- Пароли — BCrypt с work factor 12.
- JWT подписан HS256, ключ ≥ 32 символов, валидация iss/aud/exp/nbf.
- Refresh-токены ротируются на каждый refresh, в БД лежит SHA-256 хэш (не raw).
- Все мутации идут с `Idempotency-Key` (защита от двойных POST).
- ProblemDetails-ответы локализованы на русский.
- На фронте: silent refresh single-flight, Zod-парсинг ответов, безопасные cookie не используются (JWT в памяти + localStorage с auto-recovery).
- В прод-сборке web-контейнера nginx отдаёт CSP-friendly headers: `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`.

## Лицензия

Личный проект. Лицензия на усмотрение автора.

FROM node:22-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci --no-audit --no-fund

FROM node:22-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ARG VITE_API_BASE_URL=/api
ARG VITE_SENTRY_DSN=""
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_SENTRY_DSN=$VITE_SENTRY_DSN
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

HEALTHCHECK --interval=10s --timeout=3s --retries=5 \
  CMD wget -q --spider http://localhost/index.html || exit 1

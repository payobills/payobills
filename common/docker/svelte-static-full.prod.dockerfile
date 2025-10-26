ARG NODE_VERSION=20.11.1

FROM node:22.19.0-alpine AS build-env
WORKDIR /app

COPY src/package*.json ./

RUN npm ci

COPY src ./

RUN npm run build:static

FROM nginx:alpine

RUN rm -rf /usr/share/nginx/html/*

COPY --from=build-env /app/build /usr/share/nginx/html

COPY --from=build-env /app/nginx.conf /etc/nginx/conf.d/default.conf

WORKDIR /app
COPY --from=build-env /app/entrypoint.sh /app/entrypoint.sh

EXPOSE 80

CMD ["sh", "/app/entrypoint.sh"]

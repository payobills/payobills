ARG NODE_VERSION=20.11.1

FROM node:${NODE_VERSION}-alpine AS build-env
WORKDIR /app

COPY src/package*.json ./

RUN npm ci

COPY src ./

RUN rm -rfv ./src/routes/*

COPY src/src/routes/+layout* ./src/routes
COPY src/src/routes/lite/ ./src/routes/

RUN npm run build:static

FROM busybox:1.37.0

WORKDIR /app

COPY --from=build-env /app/build /app/build

ENTRYPOINT ["tail", "-f", "/dev/null"]

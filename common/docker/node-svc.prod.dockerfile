ARG NODE_VERSION

FROM node:${NODE_VERSION}-alpine AS build-env
WORKDIR /app

COPY src/package*.json ./

RUN npm ci

COPY src ./

RUN npm run build

FROM node:${NODE_VERSION}-alpine

WORKDIR /app

ENV NODE_ENV=production

COPY src/package*.json ./

RUN npm ci

COPY --from=build-env /app/dist /app/dist

EXPOSE 80

ENV HOST=0.0.0.0
ENV PORT=80

ENTRYPOINT node /app/dist/index.js

ARG NODE_VERSION

FROM  --platform=$BUILDPLATFORM node:${NODE_VERSION}-alpine AS build-env
WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY src ./src

RUN npm run build

# RUN ls /app && exit 1

FROM  --platform=$BUILDPLATFORM node:${NODE_VERSION}-alpine AS finalCodeEnv

WORKDIR /app

ENV NODE_ENV=production

COPY package*.json ./

RUN npm ci

COPY --from=build-env /app/dist /app/dist

EXPOSE 80

FROM  --platform=$BUILDPLATFORM alpine:3.20.3

RUN apk add nodejs && rm -rf /var/cache/apk/*

COPY --from=finalCodeEnv /app .

ENV HOST=0.0.0.0
ENV PORT=80

ENTRYPOINT node /dist/index.js

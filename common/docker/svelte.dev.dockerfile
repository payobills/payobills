ARG NODE_VERSION

FROM node:${NODE_VERSION}-alpine AS build-env

WORKDIR /app

ENV HOST=0.0.0.0
ENV PORT=80

CMD npm i && npm run dev -- --host --port $PORT

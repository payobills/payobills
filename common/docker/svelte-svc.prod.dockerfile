ARG NODE_VERSION=18.12.1

FROM node:${NODE_VERSION}-alpine AS build-env

WORKDIR /app

ARG WORKSPACE
COPY ${WORKSPACE}/ .
# COPY ${WORKSPACE}/package*.json ./

# RUN npm ci

# COPY ${WORKSPACE}/ .

# RUN ls

# RUN npm run build

# ARG NODE_VERSION
# FROM node:${NODE_VERSION}-alpine

# WORKDIR /app

# ENV NODE_ENV=production

# ARG WORKSPACE
# COPY ${WORKSPACE}/package*.json ./

# RUN npm ci

# COPY --from=build-env /app/build /app/build

# EXPOSE 80

# ENV HOST=0.0.0.0
# ENV PORT=80

# ENTRYPOINT node /app/build/index.js

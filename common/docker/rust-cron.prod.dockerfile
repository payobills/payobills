ARG RUST_VERSION

FROM --platform=$TARGETPLATFORM rust:${RUST_VERSION} AS builder

ARG TARGETPLATFORM

WORKDIR /app

COPY Cargo.toml Cargo.lock ./

RUN cargo fetch

RUN case "${TARGETPLATFORM}" in \
        "linux/amd64")  TARGET_TRIPLE="x86_64-unknown-linux-gnu" ;; \
        "linux/arm64")  TARGET_TRIPLE="aarch64-unknown-linux-gnu" ;; \
        *) echo "Unsupported platform: $TARGETPLATFORM" && exit 1 ;; \
    esac && \
    rustup target add "${TARGET_TRIPLE}"

COPY . .

RUN case "${TARGETPLATFORM}" in \
    "linux/amd64")  TARGET_TRIPLE="x86_64-unknown-linux-gnu" ;; \
    "linux/arm64")  TARGET_TRIPLE="aarch64-unknown-linux-gnu" ;; \
    *) echo "Unsupported platform: $TARGETPLATFORM" && exit 1 ;; \
    esac && \
    cargo build --release --target "${TARGET_TRIPLE}"

# Ready release
FROM --platform=$TARGETPLATFORM ubuntu:latest

ARG APP
ARG TARGETPLATFORM

WORKDIR /app/tmp

RUN apt-get update -y && \
    apt-get install ca-certificates -y && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/archives/*

COPY --from=builder /app/target .

RUN case "${TARGETPLATFORM}" in \
        "linux/amd64")  TARGET_TRIPLE="x86_64-unknown-linux-gnu" ;; \
        "linux/arm64")  TARGET_TRIPLE="aarch64-unknown-linux-gnu" ;; \
        *) echo "Unsupported platform: $TARGETPLATFORM" && exit 1 ;; \
    esac && \
    cp ./${TARGET_TRIPLE}/release/${APP} /app/ && \
    rm -rf /app/tmp && \
    chmod +x /app/${APP}

WORKDIR /app

ARG APP
ENV APP=${APP}
ENTRYPOINT ./${APP}

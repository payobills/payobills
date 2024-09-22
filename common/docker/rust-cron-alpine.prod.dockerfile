ARG RUST_VERSION

FROM --platform=$TARGETPLATFORM rust:${RUST_VERSION} AS builder

ARG APP
ARG TARGETPLATFORM

WORKDIR /app

COPY Cargo.toml Cargo.lock ./

RUN cargo fetch

RUN case "${TARGETPLATFORM}" in \
        "linux/amd64")  TARGET_TRIPLE="x86_64-unknown-linux-musl" ;; \
        "linux/arm64")  TARGET_TRIPLE="aarch64-unknown-linux-musl" ;; \
        *) echo "Unsupported platform: $TARGETPLATFORM" && exit 1 ;; \
    esac && \
    # apt-get update -y && apt-get install glibc ca-certificates gcc libssl-dev -y && \
    apk add --no-cache musl-dev openssl-dev 
    # && \
    # rm -rf /var/lib/apt/lists/* /var/cache/apt/archives/* && \
    # rustup target add "${TARGET_TRIPLE}"

ENV RUSTFLAGS="-C target-feature=-crt-static"

COPY . .

RUN case "${TARGETPLATFORM}" in \
    "linux/amd64")  TARGET_TRIPLE="x86_64-unknown-linux-musl" ;; \
    "linux/arm64")  TARGET_TRIPLE="aarch64-unknown-linux-musl" ;; \
    *) echo "Unsupported platform: $TARGETPLATFORM" && exit 1 ;; \
    esac && \
    cargo build --release --target "${TARGET_TRIPLE}" && \
    mkdir /app/build && cp ./target/${TARGET_TRIPLE}/release/${APP} /app/build

# Ready release
FROM --platform=$TARGETPLATFORM alpine:3.20.3

ARG TARGETPLATFORM

WORKDIR /app

RUN apk add ca-certificates && \
    apk cache clean

COPY --from=builder /app/build .

RUN case "${TARGETPLATFORM}" in \
        "linux/amd64")  TARGET_TRIPLE="x86_64-unknown-linux-musl" ;; \
        "linux/arm64")  TARGET_TRIPLE="aarch64-unknown-linux-musl" ;; \
        *) echo "Unsupported platform: $TARGETPLATFORM" && exit 1 ;; \
    esac && \
    apk add libgcc && rm -rf /var/cache/apk/* && \
    chmod +x /app/${APP}

ARG APP
ENV APP=${APP}
ENTRYPOINT ./${APP}

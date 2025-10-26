#!/bin/sh
set -eu

# SECTION: Generate the env json file from INJECTED_* env vars
APP_DIR=/usr/share/nginx/html
OUT_FILE="$APP_DIR/env.json"

echo "[env] Generating $OUT_FILE from INJECTED_* env vars..."
mkdir -p "$APP_DIR"

# Start JSON
printf "{" > "$OUT_FILE"

first=1

# Iterate over environment variables starting with INJECTED_
# Using 'env' is portable and simple
env | grep '^INJECTED_' | while IFS='=' read -r key value; do
  # Escape backslashes, double quotes, and newlines in the value
  safe_value=$(printf '%s' "$value" | sed -e 's/\\/\\\\/g' -e 's/"/\\"/g' -e ':a;N;s/\n/\\n/g;ta')

  if [ $first -eq 0 ]; then
    printf "," >> "$OUT_FILE"
  fi
  first=0

  # Write key and escaped value (keep the INJECTED_ prefix on the key)
  printf '\n  "%s": "%s"' "$key" "$safe_value" >> "$OUT_FILE"
done

# Close JSON (ensure there's a newline before closing brace)
if [ $first -eq 0 ]; then
  printf "\n" >> "$OUT_FILE"
fi
printf "}\n" >> "$OUT_FILE"

echo "[env] Generated $OUT_FILE:"
cat "$OUT_FILE"

# SECTION: start the nginx server

nginx -g "daemon off;"

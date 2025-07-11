## Stage 1: Build the app
FROM oven/bun:1.2.18-alpine AS build

WORKDIR /app

# Install dependencies
COPY package.json bun.lockb* ./
RUN bun install --frozen-lockfile

# Copy source code
COPY . .

# Build the app
RUN bun run build

## Stage 2: Serve app with httpd server
FROM httpd:2.4.63-alpine3.22

# Copy built app to serve
COPY --from=build /app/dist /usr/local/apache2/htdocs

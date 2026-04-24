FROM node@sha256:c69f4e0640e5b065f2694579793e4309f1e0e49868b0f2fea29c44d9c0dc2caf AS base

# Check https://github.com/nodejs/docker-node/tree/main#nodealpine to understand why libc6-compat might be needed
ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN corepack enable && corepack prepare pnpm@10.30.3 --activate
WORKDIR /app

# ---
FROM base AS prepare
RUN pnpm install -g turbo@2.9.4
COPY --chmod=555 . .
# Generate a partial monorepo with a pruned lockfile for a target workspace
# Assuming "@bookworm/backoffice" is the name in the project's package.json
RUN turbo prune @bookworm/backoffice --docker

# ---
FROM base AS builder

# First install the dependencies (as they change less often)
COPY --from=prepare --chmod=555 /app/out/json/ .
RUN pnpm install --frozen-lockfile --ignore-scripts

# Build the project
COPY --from=prepare --chmod=555 /app/out/full/ .

# Use build secrets to enable remote caching
ARG TURBO_TEAM
ENV TURBO_TEAM=$TURBO_TEAM

RUN --mount=type=secret,id=TURBO_TOKEN \
    TURBO_TOKEN=$(cat /run/secrets/TURBO_TOKEN 2>/dev/null) \
    pnpm turbo build --filter=@bookworm/backoffice

# ---
FROM node@sha256:c69f4e0640e5b065f2694579793e4309f1e0e49868b0f2fea29c44d9c0dc2caf AS runner
WORKDIR /app

# Apply latest Alpine security updates, remove npm tooling from runtime image,
# and create a non-root user for production.
RUN apk upgrade --no-cache && \
    rm -rf /usr/local/lib/node_modules/npm && \
    rm -f /usr/local/bin/npm /usr/local/bin/npx && \
    addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nextjs

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

USER nextjs

# Automatically leverage output traces to reduce image size
# https://nextjs.org/docs/advanced-features/output-file-tracing
COPY --from=builder --chown=nextjs:nodejs --chmod=555 /app/apps/backoffice/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs --chmod=555 /app/apps/backoffice/.next/static ./apps/backoffice/.next/static
COPY --from=builder --chown=nextjs:nodejs --chmod=555 /app/apps/backoffice/public ./apps/backoffice/public

EXPOSE 3000

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD node -e "require('http').get('http://localhost:3000/api/health', (r) => {process.exit(r.statusCode === 200 ? 0 : 1)})" || exit 1

CMD ["node", "apps/backoffice/server.js"]

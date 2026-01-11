ARG NODE_VERSION=25.0.0
FROM node:${NODE_VERSION}-slim AS base

# Check https://github.com/nodejs/docker-node/tree/main#nodealpine to understand why libc6-compat might be needed
RUN corepack enable pnpm && corepack install -g pnpm@10.28.0
WORKDIR /app

# ---
FROM base AS prepare
RUN pnpm install -g turbo@^2
COPY . .
# Generate a partial monorepo with a pruned lockfile for a target workspace
# Assuming "@bookworm/backoffice" is the name in the project's package.json
RUN turbo prune @bookworm/backoffice --docker

# ---
FROM base AS builder

# First install the dependencies (as they change less often)
COPY --from=prepare /app/out/json/ .
RUN pnpm install --frozen-lockfile

# Build the project
COPY --from=prepare /app/out/full/ .

# Uncomment and use build args to enable remote caching
# ARG TURBO_TEAM
# ENV TURBO_TEAM=$TURBO_TEAM
# ARG TURBO_TOKEN
# ENV TURBO_TOKEN=$TURBO_TOKEN

RUN pnpm turbo build --filter=@bookworm/backoffice

# ---
FROM node:${NODE_VERSION}-alpine AS runner
WORKDIR /app

# Don't run production as root for security reasons
RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nextjs

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

USER nextjs

# Automatically leverage output traces to reduce image size
# https://nextjs.org/docs/advanced-features/output-file-tracing
COPY --from=builder --chown=nextjs:nodejs /app/apps/backoffice/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/apps/backoffice/.next/static ./apps/backoffice/.next/static
COPY --from=builder --chown=nextjs:nodejs /app/apps/backoffice/public ./apps/backoffice/public

EXPOSE 3000

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD node -e "require('http').get('http://localhost:3000/api/health', (r) => {process.exit(r.statusCode === 200 ? 0 : 1)})" || exit 1

CMD ["node", "apps/backoffice/server.js"]

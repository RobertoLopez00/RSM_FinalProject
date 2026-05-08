# Build stage
FROM node:22-alpine AS build

WORKDIR /app

# Install pnpm
RUN npm install -g pnpm

# Copy the entire source code first (so quasar.config.js exists for postinstall scripts)
COPY . .

# Install dependencies
RUN pnpm install

# Build the application
RUN pnpm run build

# Production stage - serve with nginx
FROM nginx:alpine

# Copy built files to nginx
COPY --from=build /app/dist/spa /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:80 || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]

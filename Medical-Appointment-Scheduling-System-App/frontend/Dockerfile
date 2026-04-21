FROM node:20 AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

ARG BUILD_CONFIGURATION=production

RUN npm run build -- --configuration=$BUILD_CONFIGURATION

FROM nginxinc/nginx-unprivileged:alpine

USER root

WORKDIR /usr/share/nginx/html

RUN rm -rf ./*

COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

RUN chown -R nginx:nginx /usr/share/nginx/html /etc/nginx/conf.d

USER nginx

EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]
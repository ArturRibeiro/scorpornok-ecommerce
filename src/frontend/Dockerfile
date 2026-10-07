# Imagem do front-end. O Vite gera arquivos estáticos (dist/), servidos pelo
# nginx; não há servidor Node em produção.
FROM node:24-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
# A URL é embutida no JavaScript durante o build e usada pelo navegador,
# por isso aponta para a porta publicada no host, não para o container.
ARG VITE_CATALOG_API_URL=http://localhost:5064
ENV VITE_CATALOG_API_URL=$VITE_CATALOG_API_URL
RUN npm run build

FROM nginx:1.29-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80

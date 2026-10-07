import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) },
  },
  // Mesma porta do container (docker-compose.frontend.yml), que é a origem
  // liberada no CORS do Catalog.Web.Api.
  server: { port: 3000, strictPort: true },
  preview: { port: 3000, strictPort: true },
});

import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Gera .next/standalone, usado pela imagem Docker (veja o Dockerfile).
  output: "standalone",
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "unsplash.com",
      },
      {
        protocol: "https",
        hostname: "images.unsplash.com",
      },
    ],
  },
};

export default nextConfig;

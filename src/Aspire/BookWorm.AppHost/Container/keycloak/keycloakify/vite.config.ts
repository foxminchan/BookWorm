import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { keycloakify } from "keycloakify/vite-plugin";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    keycloakify({
      themeName: "bookworm",
      themeVersion: "1.0.0",
      groupId: "com.foxminchan.bookworm.keycloak",
      artifactId: "keycloak-theme-bookworm",
      accountThemeImplementation: "none",
      keycloakifyBuildDirPath: "../themes",
    }),
  ],
});

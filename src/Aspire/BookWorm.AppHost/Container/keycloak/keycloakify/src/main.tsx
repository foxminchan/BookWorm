import { createRoot } from "react-dom/client";
import { StrictMode } from "react";
import { KcPage } from "./kc.gen";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    {(globalThis as any).kcContext ? (
      <KcPage kcContext={(globalThis as any).kcContext} />
    ) : (
      <h1>No Keycloak Context</h1>
    )}
  </StrictMode>
);

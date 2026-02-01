import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { KcPage, type KcContext } from "./kc.gen";

interface KcGlobal {
  kcContext?: KcContext;
}

const global = globalThis as unknown as KcGlobal;

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    {global.kcContext ? (
      <KcPage kcContext={global.kcContext} />
    ) : (
      <h1>No Keycloak Context</h1>
    )}
  </StrictMode>
);

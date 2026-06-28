import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { App } from "./App";
import { loadConfig } from "./config";
import { initOidc } from "./oidc";
import "./index.css";

async function bootstrap() {
  // Load runtime config and start OIDC before rendering so the auth gate and
  // any login redirect callback are handled with the correct issuer.
  await loadConfig();
  initOidc();

  createRoot(document.getElementById("root")!).render(
    <StrictMode>
      <App />
    </StrictMode>,
  );
}

void bootstrap();

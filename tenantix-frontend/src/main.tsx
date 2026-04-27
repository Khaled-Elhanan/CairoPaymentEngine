import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { App } from "./App";
import "./index.css";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <BrowserRouter>
      <App />
      <Toaster
        position="top-right"
        toastOptions={{
          style: {
            background: "rgba(255,255,255,0.95)",
            color: "#111827",
            border: "1px solid #e5e7eb",
            borderRadius: "16px",
            boxShadow: "0 12px 30px rgba(15,23,42,0.12)",
            backdropFilter: "blur(6px)",
            fontWeight: "600",
          },
        }}
      />
    </BrowserRouter>
  </React.StrictMode>,
);

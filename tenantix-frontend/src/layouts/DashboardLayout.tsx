import { Outlet } from "react-router-dom";
import { env } from "../utils/env";

function getEnvironmentBadge() {
  const configured = env.apiBaseUrl.toLowerCase();
  if (configured.includes("localhost") || configured.includes("127.0.0.1")) {
    return "Local";
  }
  if (configured.includes("dev") || configured.includes("staging")) {
    return "Dev";
  }
  return "Prod";
}

export function DashboardLayout() {
  const environment = getEnvironmentBadge();

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-200 text-gray-900">
      <header className="sticky top-0 z-20 border-b border-gray-200/70 bg-white/70 px-4 py-4 backdrop-blur md:px-8">
        <div className="mx-auto flex w-full max-w-4xl items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold tracking-tight md:text-3xl">Cairo Payment Engine Dashboard</h1>
            <p className="mt-1 text-sm text-gray-600">Simulate a complete payment lifecycle using Stripe</p>
          </div>
          <span className="rounded-full border border-gray-200 bg-white px-3 py-1 text-xs font-semibold text-gray-600 shadow-sm">
            {environment}
          </span>
        </div>
      </header>

      <div className="mx-auto flex min-h-[calc(100vh-148px)] w-full max-w-4xl items-center justify-center px-4 py-8 md:px-8">
        <main className="w-full">
          <Outlet />
        </main>
      </div>

      <footer className="border-t border-gray-200/70 bg-white/70 px-4 py-4 text-xs text-gray-500 backdrop-blur md:px-8">
        <div className="mx-auto w-full max-w-4xl">
          Cairo Payment Engine - Fintech Operations Dashboard
        </div>
      </footer>
    </div>
  );
}

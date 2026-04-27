export const env = {
  // Prefer local HTTP endpoint by default to avoid dev-certificate trust issues.
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5273",
  paymobIframeId: import.meta.env.VITE_PAYMOB_IFRAME_ID ?? "913961",
};
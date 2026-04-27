const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
if (!apiBaseUrl) {
  throw new Error("Missing VITE_API_BASE_URL. Configure it in tenantix-frontend/.env");
}

export const env = {
  apiBaseUrl,
  paymobIframeId: import.meta.env.VITE_PAYMOB_IFRAME_ID ?? "913961",
};
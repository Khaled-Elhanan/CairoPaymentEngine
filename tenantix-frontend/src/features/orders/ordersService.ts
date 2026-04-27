import { apiClient } from "../../api/apiClient";

export type InitiatePaymentApiResponse = {
  orderId: string;
  externalId: string;
  gateway: string;
  paymentUrl?: string | null;
  message: string;
};

export async function createOrder(payload: Record<string, unknown>) {
  const { data } = await apiClient.post("/api/Orders", payload);
  return data;
}

export async function getOrderById(orderId: string) {
  const { data } = await apiClient.get(`/api/Orders/${orderId}`);
  return data;
}

export async function initiateOrderPayment(orderId: string, payload: Record<string, unknown>) {
  const { data } = await apiClient.post<InitiatePaymentApiResponse>(`/api/Orders/${orderId}/pay`, payload);
  return data;
}

export async function confirmStripePayment(payload: { externalId: string; eventId: string; gateway: "stripe" }) {
  const { data } = await apiClient.post("/api/Webhooks/stripe", payload);
  return data;
}

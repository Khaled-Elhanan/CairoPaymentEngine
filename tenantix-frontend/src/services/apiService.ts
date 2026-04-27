import { axiosClient } from "../api/axios";

export const apiService = {
  get: <T>(url: string) => axiosClient.get<T>(url).then((r) => r.data),
  post: <TResponse, TPayload>(url: string, payload: TPayload) =>
    axiosClient.post<TResponse>(url, payload).then((r) => r.data),
  put: <TResponse, TPayload>(url: string, payload: TPayload) =>
    axiosClient.put<TResponse>(url, payload).then((r) => r.data),
  delete: <T>(url: string) => axiosClient.delete<T>(url).then((r) => r.data),
};

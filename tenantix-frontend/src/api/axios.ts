import axios from "axios";
import { env } from "../utils/env";

export const axiosClient = axios.create({
  baseURL: env.apiBaseUrl,
  timeout: 30000,
  headers: {
    "Content-Type": "application/json",
  },
});

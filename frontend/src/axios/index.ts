import Axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL;

const axios = Axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 120_000, // 2 minutos
});

// Interceptor de request — sin auth, solo loguea en desarrollo
axios.interceptors.request.use((config) => {
  if (process.env.NODE_ENV === 'development') {
    console.log(`→ ${config.method?.toUpperCase()} ${config.url}`);
  }
  return config;
});

// Interceptor de response — maneja errores globales
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    if (process.env.NODE_ENV === 'development') {
      console.error('API Error:', error.response?.data);
    }
    return Promise.reject(error);
  }
);

export default axios;
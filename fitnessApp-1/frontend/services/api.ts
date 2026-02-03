import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { API_BASE_URL, API_TIMEOUT } from '../config/api';

const TOKEN_KEY = '@fitness_token';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use(
  async (config) => {
    const token = await AsyncStorage.getItem(TOKEN_KEY);
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (err) => Promise.reject(err)
);

api.interceptors.response.use(
  (res) => res,
  async (err) => {
    if (err.response?.status === 401) {
      await AsyncStorage.removeItem(TOKEN_KEY);
      await AsyncStorage.removeItem('@fitness_user');
    }
    return Promise.reject(err);
  }
);

export default api;

// Auth API (backend endpoint'leri arkadaşın belirleyecek)
export const authApi = {
  login: (email: string, password: string) =>
    api.post<{ token: string; user: { id: string; email: string; name?: string } }>('/auth/login', { email, password }),
  register: (email: string, password: string, name?: string) =>
    api.post<{ token: string; user: { id: string; email: string; name?: string } }>('/auth/register', { email, password, name }),
};

// Workout API
export const workoutApi = {
  getHistory: () => api.get('/workouts'),
  getWorkout: (id: string) => api.get(`/workouts/${id}`),
  startWorkout: (data?: { name?: string }) => api.post('/workouts', data),
  endWorkout: (id: string) => api.patch(`/workouts/${id}/end`),
};

// Stats API
export const statsApi = {
  getProgress: (params?: { from?: string; to?: string }) => api.get('/stats/progress', { params }),
};

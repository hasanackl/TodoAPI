/**
 * Backend API adresi - Arkadaşının backend'i çalıştığında buraya yazacaksın
 * Örnek: 'http://localhost:5000/api' veya 'https://api.fitnessapp.com'
 * Android emulator: 'http://10.0.2.2:5000/api'
 */
export const API_BASE_URL = __DEV__
  ? 'http://localhost:5000/api'
  : 'https://your-api.com/api';

export const API_TIMEOUT = 15000;

/** Backend hazır değilken ana uygulamayı test etmek için demo giriş kullanılsın mı? */
export const ENABLE_DEMO_LOGIN = __DEV__;

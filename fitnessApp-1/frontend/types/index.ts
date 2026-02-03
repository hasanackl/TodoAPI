export interface User {
  id: string;
  email: string;
  name?: string;
  createdAt?: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken?: string;
  expiresAt?: number;
}

export interface Workout {
  id: string;
  name: string;
  startedAt: string;
  endedAt?: string;
  durationMinutes?: number;
  exercises?: WorkoutExercise[];
}

export interface WorkoutExercise {
  id: string;
  name: string;
  sets?: number;
  reps?: number;
  weight?: number;
  durationSeconds?: number;
}

export interface Exercise {
  id: string;
  name: string;
  description?: string;
  videoUrl?: string;
  muscleGroup?: string;
}

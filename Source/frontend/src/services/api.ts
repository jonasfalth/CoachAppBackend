import axios from 'axios';
import { 
  LoginRequest, 
  LoginResponse, 
  SelectTeamRequest, 
  SelectTeamResponse,
  Player,
  Match,
  Position
} from '../types/api';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true, // Viktigt för JWT cookies
  headers: {
    'Content-Type': 'application/json',
  },
});

// Auth endpoints
export const authAPI = {
  validate: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post('/api/auth/validate', credentials);
    return response.data;
  },

  selectTeam: async (request: SelectTeamRequest): Promise<SelectTeamResponse> => {
    const response = await api.post('/api/auth/select-team', request);
    return response.data;
  },

  logout: async (): Promise<void> => {
    await api.post('/api/auth/logout');
  },
};

// Players endpoints
export const playersAPI = {
  getAll: async (): Promise<Player[]> => {
    const response = await api.get('/api/players');
    return response.data;
  },

  getById: async (id: number): Promise<Player> => {
    const response = await api.get(`/api/players/${id}`);
    return response.data;
  },

  create: async (player: Omit<Player, 'id'>): Promise<Player> => {
    const response = await api.post('/api/players', player);
    return response.data;
  },

  update: async (id: number, player: Player): Promise<Player> => {
    const response = await api.put(`/api/players/${id}`, player);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/api/players/${id}`);
  },
};

// Matches endpoints
export const matchesAPI = {
  getAll: async (): Promise<Match[]> => {
    const response = await api.get('/api/matches');
    return response.data;
  },

  getById: async (id: number): Promise<Match> => {
    const response = await api.get(`/api/matches/${id}`);
    return response.data;
  },

  create: async (match: Omit<Match, 'id'>): Promise<Match> => {
    const response = await api.post('/api/matches', match);
    return response.data;
  },

  update: async (id: number, match: Match): Promise<Match> => {
    const response = await api.put(`/api/matches/${id}`, match);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/api/matches/${id}`);
  },
};

// Positions endpoints
export const positionsAPI = {
  getAll: async (): Promise<Position[]> => {
    const response = await api.get('/api/positions');
    return response.data;
  },

  getById: async (id: number): Promise<Position> => {
    const response = await api.get(`/api/positions/${id}`);
    return response.data;
  },

  create: async (position: Omit<Position, 'id'>): Promise<Position> => {
    const response = await api.post('/api/positions', position);
    return response.data;
  },

  update: async (id: number, position: Position): Promise<Position> => {
    const response = await api.put(`/api/positions/${id}`, position);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/api/positions/${id}`);
  },
};

export default api; 
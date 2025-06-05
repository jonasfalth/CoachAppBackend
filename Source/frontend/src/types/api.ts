// Backend API types
export interface LoginRequest {
  username: string;
  password: string;
}

export interface UserDto {
  id: number;
  username: string;
  email: string;
}

export interface TeamDto {
  id: number;
  name: string;
  databaseName: string;
}

export interface LoginResponse {
  user: UserDto;
  teams: TeamDto[];
}

export interface SelectTeamRequest {
  teamId: number;
}

export interface SelectTeamResponse {
  team: TeamDto;
}

export interface Player {
  id: number;
  name: string;
  birthYear: number;
  jerseyNumber: number;
  profileImage?: string;
  positionId: number;
  notes?: string;
}

export interface Match {
  id: number;
  date: string;
  opponent: string;
  homeGame: boolean;
  result?: string;
  notes?: string;
}

export interface Position {
  id: number;
  name: string;
  abbreviation: string;
  category: string;
  description: string;
}

export interface AuthContextType {
  user: UserDto | null;
  selectedTeam: TeamDto | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<TeamDto[]>;
  selectTeam: (teamId: number) => Promise<void>;
  logout: () => Promise<void>;
} 
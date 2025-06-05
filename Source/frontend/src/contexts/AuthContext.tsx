import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { AuthContextType, UserDto, TeamDto } from '../types/api';
import { authAPI } from '../services/api';

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<UserDto | null>(null);
  const [selectedTeam, setSelectedTeam] = useState<TeamDto | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);

  const login = async (username: string, password: string): Promise<TeamDto[]> => {
    try {
      const response = await authAPI.validate({ username, password });
      setUser(response.user);
      setIsAuthenticated(true);
      return response.teams;
    } catch (error) {
      console.error('Inloggning misslyckades:', error);
      throw error;
    }
  };

  const selectTeam = async (teamId: number): Promise<void> => {
    try {
      const response = await authAPI.selectTeam({ teamId });
      setSelectedTeam(response.team);
    } catch (error) {
      console.error('Teamval misslyckades:', error);
      throw error;
    }
  };

  const logout = async (): Promise<void> => {
    try {
      await authAPI.logout();
    } catch (error) {
      console.error('Utloggning misslyckades:', error);
    } finally {
      setUser(null);
      setSelectedTeam(null);
      setIsAuthenticated(false);
    }
  };

  const value: AuthContextType = {
    user,
    selectedTeam,
    isAuthenticated,
    login,
    selectTeam,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth måste användas inom en AuthProvider');
  }
  return context;
}; 
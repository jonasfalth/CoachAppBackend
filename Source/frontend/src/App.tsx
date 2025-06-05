import React, { useState } from 'react';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { footballTheme } from './theme/footballTheme';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { TeamDto } from './types/api';
import Login from './components/Login';
import TeamSelector from './components/TeamSelector';
import Layout from './components/Layout';
import Players from './components/Players';
import Matches from './components/Matches';
import Positions from './components/Positions';
import Settings from './components/Settings';

type AppState = 'login' | 'team-selection' | 'authenticated';

const AppContent: React.FC = () => {
  const [appState, setAppState] = useState<AppState>('login');
  const [availableTeams, setAvailableTeams] = useState<TeamDto[]>([]);
  const [currentPath, setCurrentPath] = useState('/players');
  const { isAuthenticated, selectedTeam } = useAuth();

  const handleLoginSuccess = (teams: TeamDto[]) => {
    setAvailableTeams(teams);
    if (teams.length === 1) {
      // Om bara ett team finns, välj det automatiskt
      setAppState('authenticated');
    } else {
      setAppState('team-selection');
    }
  };

  const handleTeamSelected = () => {
    setAppState('authenticated');
  };

  const handleNavigation = (path: string) => {
    setCurrentPath(path);
  };

  const renderCurrentPage = () => {
    switch (currentPath) {
      case '/players':
        return <Players />;
      case '/matches':
        return <Matches />;
      case '/positions':
        return <Positions />;
      case '/settings':
        return <Settings />;
      default:
        return <Players />;
    }
  };

  // Om användaren loggar ut, återställ till login
  React.useEffect(() => {
    if (!isAuthenticated) {
      setAppState('login');
      setCurrentPath('/players');
    }
  }, [isAuthenticated]);

  if (appState === 'login') {
    return <Login onTeamSelect={handleLoginSuccess} />;
  }

  if (appState === 'team-selection') {
    return (
      <TeamSelector 
        teams={availableTeams} 
        onTeamSelected={handleTeamSelected} 
      />
    );
  }

  if (appState === 'authenticated' && selectedTeam) {
    return (
      <Layout 
        currentPath={currentPath} 
        onNavigate={handleNavigation}
      >
        {renderCurrentPage()}
      </Layout>
    );
  }

  // Fallback - borde inte hända
  return <Login onTeamSelect={handleLoginSuccess} />;
};

const App: React.FC = () => {
  return (
    <ThemeProvider theme={footballTheme}>
      <CssBaseline />
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </ThemeProvider>
  );
};

export default App;

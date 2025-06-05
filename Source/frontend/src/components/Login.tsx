import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Alert,
  Container,
  Avatar,
} from '@mui/material';
import SportsSoccerIcon from '@mui/icons-material/SportsSoccer';
import { useAuth } from '../contexts/AuthContext';
import { TeamDto } from '../types/api';

interface LoginProps {
  onTeamSelect: (teams: TeamDto[]) => void;
}

const Login: React.FC<LoginProps> = ({ onTeamSelect }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const teams = await login(username, password);
      onTeamSelect(teams);
    } catch (err: any) {
      setError('Inloggning misslyckades. Kontrollera användarnamn och lösenord.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        position: 'relative',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 2,
        // Bakgrundsbild med fallback
        backgroundImage: 'url(/football_stadium_backdrop.jpg)',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        backgroundRepeat: 'no-repeat',
        // Fallback gradient om bilden inte laddas
        background: `
          linear-gradient(135deg, rgba(27, 94, 32, 0.9) 0%, rgba(46, 125, 50, 0.8) 100%),
          url(/football_stadium_backdrop.jpg)
        `,
        '&::before': {
          content: '""',
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          // Mörkare overlay för bättre läsbarhet
          background: 'linear-gradient(135deg, rgba(0, 0, 0, 0.4) 0%, rgba(27, 94, 32, 0.6) 100%)',
          zIndex: 1,
        },
      }}
    >
      <Container maxWidth="sm" sx={{ position: 'relative', zIndex: 2 }}>
        <Card
          sx={{
            backdropFilter: 'blur(15px)',
            backgroundColor: 'rgba(255, 255, 255, 0.95)',
            boxShadow: '0 12px 40px rgba(0, 0, 0, 0.3)',
            borderRadius: 3,
            border: '1px solid rgba(255, 255, 255, 0.2)',
          }}
        >
          <CardContent sx={{ p: 4 }}>
            <Box display="flex" flexDirection="column" alignItems="center" mb={3}>
              <Avatar
                sx={{
                  bgcolor: 'primary.main',
                  width: 64,
                  height: 64,
                  mb: 2,
                  boxShadow: '0 4px 12px rgba(46, 125, 50, 0.3)',
                }}
              >
                <SportsSoccerIcon sx={{ fontSize: 32 }} />
              </Avatar>
              <Typography variant="h4" component="h1" gutterBottom textAlign="center">
                Coach App
              </Typography>
              <Typography variant="body1" color="text.secondary" textAlign="center">
                Hantera ditt fotbollslag enkelt och effektivt
              </Typography>
            </Box>

            <Box component="form" onSubmit={handleSubmit}>
              <TextField
                fullWidth
                label="Användarnamn"
                variant="outlined"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                sx={{ mb: 2 }}
                autoComplete="username"
              />
              
              <TextField
                fullWidth
                type="password"
                label="Lösenord"
                variant="outlined"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                sx={{ mb: 3 }}
                autoComplete="current-password"
              />

              {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                  {error}
                </Alert>
              )}

              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={loading}
                sx={{
                  py: 1.5,
                  fontSize: '1.1rem',
                  fontWeight: 600,
                  background: 'linear-gradient(45deg, #2e7d32 30%, #66bb6a 90%)',
                  boxShadow: '0 4px 12px rgba(46, 125, 50, 0.3)',
                  '&:hover': {
                    background: 'linear-gradient(45deg, #1b5e20 30%, #2e7d32 90%)',
                    boxShadow: '0 6px 16px rgba(46, 125, 50, 0.4)',
                  },
                }}
              >
                {loading ? 'Loggar in...' : 'Logga in'}
              </Button>
            </Box>

            <Box mt={3} textAlign="center">
              <Typography variant="body2" color="text.secondary">
                ⚽ Välkommen till din digitala fotbollscoach! 🏆
              </Typography>
            </Box>
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
};

export default Login; 
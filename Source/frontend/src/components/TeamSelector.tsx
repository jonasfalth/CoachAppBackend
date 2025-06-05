import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  ListItemIcon,
  Container,
  Avatar,
  Alert,
} from '@mui/material';
import GroupsIcon from '@mui/icons-material/Groups';
import SportsSoccerIcon from '@mui/icons-material/SportsSoccer';
import { useAuth } from '../contexts/AuthContext';
import { TeamDto } from '../types/api';

interface TeamSelectorProps {
  teams: TeamDto[];
  onTeamSelected: () => void;
}

const TeamSelector: React.FC<TeamSelectorProps> = ({ teams, onTeamSelected }) => {
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { selectTeam } = useAuth();

  const handleTeamSelect = async (teamId: number) => {
    setError('');
    setLoading(true);

    try {
      await selectTeam(teamId);
      onTeamSelected();
    } catch (err: any) {
      setError('Kunde inte välja team. Försök igen.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, rgba(27, 94, 32, 0.9) 0%, rgba(46, 125, 50, 0.8) 100%), url("data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' viewBox=\'0 0 1200 600\'%3E%3Cpath d=\'M0 600L100 580Q200 560 300 520Q400 480 500 460Q600 440 700 400Q800 360 900 380Q1000 400 1100 420L1200 440V600Z\' fill=\'%23388e3c\' opacity=\'0.3\'/%3E%3C/svg%3E")',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 2,
      }}
    >
      <Container maxWidth="md">
        <Card
          sx={{
            backdropFilter: 'blur(10px)',
            backgroundColor: 'rgba(255, 255, 255, 0.95)',
            boxShadow: '0 8px 32px rgba(46, 125, 50, 0.3)',
            borderRadius: 3,
          }}
        >
          <CardContent sx={{ p: 4 }}>
            <Box display="flex" flexDirection="column" alignItems="center" mb={4}>
              <Avatar
                sx={{
                  bgcolor: 'primary.main',
                  width: 64,
                  height: 64,
                  mb: 2,
                }}
              >
                <GroupsIcon sx={{ fontSize: 32 }} />
              </Avatar>
              <Typography variant="h4" component="h1" gutterBottom textAlign="center">
                Välj ditt team
              </Typography>
              <Typography variant="body1" color="text.secondary" textAlign="center">
                Välj vilket team du vill arbeta med
              </Typography>
            </Box>

            {error && (
              <Alert severity="error" sx={{ mb: 3 }}>
                {error}
              </Alert>
            )}

            <List sx={{ maxWidth: 600, mx: 'auto' }}>
              {teams.map((team) => (
                <ListItem key={team.id} disablePadding sx={{ mb: 1 }}>
                  <ListItemButton
                    onClick={() => handleTeamSelect(team.id)}
                    disabled={loading}
                    sx={{
                      borderRadius: 2,
                      border: '2px solid',
                      borderColor: 'rgba(46, 125, 50, 0.1)',
                      '&:hover': {
                        borderColor: 'primary.main',
                        backgroundColor: 'rgba(46, 125, 50, 0.05)',
                      },
                      py: 2,
                      px: 3,
                    }}
                  >
                    <ListItemIcon>
                      <SportsSoccerIcon sx={{ color: 'primary.main' }} />
                    </ListItemIcon>
                    <ListItemText
                      primary={team.name}
                      secondary={`Databas: ${team.databaseName}`}
                      primaryTypographyProps={{
                        variant: 'h6',
                        color: 'primary.main',
                        fontWeight: 600,
                      }}
                      secondaryTypographyProps={{
                        color: 'text.secondary',
                      }}
                    />
                  </ListItemButton>
                </ListItem>
              ))}
            </List>

            {teams.length === 0 && (
              <Box textAlign="center" py={4}>
                <Typography variant="body1" color="text.secondary">
                  Du har inga team tilldelade. Kontakta administratören.
                </Typography>
              </Box>
            )}
          </CardContent>
        </Card>
      </Container>
    </Box>
  );
};

export default TeamSelector; 
import React from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
} from '@mui/material';
import SettingsIcon from '@mui/icons-material/Settings';

const Settings: React.FC = () => {
  return (
    <Box>
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center' }}>
        <SettingsIcon sx={{ mr: 2, color: 'primary.main', fontSize: 40 }} />
        <Typography variant="h3" component="h1" color="primary.main">
          Inställningar
        </Typography>
      </Box>

      <Card>
        <CardContent sx={{ textAlign: 'center', py: 8 }}>
          <SettingsIcon sx={{ fontSize: 80, color: 'primary.light', mb: 2 }} />
          <Typography variant="h5" gutterBottom color="primary.main">
            Inställningssektionen kommer snart!
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Här kommer du kunna konfigurera applikationen, 
            hantera användarinställningar och mycket mer.
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
};

export default Settings; 
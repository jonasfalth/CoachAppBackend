import React from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
} from '@mui/material';
import PeopleIcon from '@mui/icons-material/People';

const Players: React.FC = () => {
  return (
    <Box>
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center' }}>
        <PeopleIcon sx={{ mr: 2, color: 'primary.main', fontSize: 40 }} />
        <Typography variant="h3" component="h1" color="primary.main">
          Spelare
        </Typography>
      </Box>

      <Card>
        <CardContent sx={{ textAlign: 'center', py: 8 }}>
          <PeopleIcon sx={{ fontSize: 80, color: 'primary.light', mb: 2 }} />
          <Typography variant="h5" gutterBottom color="primary.main">
            Spelarsektionen kommer snart!
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Här kommer du kunna hantera dina spelare, se statistik, 
            redigera profiler och mycket mer.
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
};

export default Players; 
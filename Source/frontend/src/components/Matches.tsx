import React from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
} from '@mui/material';
import SportsFootballIcon from '@mui/icons-material/SportsFootball';

const Matches: React.FC = () => {
  return (
    <Box>
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center' }}>
        <SportsFootballIcon sx={{ mr: 2, color: 'primary.main', fontSize: 40 }} />
        <Typography variant="h3" component="h1" color="primary.main">
          Matcher
        </Typography>
      </Box>

      <Card>
        <CardContent sx={{ textAlign: 'center', py: 8 }}>
          <SportsFootballIcon sx={{ fontSize: 80, color: 'primary.light', mb: 2 }} />
          <Typography variant="h5" gutterBottom color="primary.main">
            Matchsektionen kommer snart!
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Här kommer du kunna schemalägga matcher, se resultat, 
            hantera matchdata och statistik.
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
};

export default Matches; 
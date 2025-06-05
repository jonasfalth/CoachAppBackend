import React from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
} from '@mui/material';
import LocationOnIcon from '@mui/icons-material/LocationOn';

const Positions: React.FC = () => {
  return (
    <Box>
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center' }}>
        <LocationOnIcon sx={{ mr: 2, color: 'primary.main', fontSize: 40 }} />
        <Typography variant="h3" component="h1" color="primary.main">
          Positioner
        </Typography>
      </Box>

      <Card>
        <CardContent sx={{ textAlign: 'center', py: 8 }}>
          <LocationOnIcon sx={{ fontSize: 80, color: 'primary.light', mb: 2 }} />
          <Typography variant="h5" gutterBottom color="primary.main">
            Positionssektionen kommer snart!
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Här kommer du kunna hantera spelarpositioner, 
            skapa formationer och planera taktik.
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
};

export default Positions; 
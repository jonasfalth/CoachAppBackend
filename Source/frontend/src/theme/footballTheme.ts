import { createTheme } from '@mui/material/styles';

export const footballTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#2e7d32', // Fotbollsgrön
      light: '#60ad5e',
      dark: '#005005',
      contrastText: '#ffffff',
    },
    secondary: {
      main: '#66bb6a', // Ljusare grön
      light: '#98ee99',
      dark: '#338a3e',
      contrastText: '#000000',
    },
    success: {
      main: '#4caf50',
      light: '#80e27e',
      dark: '#087f23',
    },
    background: {
      default: '#f1f8e9', // Mycket ljus grön bakgrund
      paper: '#ffffff',
    },
    text: {
      primary: '#1b5e20',
      secondary: '#2e7d32',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h1: {
      fontWeight: 700,
      fontSize: '2.5rem',
      color: '#1b5e20',
    },
    h2: {
      fontWeight: 600,
      fontSize: '2rem',
      color: '#1b5e20',
    },
    h3: {
      fontWeight: 600,
      fontSize: '1.5rem',
      color: '#2e7d32',
    },
    h4: {
      fontWeight: 500,
      fontSize: '1.25rem',
      color: '#2e7d32',
    },
    h5: {
      fontWeight: 500,
      fontSize: '1.1rem',
      color: '#2e7d32',
    },
    button: {
      fontWeight: 600,
      textTransform: 'none',
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          textTransform: 'none',
          fontWeight: 600,
          boxShadow: 'none',
          '&:hover': {
            boxShadow: '0 2px 8px rgba(46, 125, 50, 0.2)',
          },
        },
        contained: {
          background: 'linear-gradient(45deg, #2e7d32 30%, #66bb6a 90%)',
          '&:hover': {
            background: 'linear-gradient(45deg, #1b5e20 30%, #2e7d32 90%)',
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0 2px 12px rgba(46, 125, 50, 0.1)',
          '&:hover': {
            boxShadow: '0 4px 20px rgba(46, 125, 50, 0.15)',
          },
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          background: 'linear-gradient(90deg, #1b5e20 0%, #2e7d32 100%)',
          boxShadow: '0 2px 8px rgba(46, 125, 50, 0.2)',
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          background: 'linear-gradient(180deg, #f1f8e9 0%, #e8f5e8 100%)',
          borderRight: '1px solid rgba(46, 125, 50, 0.1)',
        },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            '& fieldset': {
              borderColor: 'rgba(46, 125, 50, 0.3)',
            },
            '&:hover fieldset': {
              borderColor: 'rgba(46, 125, 50, 0.5)',
            },
            '&.Mui-focused fieldset': {
              borderColor: '#2e7d32',
            },
          },
        },
      },
    },
  },
}); 
import React, { useState } from 'react';
import {
  Box,
  Drawer,
  AppBar,
  Toolbar,
  Typography,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  IconButton,
  Avatar,
  Divider,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Menu as MenuIcon,
  SportsSoccer as SportsSoccerIcon,
  People as PeopleIcon,
  SportsFootball as SportsFootballIcon,
  LocationOn as LocationOnIcon,
  Settings as SettingsIcon,
  Logout as LogoutIcon,
  Groups as GroupsIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';

const DRAWER_WIDTH = 280;

interface NavigationItem {
  text: string;
  icon: React.ReactElement;
  path: string;
}

interface LayoutProps {
  children: React.ReactNode;
  currentPath: string;
  onNavigate: (path: string) => void;
}

const Layout: React.FC<LayoutProps> = ({ children, currentPath, onNavigate }) => {
  const [mobileOpen, setMobileOpen] = useState(false);
  const { user, selectedTeam, logout } = useAuth();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const navigationItems: NavigationItem[] = [
    { text: 'Spelare', icon: <PeopleIcon />, path: '/players' },
    { text: 'Matcher', icon: <SportsFootballIcon />, path: '/matches' },
    { text: 'Positioner', icon: <LocationOnIcon />, path: '/positions' },
    { text: 'Inställningar', icon: <SettingsIcon />, path: '/settings' },
  ];

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleLogout = async () => {
    await logout();
  };

  const handleNavigation = (path: string) => {
    onNavigate(path);
    if (isMobile) {
      setMobileOpen(false);
    }
  };

  const drawer = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Box sx={{ p: 3, textAlign: 'center' }}>
        <Avatar
          sx={{
            bgcolor: 'primary.main',
            width: 60,
            height: 60,
            mx: 'auto',
            mb: 2,
          }}
        >
          <SportsSoccerIcon sx={{ fontSize: 30 }} />
        </Avatar>
        <Typography variant="h6" color="primary.main" fontWeight={700}>
          Coach App
        </Typography>
        {selectedTeam && (
          <Box mt={1}>
            <Typography variant="body2" color="text.secondary">
              Team:
            </Typography>
            <Typography variant="body1" color="primary.main" fontWeight={600}>
              {selectedTeam.name}
            </Typography>
          </Box>
        )}
      </Box>

      <Divider />

      {/* Navigation */}
      <List sx={{ flex: 1, px: 2, py: 1 }}>
        {navigationItems.map((item) => (
          <ListItem key={item.path} disablePadding sx={{ mb: 0.5 }}>
            <ListItemButton
              selected={currentPath === item.path}
              onClick={() => handleNavigation(item.path)}
              sx={{
                borderRadius: 2,
                '&.Mui-selected': {
                  backgroundColor: 'rgba(46, 125, 50, 0.1)',
                  borderLeft: '4px solid',
                  borderLeftColor: 'primary.main',
                  '&:hover': {
                    backgroundColor: 'rgba(46, 125, 50, 0.15)',
                  },
                },
                '&:hover': {
                  backgroundColor: 'rgba(46, 125, 50, 0.05)',
                },
              }}
            >
              <ListItemIcon
                sx={{
                  color: currentPath === item.path ? 'primary.main' : 'text.secondary',
                }}
              >
                {item.icon}
              </ListItemIcon>
              <ListItemText
                primary={item.text}
                primaryTypographyProps={{
                  fontWeight: currentPath === item.path ? 600 : 400,
                  color: currentPath === item.path ? 'primary.main' : 'text.primary',
                }}
              />
            </ListItemButton>
          </ListItem>
        ))}
      </List>

      <Divider />

      {/* User info and logout */}
      <Box sx={{ p: 2 }}>
        {user && (
          <Box sx={{ mb: 2 }}>
            <Typography variant="body2" color="text.secondary">
              Inloggad som:
            </Typography>
            <Typography variant="body1" color="primary.main" fontWeight={600}>
              {user.username}
            </Typography>
          </Box>
        )}
        <ListItemButton
          onClick={handleLogout}
          sx={{
            borderRadius: 2,
            color: 'error.main',
            '&:hover': {
              backgroundColor: 'rgba(211, 47, 47, 0.05)',
            },
          }}
        >
          <ListItemIcon sx={{ color: 'error.main' }}>
            <LogoutIcon />
          </ListItemIcon>
          <ListItemText primary="Logga ut" />
        </ListItemButton>
      </Box>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex' }}>
      {/* AppBar */}
      <AppBar
        position="fixed"
        sx={{
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          ml: { md: `${DRAWER_WIDTH}px` },
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="öppna drawer"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { md: 'none' } }}
          >
            <MenuIcon />
          </IconButton>
          <GroupsIcon sx={{ mr: 2 }} />
          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            {selectedTeam ? `${selectedTeam.name} - Lagshantering` : 'Lagshantering'}
          </Typography>
        </Toolbar>
      </AppBar>

      {/* Drawer */}
      <Box
        component="nav"
        sx={{ width: { md: DRAWER_WIDTH }, flexShrink: { md: 0 } }}
      >
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{
            keepMounted: true, // Bättre prestanda på mobil
          }}
          sx={{
            display: { xs: 'block', md: 'none' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: DRAWER_WIDTH,
            },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', md: 'block' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: DRAWER_WIDTH,
            },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>

      {/* Main content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          mt: '64px', // AppBar height
          minHeight: 'calc(100vh - 64px)',
        }}
      >
        {children}
      </Box>
    </Box>
  );
};

export default Layout; 
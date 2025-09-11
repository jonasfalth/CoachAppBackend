import {
  Add as AddIcon,
  Warning as CardIcon,
  Edit as EditIcon,
  SportsScore as GoalIcon,
  Pause as PauseIcon,
  PlayArrow as PlayIcon,
  SportsSoccer as SoccerIcon,
  SwapHoriz as SubstitutionIcon,
  Timer as TimerIcon,
} from "@mui/icons-material";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Fab,
  FormControl,
  Grid,
  InputLabel,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  MenuItem,
  Select,
  TextField,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import React, { useEffect, useState } from "react";
import {
  currentMatchAPI,
  fieldPositionsAPI,
  matchesAPI,
  matchEventsAPI,
  playerPositionsAPI,
  playersAPI,
} from "../services/api";
import {
  CurrentMatch as CurrentMatchType,
  FieldPosition,
  Match,
  MatchEvent,
  Player,
  PlayerPosition,
} from "../types/api";

const CurrentMatch: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("lg"));

  // State management
  const [matches, setMatches] = useState<Match[]>([]);
  const [currentMatch, setCurrentMatch] = useState<CurrentMatchType | null>(
    null
  );
  const [selectedMatch, setSelectedMatch] = useState<Match | null>(null);
  const [players, setPlayers] = useState<Player[]>([]);
  const [fieldPositions, setFieldPositions] = useState<FieldPosition[]>([]);
  const [playerPositions, setPlayerPositions] = useState<PlayerPosition[]>([]);
  const [matchEvents, setMatchEvents] = useState<MatchEvent[]>([]);

  // UI State
  const [loading, setLoading] = useState(true);
  const [matchSelectOpen, setMatchSelectOpen] = useState(false);
  const [formationDialogOpen, setFormationDialogOpen] = useState(false);
  const [eventDialogOpen, setEventDialogOpen] = useState(false);
  const [eventType, setEventType] = useState<
    "goal" | "yellow_card" | "red_card" | "substitution"
  >("goal");
  const [error, setError] = useState<string | null>(null);

  // Timer state
  const [currentTime, setCurrentTime] = useState<number>(0);
  const [timerInterval, setTimerInterval] = useState<NodeJS.Timeout | null>(
    null
  );

  // Event form state
  const [selectedPlayer, setSelectedPlayer] = useState<number | "">("");
  const [eventNotes, setEventNotes] = useState("");

  // Load data on component mount
  useEffect(() => {
    loadInitialData();
  }, []);

  // Timer effect
  useEffect(() => {
    if (
      currentMatch &&
      (currentMatch.status === "first_half" ||
        currentMatch.status === "second_half")
    ) {
      startTimer();
    } else {
      stopTimer();
    }
    return () => stopTimer();
  }, [currentMatch?.status]);

  const loadInitialData = async () => {
    try {
      setLoading(true);
      const [matchesData, playersData, fieldPositionsData, currentMatchData] =
        await Promise.all([
          matchesAPI.getAll(),
          playersAPI.getAll(),
          fieldPositionsAPI.getAll(),
          currentMatchAPI.getActive(),
        ]);

      setMatches(matchesData);
      setPlayers(playersData);
      setFieldPositions(fieldPositionsData);

      if (currentMatchData) {
        setCurrentMatch(currentMatchData);
        const match = matchesData.find(
          (m) => m.id === currentMatchData.matchId
        );
        setSelectedMatch(match || null);
        await loadMatchData(currentMatchData.id);
      }
    } catch (err) {
      setError("Kunde inte ladda data");
      console.error("Error loading data:", err);
    } finally {
      setLoading(false);
    }
  };

  const loadMatchData = async (currentMatchId: number) => {
    try {
      const [playerPositionsData, eventsData] = await Promise.all([
        playerPositionsAPI.getByCurrentMatchId(currentMatchId),
        matchEventsAPI.getByCurrentMatchId(currentMatchId),
      ]);

      setPlayerPositions(playerPositionsData);
      setMatchEvents(eventsData);
    } catch (err) {
      console.error("Error loading match data:", err);
    }
  };

  const startTimer = () => {
    if (timerInterval) clearInterval(timerInterval);

    const interval = setInterval(() => {
      setCurrentTime((prev) => prev + 1);

      // Update backend every 30 seconds
      if (currentTime % 30 === 0) {
        updateMatchTime();
      }
    }, 1000);

    setTimerInterval(interval);
  };

  const stopTimer = () => {
    if (timerInterval) {
      clearInterval(timerInterval);
      setTimerInterval(null);
    }
  };

  const updateMatchTime = async () => {
    if (!currentMatch) return;

    try {
      const updatedMatch = { ...currentMatch };

      if (currentMatch.status === "first_half") {
        updatedMatch.firstHalfDurationSeconds = currentTime;
      } else if (currentMatch.status === "second_half") {
        updatedMatch.secondHalfDurationSeconds = currentTime;
      }

      await currentMatchAPI.update(currentMatch.id, updatedMatch);
    } catch (err) {
      console.error("Error updating match time:", err);
    }
  };

  // Match control functions
  const handleStartFirstHalf = async () => {
    if (!currentMatch) return;

    try {
      const updatedMatch = {
        ...currentMatch,
        status: "first_half" as const,
        firstHalfStartTime: new Date().toISOString(),
        matchStartTime: new Date().toISOString(),
      };

      const result = await currentMatchAPI.update(
        currentMatch.id,
        updatedMatch
      );
      setCurrentMatch(result);
      setCurrentTime(0);
    } catch (err) {
      setError("Kunde inte starta matchen");
      console.error("Error starting first half:", err);
    }
  };

  const handlePauseMatch = async () => {
    if (!currentMatch) return;

    try {
      const updatedMatch = {
        ...currentMatch,
        status: "paused" as const,
        lastPauseTime: new Date().toISOString(),
      };

      const result = await currentMatchAPI.update(
        currentMatch.id,
        updatedMatch
      );
      setCurrentMatch(result);
    } catch (err) {
      setError("Kunde inte pausa matchen");
      console.error("Error pausing match:", err);
    }
  };

  const handleResumeMatch = async () => {
    if (!currentMatch) return;

    try {
      const previousStatus: "first_half" | "second_half" =
        currentMatch.firstHalfDurationSeconds > 0
          ? "second_half"
          : "first_half";
      const updatedMatch = {
        ...currentMatch,
        status: previousStatus,
        lastPauseTime: undefined,
      };

      const result = await currentMatchAPI.update(
        currentMatch.id,
        updatedMatch
      );
      setCurrentMatch(result);
    } catch (err) {
      setError("Kunde inte återuppta matchen");
      console.error("Error resuming match:", err);
    }
  };

  const handleEndFirstHalf = async () => {
    if (!currentMatch) return;

    try {
      const updatedMatch = {
        ...currentMatch,
        status: "half_time" as const,
        firstHalfDurationSeconds: currentTime,
      };

      const result = await currentMatchAPI.update(
        currentMatch.id,
        updatedMatch
      );
      setCurrentMatch(result);
      setCurrentTime(0);
    } catch (err) {
      setError("Kunde inte avsluta första halvlek");
      console.error("Error ending first half:", err);
    }
  };

  const handleStartSecondHalf = async () => {
    if (!currentMatch) return;

    try {
      const updatedMatch = {
        ...currentMatch,
        status: "second_half" as const,
        secondHalfStartTime: new Date().toISOString(),
      };

      const result = await currentMatchAPI.update(
        currentMatch.id,
        updatedMatch
      );
      setCurrentMatch(result);
      setCurrentTime(0);
    } catch (err) {
      setError("Kunde inte starta andra halvlek");
      console.error("Error starting second half:", err);
    }
  };

  const handleSelectMatch = async (match: Match) => {
    try {
      // Check if there's already an active match
      const existingMatch = await currentMatchAPI.getActive();
      if (existingMatch) {
        setError("Det finns redan en aktiv match. Avsluta den först.");
        return;
      }

      // Create new current match
      const newCurrentMatch: Omit<
        CurrentMatchType,
        "id" | "createdAt" | "updatedAt"
      > = {
        matchId: match.id,
        status: "setup",
        firstHalfDurationSeconds: 0,
        secondHalfDurationSeconds: 0,
        totalPauseSeconds: 0,
        homeScore: 0,
        awayScore: 0,
      };

      const createdMatch = await currentMatchAPI.create(newCurrentMatch);
      setCurrentMatch(createdMatch);
      setSelectedMatch(match);
      setMatchSelectOpen(false);
      setFormationDialogOpen(true);
    } catch (err) {
      setError("Kunde inte skapa aktiv match");
      console.error("Error creating current match:", err);
    }
  };

  const handleAddEvent = async () => {
    if (!currentMatch || !selectedPlayer) return;

    try {
      const matchMinute = Math.floor(currentTime / 60);
      const matchSecond = currentTime % 60;

      const newEvent: Omit<MatchEvent, "id" | "eventTime"> = {
        currentMatchId: currentMatch.id,
        eventType: eventType,
        playerId: Number(selectedPlayer),
        matchMinute,
        matchSecond,
        notes: eventNotes || undefined,
      };

      const createdEvent = await matchEventsAPI.create(newEvent);
      setMatchEvents([...matchEvents, createdEvent]);

      // Update score if goal
      if (eventType === "goal") {
        const updatedMatch = {
          ...currentMatch,
          homeScore: currentMatch.homeScore + 1,
        };
        const result = await currentMatchAPI.update(
          currentMatch.id,
          updatedMatch
        );
        setCurrentMatch(result);
      }

      // Reset form
      setSelectedPlayer("");
      setEventNotes("");
      setEventDialogOpen(false);
    } catch (err) {
      setError("Kunde inte lägga till händelse");
      console.error("Error adding event:", err);
    }
  };

  const openEventDialog = (
    type: "goal" | "yellow_card" | "red_card" | "substitution"
  ) => {
    setEventType(type);
    setEventDialogOpen(true);
  };

  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, "0")}:${secs
      .toString()
      .padStart(2, "0")}`;
  };

  const getMatchTime = (): string => {
    if (!currentMatch) return "00:00";

    if (currentMatch.status === "first_half") {
      return formatTime(currentMatch.firstHalfDurationSeconds + currentTime);
    } else if (currentMatch.status === "second_half") {
      return formatTime(
        45 * 60 + currentMatch.secondHalfDurationSeconds + currentTime
      );
    } else if (currentMatch.status === "half_time") {
      return "45:00";
    }

    return formatTime(currentMatch.firstHalfDurationSeconds);
  };

  const getActivePlayerPositions = () => {
    return playerPositions.filter((pp) => pp.isActive);
  };

  const getPlayerName = (playerId: number | undefined) => {
    if (!playerId) return "Okänd spelare";
    const player = players.find((p) => p.id === playerId);
    return player ? player.name : "Okänd spelare";
  };

  const getStatusText = (status: string) => {
    const statusTexts = {
      setup: "Förberedelse",
      first_half: "Första halvlek",
      half_time: "Halvtid",
      second_half: "Andra halvlek",
      paused: "Pausad",
      finished: "Avslutad",
    };
    return statusTexts[status as keyof typeof statusTexts] || status;
  };

  const getEventIcon = (eventType: string) => {
    switch (eventType) {
      case "goal":
        return <GoalIcon color="success" />;
      case "yellow_card":
        return <CardIcon sx={{ color: "#FFC107" }} />;
      case "red_card":
        return <CardIcon color="error" />;
      case "substitution":
        return <SubstitutionIcon color="primary" />;
      default:
        return <SoccerIcon />;
    }
  };

  const getEventText = (eventType: string) => {
    const eventTexts = {
      goal: "Mål",
      yellow_card: "Gult kort",
      red_card: "Rött kort",
      substitution: "Byte",
    };
    return eventTexts[eventType as keyof typeof eventTexts] || eventType;
  };

  // Render loading state
  if (loading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="400px"
      >
        <CircularProgress />
      </Box>
    );
  }

  // Render match selection if no current match
  if (!currentMatch || !selectedMatch) {
    return (
      <Box>
        <Box
          display="flex"
          justifyContent="space-between"
          alignItems="center"
          mb={3}
        >
          <Typography
            variant="h4"
            component="h1"
            color="primary.main"
            fontWeight={700}
          >
            Aktuell Match
          </Typography>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <Card>
          <CardContent sx={{ textAlign: "center", py: 6 }}>
            <SoccerIcon sx={{ fontSize: 64, color: "primary.main", mb: 2 }} />
            <Typography variant="h6" color="text.secondary" mb={3}>
              Ingen aktiv match vald
            </Typography>
            <Button
              variant="contained"
              size="large"
              onClick={() => setMatchSelectOpen(true)}
              startIcon={<PlayIcon />}
            >
              Välj Match
            </Button>
          </CardContent>
        </Card>

        {/* Match Selection Dialog */}
        <Dialog
          open={matchSelectOpen}
          onClose={() => setMatchSelectOpen(false)}
          maxWidth="sm"
          fullWidth
        >
          <DialogTitle>Välj Match för Live-spelning</DialogTitle>
          <DialogContent>
            <FormControl fullWidth sx={{ mt: 2 }}>
              <InputLabel>Match</InputLabel>
              <Select
                value=""
                onChange={(e) => {
                  const match = matches.find(
                    (m) => m.id === Number(e.target.value)
                  );
                  if (match) handleSelectMatch(match);
                }}
              >
                {matches.map((match) => (
                  <MenuItem key={match.id} value={match.id}>
                    {match.date} - {match.opponent} (
                    {match.homeGame ? "Hemma" : "Borta"})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setMatchSelectOpen(false)}>Avbryt</Button>
          </DialogActions>
        </Dialog>
      </Box>
    );
  }

  // Main live match interface
  return (
    <Box>
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={3}
      >
        <Typography
          variant="h4"
          component="h1"
          color="primary.main"
          fontWeight={700}
        >
          Live Match
        </Typography>
        <Button
          variant="outlined"
          onClick={() => setMatchSelectOpen(true)}
          startIcon={<EditIcon />}
        >
          Byt Match
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Match Header */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid component="div" item xs={12} md={4}>
              <Typography variant="h6">{selectedMatch.opponent}</Typography>
              <Typography variant="body2" color="text.secondary">
                {selectedMatch.date} •{" "}
                {selectedMatch.homeGame ? "Hemma" : "Borta"}
              </Typography>
            </Grid>
            <Grid component="div" item xs={12} md={4} textAlign="center">
              <Typography variant="h3" color="primary.main" fontWeight={700}>
                {currentMatch.homeScore} - {currentMatch.awayScore}
              </Typography>
              <Box
                display="flex"
                alignItems="center"
                justifyContent="center"
                gap={1}
              >
                <TimerIcon color="action" />
                <Typography variant="h6" color="text.secondary">
                  {getMatchTime()}
                </Typography>
              </Box>
            </Grid>
            <Grid component="div" item xs={12} md={4} textAlign="right">
              <Chip
                label={getStatusText(currentMatch.status)}
                color={currentMatch.status === "setup" ? "default" : "primary"}
                sx={{ mr: 1 }}
              />
              {currentMatch.formation && (
                <Chip label={currentMatch.formation} variant="outlined" />
              )}
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Box
        sx={{
          display: "flex",
          flexDirection: { xs: "column", md: "row" },
          gap: 3,
        }}
      >
        <Box sx={{ width: { xs: "100%", md: "66.66%" }, pr: { md: 1.5 } }}>
          {/* Match Controls */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Match Kontroller
              </Typography>
              <Box display="flex" gap={1} flexWrap="wrap">
                {currentMatch.status === "setup" && (
                  <Button
                    variant="contained"
                    startIcon={<PlayIcon />}
                    onClick={handleStartFirstHalf}
                  >
                    Starta Match
                  </Button>
                )}

                {(currentMatch.status === "first_half" ||
                  currentMatch.status === "second_half") && (
                  <Button
                    variant="contained"
                    color="warning"
                    startIcon={<PauseIcon />}
                    onClick={handlePauseMatch}
                  >
                    Pausa
                  </Button>
                )}

                {currentMatch.status === "paused" && (
                  <Button
                    variant="contained"
                    startIcon={<PlayIcon />}
                    onClick={handleResumeMatch}
                  >
                    Fortsätt
                  </Button>
                )}

                {currentMatch.status === "first_half" && (
                  <Button variant="outlined" onClick={handleEndFirstHalf}>
                    Avsluta Första Halvlek
                  </Button>
                )}

                {currentMatch.status === "half_time" && (
                  <Button
                    variant="contained"
                    startIcon={<PlayIcon />}
                    onClick={handleStartSecondHalf}
                  >
                    Starta Andra Halvlek
                  </Button>
                )}
              </Box>
            </CardContent>
          </Card>

          {/* Event Controls */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Händelser
              </Typography>
              <Box display="flex" gap={1} flexWrap="wrap">
                <Button
                  variant="outlined"
                  startIcon={<GoalIcon />}
                  onClick={() => openEventDialog("goal")}
                  size="small"
                >
                  Mål
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<CardIcon />}
                  color="warning"
                  onClick={() => openEventDialog("yellow_card")}
                  size="small"
                >
                  Gult Kort
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<CardIcon />}
                  color="error"
                  onClick={() => openEventDialog("red_card")}
                  size="small"
                >
                  Rött Kort
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<SubstitutionIcon />}
                  onClick={() => openEventDialog("substitution")}
                  size="small"
                >
                  Byte
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Box>

        <Box sx={{ width: { xs: "100%", md: "33.33%" }, pl: { md: 1.5 } }}>
          {/* Active Players */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Aktiva Spelare ({getActivePlayerPositions().length}/9)
              </Typography>
              <List dense>
                {getActivePlayerPositions().map((playerPosition) => {
                  const player = players.find(
                    (p) => p.id === playerPosition.playerId
                  );
                  const fieldPosition = fieldPositions.find(
                    (fp) => fp.id === playerPosition.fieldPositionId
                  );

                  return (
                    <ListItem key={playerPosition.id}>
                      <ListItemIcon>
                        <Chip
                          label={player?.jerseyNumber || "?"}
                          size="small"
                          color="primary"
                        />
                      </ListItemIcon>
                      <ListItemText
                        primary={player?.name || "Okänd spelare"}
                        secondary={
                          fieldPosition?.abbreviation || "Okänd position"
                        }
                      />
                    </ListItem>
                  );
                })}
              </List>
            </CardContent>
          </Card>

          {/* Recent Events */}
          <Card>
            <CardContent>
              <Typography variant="h6" mb={2}>
                Senaste Händelser ({matchEvents.length})
              </Typography>
              <List dense>
                {matchEvents
                  .slice(-5)
                  .reverse()
                  .map((event) => (
                    <ListItem key={event.id}>
                      <ListItemIcon>
                        {getEventIcon(event.eventType)}
                      </ListItemIcon>
                      <ListItemText
                        primary={`${getEventText(
                          event.eventType
                        )} - ${getPlayerName(event.playerId)}`}
                        secondary={`${event.matchMinute}:${event.matchSecond
                          .toString()
                          .padStart(2, "0")}`}
                      />
                    </ListItem>
                  ))}
                {matchEvents.length === 0 && (
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    textAlign="center"
                  >
                    Inga händelser än
                  </Typography>
                )}
              </List>
            </CardContent>
          </Card>
        </Box>
      </Box>

      {/* Formation Setup Dialog */}
      <Dialog
        open={formationDialogOpen}
        onClose={() => setFormationDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Sätt Startuppställning</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" mb={3}>
            Välj 9 spelare för startuppställningen
          </Typography>
          {/* Placeholder for formation builder */}
          <Box
            sx={{
              minHeight: 400,
              border: "1px dashed grey",
              p: 2,
              textAlign: "center",
            }}
          >
            <Typography variant="h6" color="text.secondary">
              Formation Builder kommer snart
            </Typography>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setFormationDialogOpen(false)}>Avbryt</Button>
          <Button
            variant="contained"
            onClick={() => setFormationDialogOpen(false)}
          >
            Spara Uppställning
          </Button>
        </DialogActions>
      </Dialog>

      {/* Event Dialog */}
      <Dialog
        open={eventDialogOpen}
        onClose={() => setEventDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Lägg till {getEventText(eventType)}</DialogTitle>
        <DialogContent>
          <FormControl fullWidth sx={{ mt: 2, mb: 2 }}>
            <InputLabel>Spelare</InputLabel>
            <Select
              value={selectedPlayer}
              onChange={(e) => setSelectedPlayer(e.target.value as number | "")}
            >
              {players.map((player) => (
                <MenuItem key={player.id} value={player.id}>
                  #{player.jerseyNumber} {player.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <TextField
            fullWidth
            label="Anteckningar (valfritt)"
            value={eventNotes}
            onChange={(e) => setEventNotes(e.target.value)}
            multiline
            rows={2}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEventDialogOpen(false)}>Avbryt</Button>
          <Button
            variant="contained"
            onClick={handleAddEvent}
            disabled={!selectedPlayer}
          >
            Lägg till
          </Button>
        </DialogActions>
      </Dialog>

      {/* FAB for mobile */}
      {isMobile && (
        <Fab
          color="primary"
          sx={{ position: "fixed", bottom: 16, right: 16 }}
          onClick={() => openEventDialog("goal")}
        >
          <AddIcon />
        </Fab>
      )}
    </Box>
  );
};

export default CurrentMatch;

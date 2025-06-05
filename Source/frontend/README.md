# Coach App Frontend

En modern React TypeScript-applikation för fotbollslagshantering med Material-UI design.

## Funktioner

- **Autentisering**: Säker inloggning med JWT-cookies
- **Teamval**: Stöd för flera team per användare
- **Responsiv design**: Fungerar på desktop och mobil
- **Fotbollstema**: Grön färgpalett inspirerad av fotboll
- **Drawer-navigation**: Hamburgermeny med navigationsalternativ

## Teknisk stack

- **React 19** med TypeScript
- **Material-UI (MUI)** för UI-komponenter
- **Axios** för HTTP-anrop
- **Emotion** för styling

## Installation

```bash
npm install
```

## Utveckling

```bash
npm start
```

Applikationen startar på `http://localhost:3000`

## Backend-integration

Applikationen förväntar sig att backend körs på `http://localhost:5000`.

För att ändra backend-URL, skapa en `.env.local`-fil:

```
REACT_APP_API_URL=http://localhost:5000
```

## API-endpoints som används

- `POST /api/auth/validate` - Användarvalidering
- `POST /api/auth/select-team` - Teamval
- `POST /api/auth/logout` - Utloggning
- `GET /api/players` - Hämta spelare
- `GET /api/matches` - Hämta matcher
- `GET /api/positions` - Hämta positioner

## Projektstruktur
```
src/
├── components/          # React-komponenter
│   ├── Login.tsx       # Inloggningskomponent
│   ├── TeamSelector.tsx # Teamvalskomponent
│   ├── Layout.tsx      # Huvudlayout med navigation
│   ├── Players.tsx     # Spelarhantering
│   ├── Matches.tsx     # Matchhantering
│   ├── Positions.tsx   # Positionshantering
│   └── Settings.tsx    # Inställningar
├── contexts/           # React contexts
│   └── AuthContext.tsx # Autentiseringskontext
├── services/           # API-tjänster
│   └── api.ts         # HTTP-anrop till backend
├── theme/             # MUI-tema
│   └── footballTheme.ts # Fotbollstema
├── types/             # TypeScript-typer
│   └── api.ts         # API-typer
└── App.tsx            # Huvudapplikation
```

## Designprinciper

- **Fotbollskänsla**: Grön färgpalett och fotbollsikoner
- **Användarvänlighet**: Intuitiv navigation och tydliga gränssnitt
- **Responsivitet**: Fungerar på alla skärmstorlekar
- **Tillgänglighet**: Följer WCAG-riktlinjer

## Utvecklingsriktlinjer

- Använd TypeScript för all kod
- Följ React best practices
- Använd MUI-komponenter konsekvent
- Håll komponenter små och fokuserade
- Skriv beskrivande kommentarer på svenska

// Development. Backend dev server: http://localhost:5006 (per launchSettings.json).
// CORS on the API is configured to allow http://localhost:4200, which is the
// default port for `ng serve` — see GrantTrack/Program.cs:43.
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5006/api/v1',
  notificationApiUrl: 'http://localhost:5006/api/notification',
  hubUrl: 'http://localhost:5006/notificationHub',
};

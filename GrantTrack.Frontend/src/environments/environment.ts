// Production environment. Override at deploy time.
export const environment = {
  production: true,
  // Most controllers live under /api/v1; NotificationController is the odd one
  // out at /api/notification (no v1 segment) — see NotificationController.cs.
  apiUrl: '/api/v1',
  notificationApiUrl: '/api/notification',
  hubUrl: '/notificationHub',
};

// Mirrors GrantTrack/Dto/NotificationsDtos/*.
import { NotificationStatus } from './enums';

export interface NotificationCreateRequest {
  userId: number;
  applicantId: number;
  message: string;
  category: string;
}

/**
 * Read shape returned from GET /api/notification/user/{userId}.
 * Note: GetUserNotificationsAsync returns the EF entity directly, so the
 * shape lines up with the Notification entity (NotificationsReadDto isn't
 * actually used on the read path).
 */
export interface NotificationItem {
  notificationId: number;
  userId: number;
  applicationId: number;
  message: string;
  category: string;
  status: NotificationStatus | number; // enum sometimes serialized as int (no HasConversion override on read?)
  createdDate: string;
}

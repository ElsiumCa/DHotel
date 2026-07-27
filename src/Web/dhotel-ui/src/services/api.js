import axios from 'axios';

const GATEWAY_URL = 'http://localhost:5000';

const api = axios.create({
  baseURL: GATEWAY_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Saga State API
export const getSagaRoomStates = async () => {
  const response = await api.get('/api/saga/rooms');
  return response.data;
};

// FrontDesk API
export const checkoutGuest = async (reservationId, roomNumber) => {
  const response = await api.post(`/api/checkout/${reservationId}`, { roomNumber });
  return response.data;
};

export const checkinGuest = async (roomNumber, guestName, correlationId) => {
  const response = await api.post('/api/checkin', {
    roomNumber,
    guestName,
    correlationId
  });
  return response.data;
};

// Housekeeping API
export const getHousekeepingTasks = async () => {
  const response = await api.get('/api/housekeeping/tasks');
  return response.data;
};

export const startCleaning = async (taskId, cleanerName) => {
  const response = await api.post(`/api/housekeeping/tasks/${taskId}/start`, JSON.stringify(cleanerName));
  return response.data;
};

export const finishCleaning = async (taskId) => {
  const response = await api.post(`/api/housekeeping/tasks/${taskId}/finish`);
  return response.data;
};

export const reportDamage = async (roomNumber, correlationId, description) => {
  const response = await api.post('/api/housekeeping/report-damage', {
    roomNumber,
    correlationId,
    description,
    reportedBy: 'Housekeeper'
  });
  return response.data;
};

// Maintenance API
export const getMaintenanceTickets = async () => {
  const response = await api.get('/api/maintenance/tickets');
  return response.data;
};

export const resolveTicket = async (ticketId, technicianName) => {
  const response = await api.post(`/api/maintenance/tickets/${ticketId}/resolve`, JSON.stringify(technicianName));
  return response.data;
};

export default api;

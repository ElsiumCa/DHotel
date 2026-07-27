import { create } from 'zustand';

// F5 sonrasında durumların kaybolmaması için localStorage kontrolü
const defaultRooms = [
  { id: '101-guid', number: '101', type: 'Single Standard', status: 'Occupied', correlationId: null },
  { id: '102-guid', number: '102', type: 'Double Suite', status: 'Occupied', correlationId: null },
  { id: '103-guid', number: '103', type: 'King Deluxe', status: 'Occupied', correlationId: null },
  { id: '201-guid', number: '201', type: 'Single Standard', status: 'Occupied', correlationId: null },
  { id: '202-guid', number: '202', type: 'Double Suite', status: 'Occupied', correlationId: null },
  { id: '301-guid', number: '301', type: 'Penthouse Suite', status: 'Occupied', correlationId: null },
];

const loadInitialRooms = () => {
  try {
    const saved = localStorage.getItem('dhotel_rooms');
    if (saved) {
      const parsed = JSON.parse(saved);
      if (Array.isArray(parsed) && parsed.length > 0) return parsed;
    }
  } catch (e) {
    console.error('LocalStorage okunamadı:', e);
  }
  return defaultRooms;
};

export const useHotelStore = create((set) => ({
  rooms: loadInitialRooms(),
  logs: [],
  connectionStatus: 'disconnected',
  activeTab: 'overview', // 'overview' | 'frontdesk' | 'housekeeping' | 'maintenance'

  setActiveTab: (tab) => set({ activeTab: tab }),

  setConnectionStatus: (status) => set({ connectionStatus: status }),

  updateRoomStatus: (roomNumber, newStatus, correlationId) =>
    set((state) => {
      const updatedRooms = state.rooms.map((room) =>
        room.number === roomNumber
          ? { ...room, status: newStatus, correlationId: correlationId || room.correlationId }
          : room
      );
      try {
        localStorage.setItem('dhotel_rooms', JSON.stringify(updatedRooms));
      } catch (e) {
        console.error('LocalStorage kaydedilemedi:', e);
      }
      return { rooms: updatedRooms };
    }),

  addLog: (logItem) =>
    set((state) => ({
      logs: [
        {
          id: Date.now() + Math.random(),
          timestamp: new Date().toLocaleTimeString('tr-TR'),
          ...logItem,
        },
        ...state.logs,
      ].slice(0, 50),
    })),
}));

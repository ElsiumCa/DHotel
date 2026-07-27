import React, { useEffect } from 'react';
import { useHotelStore } from './store/hotelStore';
import { createSignalRConnection } from './services/signalr';
import { getSagaRoomStates } from './services/api';
import { Header } from './components/Header';
import { RoomCard } from './components/RoomCard';
import { AuditPanel } from './components/AuditPanel';
import { LayoutGrid, AlertCircle, Info } from 'lucide-react';

export function App() {
  const {
    rooms,
    activeTab,
    updateRoomStatus,
    addLog,
    setConnectionStatus,
  } = useHotelStore();

  useEffect(() => {
    // 0. F5 Yapıldığında MariaDB SagaDb Veritabanından Canlı Durumları Yükle
    getSagaRoomStates()
      .then((states) => {
        if (Array.isArray(states) && states.length > 0) {
          states.forEach((s) => {
            const roomNum = s.roomNumber || s.RoomNumber;
            const stateVal = s.currentState || s.CurrentState;
            const corrId = s.correlationId || s.CorrelationId;
            if (roomNum && stateVal) {
              updateRoomStatus(roomNum, stateVal, corrId);
            }
          });
          addLog({
            title: 'SagaDb Veritabanından Oda Durumları Yüklendi',
            details: `${states.length} adet canlı oda durumu MariaDB üzerinden senkronize edildi.`,
          });
        }
      })
      .catch((err) => {
        console.log('SagaDb okuma uyarısı:', err);
      });

    // SignalR Canlı WebSocket Bağlantısı Başlatma
    const connection = createSignalRConnection(
      (data) => {
        console.log('SignalR Canlı Bildirim Alındı:', data);
        
        // 1. Olay Türüne göre durum güncelleme
        let newStatus = 'Occupied';
        let logTitle = 'Olay Alındı';

        if (data.eventType === 'RoomReady') {
          newStatus = 'ReadyForCheckIn';
          logTitle = `✅ Oda ${data.roomNumber} GİRİŞE HAZIR!`;
        } else if (data.eventType === 'GuestCheckedIn') {
          newStatus = 'Occupied';
          logTitle = `🔑 Oda ${data.roomNumber} YENİ GİRİŞ YAPILDI! (${data.guestName || 'Misafir'})`;
        } else if (data.eventType === 'CleaningStarted') {
          newStatus = 'InCleaning';
          logTitle = `🧹 Oda ${data.roomNumber} Temizliği Başladı (${data.cleaner || 'Görevli'})`;
        } else if (data.eventType === 'DamageReported') {
          newStatus = 'InMaintenance';
          logTitle = `⚠️ Oda ${data.roomNumber} ARIZALI/BAKIMDA (${data.description || 'Arıza'})`;
        }

        // 2. Zustand State Güncelleme
        updateRoomStatus(data.roomNumber, newStatus, data.correlationId);

        // 3. Canlı Log Paneline Ekleme
        addLog({
          title: logTitle,
          details: `CorrelationId: ${data.correlationId}`,
          type: data.eventType,
        });
      },
      (status) => {
        setConnectionStatus(status);
      }
    );

    connection
      .start()
      .then(() => {
        setConnectionStatus('connected');
        addLog({
          title: 'SignalR Gateway Bağlantısı Kuruldu',
          details: 'WebSocket kanalı dinleniyor (http://localhost:5000/hubs/room)',
        });
      })
      .catch((err) => {
        console.error('SignalR Bağlantı Hatası:', err);
        setConnectionStatus('disconnected');
      });

    return () => {
      connection.stop();
    };
  }, [updateRoomStatus, addLog, setConnectionStatus]);

  // Sekmelere Göre Oda Filtreleme
  const filteredRooms = rooms.filter((room) => {
    if (activeTab === 'frontdesk') return room.status === 'Occupied' || room.status === 'ReadyForCheckIn';
    if (activeTab === 'housekeeping') return room.status === 'AwaitingCleaning' || room.status === 'InCleaning';
    if (activeTab === 'maintenance') return room.status === 'InMaintenance';
    return true; // 'overview' -> Tüm odalar
  });

  return (
    <div className="app-container">
      <Header />

      <main className="main-content">
        <div>
          <div className="section-header">
            <div className="section-title">
              <LayoutGrid size={20} className="text-blue-400" />
              <span>
                {activeTab === 'overview' && 'Tüm Otel Odaları ve Anlık Durum Haritası'}
                {activeTab === 'frontdesk' && 'Resepsiyon Yönetim Paneli (Çıkış İşlemleri)'}
                {activeTab === 'housekeeping' && 'Kat Görevlisi Temizlik Paneli'}
                {activeTab === 'maintenance' && 'Teknik Servis Arıza Paneli'}
              </span>
            </div>
            <span style={{ fontSize: '0.85rem', color: '#94a3b8' }}>
              Toplam {filteredRooms.length} Oda Gösteriliyor
            </span>
          </div>

          {filteredRooms.length === 0 ? (
            <div
              style={{
                backgroundColor: '#131b2e',
                border: '1px solid #243252',
                borderRadius: '10px',
                padding: '3rem',
                textAlign: 'center',
                color: '#94a3b8',
              }}
            >
              <Info size={32} style={{ marginBottom: '0.5rem', color: '#64748b' }} />
              <div>Bu görünüm için şu anda gösterilecek oda bulunmuyor.</div>
            </div>
          ) : (
            <div className="room-grid">
              {filteredRooms.map((room) => (
                <RoomCard key={room.number} room={room} />
              ))}
            </div>
          )}
        </div>

        {/* Sağ Taraf: Real-Time Audit Log Paneli */}
        <AuditPanel />
      </main>
    </div>
  );
}

export default App;

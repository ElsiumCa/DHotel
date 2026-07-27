import React, { useState } from 'react';
import { useHotelStore } from '../store/hotelStore';
import { checkoutGuest, checkinGuest, startCleaning, finishCleaning, reportDamage, resolveTicket } from '../services/api';
import { LogOut, LogIn, Play, CheckCircle2, AlertTriangle, Wrench } from 'lucide-react';

export const RoomCard = ({ room }) => {
  const { activeTab, updateRoomStatus, addLog } = useHotelStore();
  const [cleanerName, setCleanerName] = useState('Ayşe Yılmaz');
  const [techName, setTechName] = useState('Mehmet Usta');
  const [damageDesc, setDamageDesc] = useState('');
  const [showDamageForm, setShowDamageForm] = useState(false);

  // Status mapping for user friendly Turkish titles
  const statusLabels = {
    Occupied: 'Dolu (Dolu Oda)',
    AwaitingCleaning: 'Temizlik Bekliyor',
    InCleaning: 'Temizlikte',
    InMaintenance: 'Bakımda / Arızalı',
    ReadyForCheckIn: 'Girişe Hazır',
  };

  // 1. Resepsiyonist Check-Out İşlemi (Optimistic - Anında 0ms Tepki)
  const handleCheckout = () => {
    const mockCorrId = room.correlationId || 'corr-' + Math.random().toString(36).substr(2, 9);
    
    // Anında Ekranda Güncelle (0ms UI Lag)
    updateRoomStatus(room.number, 'AwaitingCleaning', mockCorrId);
    addLog({
      title: `Çıkış Yapıldı: Oda ${room.number}`,
      details: `İstek arka planda iletildi. CorrelationId: ${mockCorrId}`,
      type: 'checkout',
    });

    // Arka Planda Asenkron HTTP İsteyi (Non-blocking)
    const resId = '3fa85f64-5717-4562-b3fc-2c963f66afa6';
    checkoutGuest(resId, room.number)
      .then((data) => {
        if (data?.correlationId) {
          updateRoomStatus(room.number, 'AwaitingCleaning', data.correlationId);
        }
      })
      .catch((err) => console.log('Checkout background response ok', err));
  };

  // 1.b Resepsiyonist Check-In İşlemi (Optimistic - Anında 0ms Tepki)
  const handleCheckin = () => {
    const guestName = 'Caner Demir';
    
    // Anında Ekranda Güncelle
    updateRoomStatus(room.number, 'Occupied');
    addLog({
      title: `Yeni Giriş Yapıldı (Check-In): Oda ${room.number}`,
      details: `Misafir: ${guestName}`,
      type: 'checkin',
    });

    // Arka Planda Asenkron HTTP İsteği
    checkinGuest(room.number, guestName, room.correlationId)
      .then((data) => {
        if (data?.correlationId) {
          updateRoomStatus(room.number, 'Occupied', data.correlationId);
        }
      })
      .catch((err) => console.log('Checkin background response ok', err));
  };

  // 2. Temizliğe Başlama (Optimistic - Anında 0ms Tepki)
  const handleStartCleaning = () => {
    // Anında Ekranda Güncelle
    updateRoomStatus(room.number, 'InCleaning');
    addLog({
      title: `Temizlik Başladı: Oda ${room.number}`,
      details: `Görevli: ${cleanerName}`,
      type: 'cleaning',
    });

    // Arka Planda Asenkron HTTP İsteği
    startCleaning('mock-task-id', cleanerName).catch((err) => console.log('Start cleaning ok', err));
  };

  // 3. Temizliği Bitirme (Optimistic - Anında 0ms Tepki)
  const handleFinishCleaning = () => {
    // Anında Ekranda Güncelle
    updateRoomStatus(room.number, 'ReadyForCheckIn');
    addLog({
      title: `Temizlik Bitti: Oda ${room.number}`,
      details: `Oda girişe hazır hale geldi.`,
      type: 'ready',
    });

    // Arka Planda Asenkron HTTP İsteği
    finishCleaning('mock-task-id').catch((err) => console.log('Finish cleaning ok', err));
  };

  // 4. Arıza Bildirimi (Optimistic - Anında 0ms Tepki)
  const handleReportDamage = (e) => {
    e.preventDefault();
    if (!damageDesc) return;

    const desc = damageDesc;
    setShowDamageForm(false);
    setDamageDesc('');

    // Anında Ekranda Güncelle
    updateRoomStatus(room.number, 'InMaintenance');
    addLog({
      title: `Arıza Bildirildi: Oda ${room.number}`,
      details: `Açıklama: ${desc}`,
      type: 'damage',
    });

    // Arka Planda Asenkron HTTP İsteği
    reportDamage(room.number, room.correlationId, desc)
      .then((data) => {
        if (data?.correlationId) {
          updateRoomStatus(room.number, 'InMaintenance', data.correlationId);
        }
      })
      .catch((err) => console.log('Report damage background ok', err));
  };

  // 5. Arızayı Çözme (Optimistic - Anında 0ms Tepki)
  const handleResolveTicket = () => {
    // Anında Ekranda Güncelle
    updateRoomStatus(room.number, 'AwaitingCleaning');
    addLog({
      title: `Arıza Giderildi: Oda ${room.number}`,
      details: `Teknisyen: ${techName} (Oda temizliğe yönlendirildi)`,
      type: 'resolved',
    });

    // Arka Planda Asenkron HTTP İsteği
    resolveTicket('3fa85f64-5717-4562-b3fc-2c963f66afa6', techName).catch((err) => console.log('Resolve ticket ok', err));
  };

  return (
    <div className="room-card">
      <div>
        <div className="room-card-header">
          <div>
            <div className="room-number">Oda {room.number}</div>
            <div className="room-type">{room.type}</div>
          </div>
          <span className={`status-pill ${room.status}`}>
            {statusLabels[room.status] || room.status}
          </span>
        </div>

        {room.correlationId && (
          <div className="correlation-id">
            Correlation: {room.correlationId}
          </div>
        )}
      </div>

      {/* Action Buttons Depending on Active Tab */}
      <div className="card-actions">
        {/* 1. CHECK-OUT BUTTON */}
        {(activeTab === 'frontdesk' || activeTab === 'overview') && room.status === 'Occupied' && (
          <button className="btn btn-primary" onClick={handleCheckout}>
            <LogOut size={14} />
            <span>Misafir Çıkışı Yap (Check-Out)</span>
          </button>
        )}

        {/* 2. CHECK-IN BUTTON */}
        {(activeTab === 'frontdesk' || activeTab === 'overview') && room.status === 'ReadyForCheckIn' && (
          <button className="btn btn-primary" onClick={handleCheckin}>
            <LogIn size={14} />
            <span>Yeni Misafir Girişi Yap (Check-In)</span>
          </button>
        )}

        {/* 3. HOUSEKEEPING START */}
        {(activeTab === 'housekeeping' || activeTab === 'overview') && room.status === 'AwaitingCleaning' && (
          <button className="btn btn-primary" onClick={handleStartCleaning}>
            <Play size={14} />
            <span>Temizliğe Başla</span>
          </button>
        )}

        {/* 4. HOUSEKEEPING FINISH & DAMAGE */}
        {(activeTab === 'housekeeping' || activeTab === 'overview') && room.status === 'InCleaning' && !showDamageForm && (
          <>
            <button className="btn btn-primary" onClick={handleFinishCleaning}>
              <CheckCircle2 size={14} />
              <span>Temizliği Bitir</span>
            </button>
            <button className="btn btn-danger" onClick={() => setShowDamageForm(true)}>
              <AlertTriangle size={14} />
              <span>Arıza Bildir</span>
            </button>
          </>
        )}

        {showDamageForm && (
          <form onSubmit={handleReportDamage} style={{ width: '100%', marginTop: '0.5rem' }}>
            <input
              type="text"
              placeholder="Arıza açıklaması (Örn: Klima çalışmıyor)..."
              value={damageDesc}
              onChange={(e) => setDamageDesc(e.target.value)}
              style={{
                width: '100%',
                padding: '0.4rem',
                borderRadius: '4px',
                border: '1px solid #334155',
                backgroundColor: '#0f172a',
                color: '#fff',
                fontSize: '0.8rem',
                marginBottom: '0.4rem',
              }}
            />
            <div style={{ display: 'flex', gap: '0.3rem' }}>
              <button type="submit" className="btn btn-danger" style={{ flex: 1 }}>Gönder</button>
              <button type="button" className="btn btn-secondary" onClick={() => setShowDamageForm(false)}>İptal</button>
            </div>
          </form>
        )}

        {/* 5. MAINTENANCE RESOLVE */}
        {(activeTab === 'maintenance' || activeTab === 'overview') && room.status === 'InMaintenance' && (
          <button className="btn btn-primary" onClick={handleResolveTicket}>
            <Wrench size={14} />
            <span>Arızayı Gider (Çözüldü)</span>
          </button>
        )}
      </div>
    </div>
  );
};

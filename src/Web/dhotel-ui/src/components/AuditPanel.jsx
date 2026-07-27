import React from 'react';
import { useHotelStore } from '../store/hotelStore';
import { Activity } from 'lucide-react';

export const AuditPanel = () => {
  const { logs } = useHotelStore();

  return (
    <div className="audit-panel">
      <div className="section-title">
        <Activity size={18} className="text-blue-400" />
        <span>Canlı Olay & Akış Logu</span>
      </div>

      <div className="log-list">
        {logs.length === 0 ? (
          <div style={{ color: '#64748b', fontSize: '0.85rem', textAlign: 'center', padding: '2rem 0' }}>
            Henüz bir işlem veya canlı event gerçekleşmedi.
          </div>
        ) : (
          logs.map((log) => (
            <div key={log.id} className="log-item">
              <div className="log-time">{log.timestamp}</div>
              <div className="log-title">{log.title}</div>
              {log.details && <div className="log-details">{log.details}</div>}
            </div>
          ))
        )}
      </div>
    </div>
  );
};

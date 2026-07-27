import React from 'react';
import { useHotelStore } from '../store/hotelStore';
import { Building2, ConciergeBell, Sparkles, Wrench, LayoutGrid } from 'lucide-react';

export const Header = () => {
  const { activeTab, setActiveTab, connectionStatus } = useHotelStore();

  const tabs = [
    { id: 'overview', label: 'Oda Haritası', icon: LayoutGrid },
    { id: 'frontdesk', label: 'Resepsiyon', icon: ConciergeBell },
    { id: 'housekeeping', label: 'Kat Görevlisi', icon: Sparkles },
    { id: 'maintenance', label: 'Teknik Servis', icon: Wrench },
  ];

  return (
    <header className="app-header">
      <div className="brand-title">
        <Building2 size={24} className="text-blue-500" />
        <span>DHotel Operations</span>
        <span className="brand-badge">SAGA Real-time</span>
      </div>

      <nav className="nav-tabs">
        {tabs.map((tab) => {
          const Icon = tab.icon;
          return (
            <button
              key={tab.id}
              className={`tab-button ${activeTab === tab.id ? 'active' : ''}`}
              onClick={() => setActiveTab(tab.id)}
            >
              <Icon size={16} />
              <span>{tab.label}</span>
            </button>
          );
        })}
      </nav>

      <div className="connection-status">
        <div className={`status-dot ${connectionStatus === 'connected' ? 'connected' : ''}`} />
        <span>
          {connectionStatus === 'connected'
            ? 'Canlı Bağlantı Aktif (SignalR)'
            : connectionStatus === 'reconnecting'
            ? 'Yeniden Bağlanılıyor...'
            : 'Simülasyon Modu (Gateway Kapalı)'}
        </span>
      </div>
    </header>
  );
};

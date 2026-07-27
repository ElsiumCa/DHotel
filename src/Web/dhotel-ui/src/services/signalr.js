import * as signalR from '@microsoft/signalr';

const SIGNALR_URL = 'http://localhost:5000/hubs/room';

export const createSignalRConnection = (onMessageReceived, onConnectionStateChange) => {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(SIGNALR_URL, {
      skipNegotiation: false,
      transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

  connection.on('ReceiveRoomStatus', (data) => {
    if (onMessageReceived) {
      onMessageReceived(data);
    }
  });

  connection.onreconnecting(() => {
    if (onConnectionStateChange) onConnectionStateChange('reconnecting');
  });

  connection.onreconnected(() => {
    if (onConnectionStateChange) onConnectionStateChange('connected');
  });

  connection.onclose(() => {
    if (onConnectionStateChange) onConnectionStateChange('disconnected');
  });

  return connection;
};

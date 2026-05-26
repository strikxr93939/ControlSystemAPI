import * as signalR from '@microsoft/signalr';

const API_BASE = process.env.REACT_APP_API_URL || 'http://localhost:5186';

let connection = null;

export function getConnection() {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE}/hubs/monitoring`)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();
  }
  return connection;
}

export { API_BASE };

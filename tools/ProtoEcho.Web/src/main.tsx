import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { keycloak } from './keycloak.ts'
import { connection } from './signalr.ts'
import "bootstrap/dist/css/bootstrap.min.css";
import './index.css'
import App from './App.tsx'

keycloak.init({
  onLoad: 'login-required',
  pkceMethod: 'S256',
})
.then(async () => {
  console.log("Token audience:", JSON.parse(atob(keycloak.token!.split(".")[1])).aud);

  await connection.start();

  console.log("SignalR connected!");

  createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
  );
})
.catch((error) => {
  console.error('Keycloak initialization failed:', error);
});
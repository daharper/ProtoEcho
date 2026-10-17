import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { keycloak } from './keycloak.ts'
import './index.css'
import App from './App.tsx'


keycloak.init({
  onLoad: 'login-required',
  pkceMethod: 'S256',
})
.then(() => {
  createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
  );
})
.catch((error) => {
  console.error('Keycloak initialization failed:', error);
});
# ProtoEcho.Web

A React and TypeScript web client for the **ProtoEcho** ASP.NET Core SignalR application.

This project demonstrates authenticated, real-time communication between a browser and an ASP.NET Core backend using **Keycloak**, **OpenID Connect**, and **SignalR**.

It is part of the larger ProtoEcho solution, which also includes a .NET console client (`AgentEcho`).

## Technology Stack

- **React** — user interface and component state
- **TypeScript** — type-safe application code
- **Vite** — development server and build tooling
- **Bootstrap 5** — responsive layout, components, and dark theme
- **ASP.NET Core SignalR JavaScript client** — real-time server communication
- **Keycloak JS adapter** — OpenID Connect authentication using Authorization Code Flow with PKCE

## Features

- Interactive login through Keycloak
- Authentication using a public OIDC client with PKCE (S256)
- Access-token renewal before SignalR connection attempts
- Authenticated SignalR communication over WebSockets
- Automatic SignalR reconnection
- Editable message input
- Echo responses displayed using React state
- Bootstrap 5 dark-themed interface

## Architecture

```text
Browser
  |
  | OIDC Authorization Code Flow + PKCE
  v
Keycloak (localhost:8180)
  |
  | Access token
  v
React / TypeScript
  |
  | Authenticated SignalR connection
  | WebSocket
  v
ProtoEcho.Server (localhost:5000)
  |
  | EchoHub.Echo(message)
  |
  | AgentEcho event
  v
React state update
  |
  v
Browser UI
```

The browser authenticates with Keycloak and receives an access token. The SignalR client supplies this token when establishing its connection to the ASP.NET Core backend.

The backend validates the token's issuer, audience, signature and lifetime before allowing access to the SignalR hub.

## Prerequisites

- Node.js and npm
- A running `ProtoEcho.Server` instance
- A running Keycloak instance with the `protoecho` realm configured
- A Keycloak user permitted to sign in to the web client

### Local development endpoints

| Service | URL |
|---|---|
| React / Vite | `http://localhost:5173` |
| ProtoEcho.Server | `http://localhost:5000` |
| SignalR hub | `http://localhost:5000/hubs/echo` |
| Keycloak | `http://localhost:8180` |
| Keycloak realm | `protoecho` |

These URLs are currently configured for local development.

## Keycloak Configuration

The React application uses the following OIDC client:

| Setting | Value |
|---|---|
| Client ID | `protoecho-web` |
| Client authentication | Off (public client) |
| Standard flow | Enabled |
| PKCE method | S256 |
| Valid redirect URIs | `http://localhost:5173/*` |
| Web origins | `http://localhost:5173` |

The client must also have an **Audience** protocol mapper configured:

- **Included Custom Audience:** `protoecho`
- **Add to access token:** Enabled

This ensures the access token contains the audience expected by `ProtoEcho.Server`.

The browser client does **not** use a client secret.

## Running Locally

Start Keycloak and `ProtoEcho.Server` first.

From the `tools/ProtoEcho.Web` directory, install dependencies:

```bash
npm install
```

Start the Vite development server:

```bash
npm run dev
```

Open:

```text
http://localhost:5173
```

You will be redirected to Keycloak to sign in.

After authentication, the application establishes an authenticated SignalR connection.

Enter a message and select **Send Echo**. The server's response will appear in the interface.

## Project Structure

```text
src/
├── App.tsx        # React UI and echo interaction
├── keycloak.ts    # Keycloak client configuration
├── signalr.ts     # SignalR connection configuration
├── main.tsx       # Authentication and application startup
└── index.css      # Application-specific CSS
```

## Authentication and SignalR

Authentication is initialized before the React application is rendered.

The SignalR connection uses an `accessTokenFactory` that calls `keycloak.updateToken(30)` to refresh the access token when necessary.

The server accepts bearer tokens from the `access_token` query parameter for the SignalR hub endpoint. This is required because browser WebSocket APIs cannot set arbitrary HTTP authorization headers.

**Security considerations:**

- Use HTTPS/WSS outside local development.
- Never store a client secret in browser code.
- Avoid logging access tokens or token-bearing URLs.
- Restrict query-string token handling to the SignalR endpoint.
- Do not commit credentials or access tokens to source control.

## Development Commands

```bash
npm run dev       # Start development server
npm run build     # Type-check and build for production
npm run lint      # Run ESLint
npm run preview   # Preview the production build locally
```

## Current Scope

ProtoEcho.Web is a development and learning application rather than a production-ready frontend.

Its current purpose is to demonstrate the integration of React, TypeScript, Keycloak authentication and ASP.NET Core SignalR through a small end-to-end echo workflow.

The broader ProtoEcho solution also contains `AgentEcho`, a .NET console client that authenticates using OAuth 2.0 client credentials, providing a useful contrast with the browser's interactive OIDC authentication flow.

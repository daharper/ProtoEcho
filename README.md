# ProtoEcho

**Exploring ASP.NET Core SignalR, Keycloak authentication, and real-time communication across .NET and React clients.**

ProtoEcho is a small experimental solution built to investigate authenticated, bidirectional communication using ASP.NET Core SignalR.

The objective was explore SignalR as a potential messaging server, and to see how it behaves across different client technologies and authentication flows, including connection establishment, access-token management, message exchange, disconnection, and reconnection.

The solution consists of a shared ASP.NET Core backend and two clients: a C# console application and a React/TypeScript web application.

## Objectives

The experiment focused on several areas:

- **Authentication:** Integrating ASP.NET Core with Keycloak using OAuth 2.0 and OpenID Connect.
- **Different client identities:** Comparing machine-to-machine authentication with interactive browser authentication.
- **Real-time communication:** Establishing authenticated SignalR connections and exchanging messages between clients and the server.
- **Token lifecycle:** Acquiring, caching, renewing, and supplying access tokens as connections are established or re-established.
- **Connection lifecycle:** Exploring disconnection, automatic reconnection, and recovery after temporary server outages.
- **Transport behaviour:** Understanding how SignalR authentication differs between .NET clients and browser WebSocket connections.

## Architecture

```text
                           Keycloak
                        (Identity Provider)
                               |
                +--------------+--------------+
                |                             |
                | OAuth 2.0                   | OpenID Connect
                | Client Credentials          | Authorization Code + PKCE
                |                             |
                v                             v
        +----------------+           +------------------+
        |   AgentEcho    |           |  ProtoEcho.Web   |
        |                |           |                  |
        | C# / .NET      |           | React / TypeScript|
        | Console Client |           | Browser Client   |
        +----------------+           +------------------+
                |                             |
                |     Authenticated SignalR   |
                |                             |
                +--------------+--------------+
                               |
                               v
                     +------------------+
                     | ProtoEcho.Server |
                     |                  |
                     | ASP.NET Core     |
                     | SignalR EchoHub  |
                     | JWT Bearer Auth  |
                     +------------------+
```

Both clients authenticate independently with Keycloak, obtain access tokens, and connect to the same protected SignalR hub.

The server validates the tokens and exposes hub methods for receiving messages and sending responses back to the originating client.

## Solution Components

### ProtoEcho.Server

**ASP.NET Core · SignalR · JWT Bearer Authentication**

The backend hosts an authenticated SignalR hub that accepts messages from connected clients and sends echo responses using `Clients.Caller`.

Its responsibilities include:

- Validating Keycloak-issued JWT access tokens.
- Enforcing the expected token audience and lifetime.
- Protecting the SignalR hub with authorization.
- Accepting authenticated connections from both .NET and browser clients.
- Handling browser WebSocket authentication through the SignalR-specific `access_token` query parameter.

The server deliberately contains very little application logic so the experiment can concentrate on authentication and communication.

### AgentEcho

**C# · .NET Console Application · SignalR Client**

AgentEcho represents a non-interactive, machine-to-machine client.

It authenticates with Keycloak using the **OAuth 2.0 Client Credentials** grant and a confidential client identity.

The experiment explores:

- Requesting access tokens without user interaction.
- Caching tokens and renewing them before expiration.
- Supplying access tokens through SignalR's `AccessTokenProvider`.
- Invoking server-side hub methods and receiving responses.
- Monitoring the connection lifecycle through `Reconnecting`, `Reconnected`, and `Closed` callbacks.
- Recovering from temporary backend outages through automatic reconnection.

### ProtoEcho.Web

**React · TypeScript · Vite · Bootstrap 5 · SignalR JavaScript Client**

ProtoEcho.Web represents an interactive browser client.

It authenticates users through Keycloak using the **OpenID Connect Authorization Code Flow with PKCE**, without embedding a client secret in the application.

The experiment explores:

- Interactive login and token acquisition.
- Initializing authentication before establishing the SignalR connection.
- Renewing access tokens when required.
- Establishing authenticated browser WebSocket connections.
- Invoking server-side hub methods from TypeScript.
- Receiving SignalR events and updating React component state.
- Configuring automatic reconnection.

The interface is intentionally minimal: a message input, a Send Echo button, and the latest server response.

## Authentication Models

One of the central goals was to compare two different approaches to client authentication against the same backend.

| Aspect | AgentEcho | ProtoEcho.Web |
|---|---|---|
| Client environment | .NET console | Browser |
| Authentication | OAuth 2.0 Client Credentials | OIDC Authorization Code + PKCE |
| Identity | Application / service | Authenticated user |
| Keycloak client | Confidential | Public |
| Client secret | Yes | No |
| Token management | Custom C# token provider | Keycloak JavaScript adapter |
| SignalR client | .NET | JavaScript / TypeScript |

Both clients obtain tokens containing the audience expected by ProtoEcho.Server.

The browser client also illustrates a transport-specific detail: WebSocket authentication may require the access token to be supplied through the connection URL because browser WebSocket APIs cannot set arbitrary HTTP authorization headers.

## SignalR Communication

The application uses a simple echo workflow to demonstrate bidirectional communication.

```text
Client                          ProtoEcho.Server
  |                                    |
  |  Establish authenticated           |
  |  SignalR connection                |
  |----------------------------------->|
  |                                    |
  |  Invoke Echo(message)              |
  |----------------------------------->|
  |                                    |
  |          AgentEcho(message)        |
  |<-----------------------------------|
  |                                    |
  |  Process/display response          |
  |                                    |
```

Although the application logic is deliberately simple, the communication exercises the same SignalR mechanisms used by more sophisticated real-time applications.

## Connection and Token Lifecycle

An important part of the experiment was understanding what happens beyond the initial successful connection.

For the .NET console client, this included caching and refreshing access tokens, observing reconnection callbacks, and testing recovery after the SignalR server became temporarily unavailable.

For the React client, this included refreshing tokens through the Keycloak adapter and handling the differences between HTTP negotiation and browser WebSocket authentication.

The experiment distinguishes two related but separate concerns:

- **Authentication lifecycle:** Obtaining and renewing credentials used to establish authenticated connections.
- **Connection lifecycle:** Establishing, losing, and recovering the underlying SignalR connection.

SignalR's automatic reconnection functionality addresses connection recovery; token acquisition and renewal remain the responsibility of the client authentication implementation.

## Scope and Status

ProtoEcho is a **completed local proof of concept** for the two implemented clients.

It demonstrates authenticated SignalR communication using two distinct client technologies and authentication flows, with particular attention to token handling, transport behaviour, and connection lifecycle management.

The project is intentionally small and is not presented as a production-ready authentication or messaging system.

import * as signalR from "@microsoft/signalr"
import { keycloak } from "./keycloak.ts"

export const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/echo", {
        accessTokenFactory: async() => {
            await keycloak.updateToken(30); 
            return keycloak.token!;
        },
    })
    .configureLogging(signalR.LogLevel.Warning)
    .withAutomaticReconnect()    
    .build();


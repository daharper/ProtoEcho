import { useEffect, useState } from "react";
import { connection } from "./signalr";

function App() {
    const [echoResponse, setEchoResponse] = useState("No response yet");
    const [message, setMessage] = useState("Hello from React!");

    useEffect(() => {
        const handleEcho = (message: string) => {
            setEchoResponse(message);
        };

        connection.on("AgentEcho", handleEcho);

        return () => {
            connection.off("AgentEcho", handleEcho);
        };
    }, []);

    const sendEcho = async () => {
        try {
            await connection.invoke("Echo", message);
        } catch (error) {
            console.error("Failed to send echo: ", error);
        }
    };

    return (
      <main className="container py-5">
        <div className="row justify-content-center">
          <div className="col-12 col-md-8 col-lg-6">
            <div className="card shadow-sm">
              <div className="card-body p-4">
                <h1 className="h3 card-title mb-4">ProtoEcho</h1>

                <p className="mb-4">
                  Server response: {echoResponse}
                </p>

                <div className="mb-3">
                  <label htmlFor="echoMessage" className="form-label">Message</label>
                  <input 
                      id="echoMessage"
                      type="text"
                      className="form-control"
                      value={message}
                      onChange={(e) => setMessage(e.target.value)}
                  />
                </div>

                <button className="btn btn-primary mt-2" onClick={sendEcho}>
                  Send Echo
                </button>
              </div>
            </div>
          </div>
        </div>
      </main>
    );
}            

export default App;
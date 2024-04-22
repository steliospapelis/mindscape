from pythonosc import dispatcher
from pythonosc import osc_server
import threading
import os

def ppg_green_handler(address, *args):
    if "PPG:GRN" in address:  # Check if the message is for PPG Green
        message = f"Received PPG Green from {address}: {args}"
        print(message)
        # Save the full message to output.txt
        with open("output.txt", "a") as file:
            file.write(message + "\n")
        # Save just the PPG values (arguments) to output_args.txt
        with open("output_args.txt", "a") as file:
            file.write(f"{args}\n")

def run_osc_listener(ip, port, stop_event):
    disp = dispatcher.Dispatcher()
    disp.set_default_handler(ppg_green_handler)  # Use the PPG Green handler
    
    server = osc_server.BlockingOSCUDPServer((ip, port), disp)
    print(f"Listening on {ip}:{port}")
    
    # Serve until told to stop
    while not stop_event.is_set():
        server.handle_request()

def listen_for_exit(stop_event):
    input("Press 'Enter' to stop listening...\n")
    stop_event.set()

def main():
    ip = '127.0.0.1'  # Localhost, adjust if EmotiBit is sending to a different IP
    port = 12345      # The port where EmotiBit sends OSC data, adjust accordingly

    # Event to signal when the server should stop
    stop_event = threading.Event()

    # Run the OSC server in a separate thread
    listener_thread = threading.Thread(target=run_osc_listener, args=(ip, port, stop_event))
    listener_thread.start()

    # Wait for the user to signal to exit
    listen_for_exit(stop_event)

    # Wait for the server thread to finish
    listener_thread.join()
    print("Server has been stopped.")

if __name__ == "__main__":
    main()

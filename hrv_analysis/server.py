from pythonosc import dispatcher 
from pythonosc import osc_server
from flask import Flask, jsonify, render_template_string
import threading
import queue
import signal
import sys
import socket  # For sending dummy data to unblock the listener
from data_processor import data_processor, get_results

# Global queue for sharing data between threads
data_queue = queue.Queue()
file_lock = threading.Lock()

app = Flask(__name__)

# Serve the HTML page at the root route
@app.route('/')
def index():
    html_content = '''
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>HRV Results</title>
    </head>
    <body>
        <h1>Welcome to HRV Analysis</h1>
        <p>Click the button below to view the HRV results in real time.</p>
        <button onclick="window.location.href='/results'">View Results (Table)</button>
        <button onclick="window.location.href='/results_json'">View Results (JSON)</button>
    </body>
    </html>
    '''
    return render_template_string(html_content)

@app.route('/results', methods=['GET'])
def results():
    results = get_results()
    
    # Define a simple HTML page to display the results
    html_content = '''
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>HRV Results</title>
        <style>
            table { width: 50%; margin: 50px auto; border-collapse: collapse; }
            table, th, td { border: 1px solid black; }
            th, td { padding: 10px; text-align: center; }
        </style>
    </head>
    <body>
        <h1 style="text-align: center;">HRV Analysis Results</h1>
        <table>
            <tr>
                <th>Current HRV</th>
                <td>{{ results['current_hrv'] }}</td>
            </tr>
            <tr>
                <th>Binary Output (0: calm, 1: anxious)</th>
                <td>{{ results['binary_output'] }}</td>
            </tr>
            <tr>
                <th>Categorical Output (0: very calm, 1: calm, 2: anxious, 3: very anxious)</th>
                <td>{{ results['previous_hrv'] }}</td>
            </tr>
        </table>
    </body>
    </html>
    '''
    
    return render_template_string(html_content, results=results)

# Route for returning the raw JSON data
@app.route('/results_json')
def get_json_results():
    results = get_results()
    return jsonify(results)

def ppg_green_handler(address, *args):
    if "PPG:GRN" in address:
        for arg in args:
            data_queue.put(arg)  # Put the PPG values into the queue

def run_osc_listener(ip, port, stop_event):
    disp = dispatcher.Dispatcher()
    disp.set_default_handler(ppg_green_handler)
    
    # Create the OSC server
    server = osc_server.BlockingOSCUDPServer((ip, port), disp)
    print(f"Listening on {ip}:{port}")
    
    # Run the server and handle requests until stop_event is set
    try:
        while not stop_event.is_set():
            server.handle_request()  # This is blocking, but we will terminate it with stop_event
    finally:
        print("Stopping OSC listener.")
        server.server_close()  # Close the server to free up the port and exit

def send_dummy_packet(ip, port):
    """Sends a dummy packet to the OSC listener to unblock it."""
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        sock.sendto(b"dummy", (ip, port))  # Send a dummy request to unblock the listener
        sock.close()
        print("Dummy packet sent to unblock OSC listener.")
    except Exception as e:
        print(f"Error sending dummy packet: {e}")

def run_flask():
    app.run(host='0.0.0.0', port=5000)

# Handle shutdown signals (e.g., Ctrl+C) and stop all threads gracefully
def handle_shutdown(signal, frame, stop_event, ip, port):
    print("Shutdown signal received. Stopping server...")
    stop_event.set()  # Signal all threads to stop

    # Send a dummy packet to the OSC listener to unblock it
    send_dummy_packet(ip, port)

    sys.exit(0)  # Exit the program

def main():
    ip = '127.0.0.1'
    port = 12345
    stop_event = threading.Event()

    # Handle Ctrl+C signal to stop the server
    signal.signal(signal.SIGINT, lambda signal, frame: handle_shutdown(signal, frame, stop_event, ip, port))

    # Run the OSC server in a separate thread
    listener_thread = threading.Thread(target=run_osc_listener, args=(ip, port, stop_event))
    listener_thread.start()

    # Run the data processor in a separate thread
    data_thread = threading.Thread(target=data_processor, args=(data_queue, stop_event))
    data_thread.start()

    # Run the Flask server in the main thread
    print("Starting Flask server...")
    app.run(host='0.0.0.0', port=5000, use_reloader=False)

    # Wait for all threads to finish (this ensures proper termination)
    listener_thread.join()
    data_thread.join()
    print("Server has been stopped.")

if __name__ == "__main__":
    main()

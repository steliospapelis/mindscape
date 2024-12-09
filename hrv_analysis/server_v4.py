from pythonosc import dispatcher
from pythonosc import osc_server
from flask import Flask, jsonify, request, render_template
import threading
import queue
import signal
import sys
import socket  # For sending dummy data to unblock the listener
from data_processor_v4 import data_processor, get_analysis_results

# Global queues for sharing data between threads
ppg_data_queue = queue.Queue()  # Queue for PPG green data
eda_data_queue = queue.Queue()  # Queue for EDA data
file_lock = threading.Lock()

app = Flask(__name__)

# Serve the HTML page at the root route
@app.route('/')
def index():
    return render_template('index.html')    # change to overview.html (no buttons or other pages)

# Route for returning the raw JSON data
@app.route('/analysis_results')
def get_analysis_results():
    results = get_analysis_results()
    return jsonify(results)

# Global variables for game flags
calm_calibration_value = 0
anxious_calibration_value = 0
ability_value = 0

@app.route('/game_flags_post', methods=['POST'])
def game_flags_post():
    global ability_value
    global anxious_calibration_value
    global calm_calibration_value
    if request.is_json:
        data = request.get_json()
        # with open("dummy_data.txt", 'a') as f:
        #     f.write(f"Data: {data}")
        anxious_calibration_value = data.get('StressedCalib', 0)
        calm_calibration_value = data.get('CalmCalib', 0)
        ability_value = data.get('Breathing', 0)
    else:
        anxious_calibration_value = 0
        calm_calibration_value = 0
        ability_value = 0
        
    return jsonify(anxious_calibration_value=anxious_calibration_value, calm_calibration_value=calm_calibration_value, ability_value=ability_value), 200

@app.route('/game_flags_get', methods=['GET'])
def gameFlags():
    """Route to return the current game flags values stored in the global variables."""        
    return jsonify(anxious_calibration_value=anxious_calibration_value, calm_calibration_value=calm_calibration_value, ability_value=ability_value), 200

@app.route('/game_flags', methods=['POST', 'GET'])
def game_flags():
    global ability_value
    global anxious_calibration_value
    global calm_calibration_value

    if request.method == 'POST':
        if request.is_json:
            data = request.get_json()
            anxious_calibration_value = data.get('StressedCalib', 0)
            calm_calibration_value = data.get('CalmCalib', 0)
            ability_value = data.get('Breathing', 0)
        else:
            anxious_calibration_value = 0
            calm_calibration_value = 0
            ability_value = 0

        return jsonify(
            message="Game flags updated",
            anxious_calibration_value=anxious_calibration_value,
            calm_calibration_value=calm_calibration_value,
            ability_value=ability_value
        ), 200

    elif request.method == 'GET':
        return jsonify(
            anxious_calibration_value=anxious_calibration_value,
            calm_calibration_value=calm_calibration_value,
            ability_value=ability_value
        ), 200

# Functions to retrieve the game flags values
def get_calm_calibration_value():
    global calm_calibration_value
    return calm_calibration_value

def get_anxious_calibration_value():
    global anxious_calibration_value
    return anxious_calibration_value

def get_ability_value():
    global ability_value
    return ability_value

# Handler for PPG green and EDA values
def measurements_handler(address, *args):
    if "PPG:GRN" in address:
        for arg in args:
            ppg_data_queue.put(arg)  # Put the PPG green values into the queue
            
    if "EDA" in address:
        # with open("dummy_ed.txt", 'a') as f:
        #     # Write the address and all arguments to the file
        #     f.write(f"Address: {address}, Values: {args}\n")
        for arg in args:
            eda_data_queue.put(arg)  # Put the EDA values into the queue

def run_osc_listener(ip, port, stop_event):
    disp = dispatcher.Dispatcher()
    disp.set_default_handler(measurements_handler)

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
    """Sends a dummy packet to the OSC listener to unblock it when shutting down."""
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

    # Run the data processor in a separate thread, passing both queues
    data_thread = threading.Thread(target=data_processor, args=(ppg_data_queue, eda_data_queue, stop_event))
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

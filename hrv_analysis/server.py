from pythonosc import dispatcher 
from pythonosc import osc_server
from flask import Flask, jsonify, render_template_string, request, render_template
import threading
import queue
import signal
import sys
import requests
import socket  # For sending dummy data to unblock the listener
from data_processor import data_processor, get_results

# Global queue for sharing data between threads
data_queue = queue.Queue()
file_lock = threading.Lock()

app = Flask(__name__)

# Serve the HTML page at the root route
@app.route('/')
def index():
    return render_template('index.html')

@app.route('/results', methods=['GET'])
def results():
    results = get_results()
    return render_template('results.html', results=results)

# Route for returning the raw JSON data
@app.route('/results_json')
def get_json_results():
    results = get_results()
    return jsonify(results)

# Global variable to store the current ability value
ability_value = 0

@app.route('/ability', methods=['POST'])
def ability():
    global ability_value
    # Check if the request contains JSON data
    if request.is_json:
        data = request.get_json()
        ability_value = data.get('value', 0)  # Update ability_value with the value from JSON, default to 0
    else:
        ability_value = 0  # Default to 0 if no JSON or value is provided
    
    # Return the current ability value as a JSON response
    return jsonify(ability_value=ability_value), 200

@app.route('/ability_value', methods=['GET'])
def ability_value_route():
    """Route to display the current ability value in a table format."""
    return render_template('ability_value.html', ability_value=ability_value)

@app.route('/ability_value_json', methods=['GET'])
def get_current_ability_value():
    """Route to return the current ability value stored in the global variable."""
    return jsonify(ability_value=ability_value), 200

# Function to provide the current ability_value
def get_ability_value():
    global ability_value
    return ability_value

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
        
# Example processing function that uses the ability_value
def process_data_based_on_ability(data):
    global ability_value
    if ability_value == 1:
        # Perform some action if the ability is activated (value is 1)
        print("Processing with the ability activated (value set to 1).")
        # Data processing logic when ability is activated
    else:
        # Perform some action if the ability is not activated (value is 0)
        print("Processing with the ability deactivated (value set to 0).")
        # Data processing logic when ability is deactivated

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

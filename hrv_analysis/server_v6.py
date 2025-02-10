# Three stressed calibration implementation and minor changes

from pythonosc import dispatcher
from pythonosc import osc_server
from flask import Flask, jsonify, request, render_template
import threading
import queue
import signal
import sys
import socket
from calm_calibration import calm_calibration
from stressed_calibration import stressed_calibration
from data_analysis import data_analysis, get_analysis_results

# Flask app
app = Flask(__name__)

# Global variables for states and queues
state = "WAITING"  # States: WAITING, CALM_CALIBRATION, stressed_CALIBRATION, DATA_ANALYSIS
calm_ppg_queue = queue.Queue()  # Queue for Calm Calibration PPG data
calm_eda_queue = queue.Queue()  # Queue for Calm Calibration EDA data
stressed_ppg_queue_1 = queue.Queue()  # Queue for Stressed Calibration 1 PPG data
stressed_eda_queue_1 = queue.Queue()  # Queue for Stressed Calibration 1 EDA data
stressed_ppg_queue_2 = queue.Queue()  # Queue for Stressed Calibration 2 PPG data
stressed_eda_queue_2 = queue.Queue()  # Queue for Stressed Calibration 2 EDA data
stressed_ppg_queue_3 = queue.Queue()  # Queue for Stressed Calibration 3 PPG data
stressed_eda_queue_3 = queue.Queue()  # Queue for Stressed Calibration 3 EDA data
analysis_ppg_queue = queue.Queue()  # Queue for Data Analysis PPG data
analysis_eda_queue = queue.Queue()  # Queue for Data Analysis EDA data
stop_event = threading.Event()

# Global variables for game flags
calm_calibration_value = 0
stressed_calibration_value_1 = 0
stressed_calibration_value_2 = 0
stressed_calibration_value_3 = 0
data_analysis_value = 0
ability_value = 0

# Global booleans to check whether we can change the state or not
calm_calibration_done = False
stressed_calibration_done_1 = False
stressed_calibration_done_2 = False
stressed_calibration_done_3 = False

# Global variables for baselines
calm_baseline_hrv = None
stressed_baseline_hrv_1 = None
stressed_baseline_hrv_2 = None
stressed_baseline_hrv_3 = None

# Global variable for general logging start time
general_start_time = None


@app.route('/')
def index():
    global calm_calibration_value, stressed_calibration_value, data_analysis_value, ability_value
    global calm_calibration_done, stressed_calibration_done_1, stressed_calibration_done_2, stressed_calibration_done_3, state
    global calm_baseline_hrv, stressed_baseline_hrv_1, stressed_baseline_hrv_2, stressed_baseline_hrv_3
    results = get_analysis_results()
    game_flags_values = [calm_calibration_value, stressed_calibration_value_1, stressed_calibration_value_2, stressed_calibration_value_3, data_analysis_value, ability_value]
    calibration_done = [calm_calibration_done, stressed_calibration_done_1, stressed_calibration_done_2, stressed_calibration_done_3]
    baselines = [calm_baseline_hrv, stressed_baseline_hrv_1, stressed_baseline_hrv_2, stressed_baseline_hrv_3]
    return render_template('index.html', results=results, game_flags_values=game_flags_values, calibration_done=calibration_done, baselines=baselines, state=state)

# Route for returning the raw JSON data of the results
@app.route('/analysis_results')
def get_results():
    results = get_analysis_results()
    return jsonify(results)

@app.route('/game_flags', methods=['POST', 'GET'])
def game_flags():
    global calm_calibration_value, stressed_calibration_value_1, stressed_calibration_value_2, stressed_calibration_value_3, data_analysis_value, ability_value
    if request.method == 'POST':
        if request.is_json:
            data = request.get_json()
            calm_calibration_value = data.get('CalmCalib', 0)
            stressed_calibration_value_1 = data.get('StressedCalib1', 0)
            stressed_calibration_value_2 = data.get('StressedCalib2', 0)
            stressed_calibration_value_3 = data.get('StressedCalib3', 0)
            data_analysis_value = data.get('DataAnalysis', 0)
            ability_value = data.get('Breathing', 0)
        else:
            calm_calibration_value = 0
            stressed_calibration_value_1 = 0
            stressed_calibration_value_2 = 0
            stressed_calibration_value_3 = 0
            data_analysis_value = 0
            ability_value = 0
        return jsonify(
            message="Game flags updated",
            calm_calibration_value=calm_calibration_value,
            stressed_calibration_value_1=stressed_calibration_value_1,
            stressed_calibration_value_2=stressed_calibration_value_2,
            stressed_calibration_value_3=stressed_calibration_value_3,
            data_analysis_value=data_analysis_value,
            ability_value=ability_value
        ), 200
    elif request.method == 'GET':
        return jsonify(
            calm_calibration_value=calm_calibration_value,
            stressed_calibration_value_1=stressed_calibration_value_1,
            stressed_calibration_value_2=stressed_calibration_value_2,
            stressed_calibration_value_3=stressed_calibration_value_3,
            data_analysis_value=data_analysis_value,
            ability_value=ability_value
        ), 200


def send_dummy_packet(ip, port):
    """Sends a dummy packet to the OSC listener to unblock it when shutting down."""
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        sock.sendto(b"dummy", (ip, port))  # Send a dummy request to unblock the listener
        sock.close()
        print("Dummy packet sent to unblock OSC listener.")
    except Exception as e:
        print(f"Error sending dummy packet: {e}")
        

# def handle_shutdown(signal, frame, stop_event, ip, port):
#     print("Shutdown signal received. Stopping server...")
#     stop_event.set()
#     # Send a dummy packet to the OSC listener to unblock it
#     #send_dummy_packet(ip, port)
#     sys.exit(0)
    
def handle_shutdown(signal, frame):
    global state
    print("Shutdown signal received. Stopping server...")
    stop_event.set()
    sys.exit(0)


def handle_state_changes():
    """Thread to manage state changes based on game flags."""
    global state
    while not stop_event.is_set():
        if calm_calibration_value == 1 and state == "WAITING":
            state = "CALM_CALIBRATION"
            print("State changed to CALM_CALIBRATION")

        elif stressed_calibration_value_1 == 1 and state == "CALM_CALIBRATION" and calm_calibration_done == True:
            state = "STRESSED_CALIBRATION_1"
            print("State changed to STRESSED_CALIBRATION_1")

        elif stressed_calibration_value_2 == 1 and state == "STRESSED_CALIBRATION_1" and stressed_calibration_done_1 == True:
            state = "STRESSED_CALIBRATION_2"
            print("State changed to STRESSED_CALIBRATION_2")

        elif stressed_calibration_value_3 == 1 and state == "STRESSED_CALIBRATION_2" and stressed_calibration_done_2 == True:
            state = "STRESSED_CALIBRATION_3"
            print("State changed to STRESSED_CALIBRATION_3")

        elif data_analysis_value == 1 and state == "STRESSED_CALIBRATION_3" and stressed_calibration_done_3 == True:
            state = "DATA_ANALYSIS"
            print("State changed to DATA_ANALYSIS")

        threading.Event().wait(0.1)  # Prevent CPU overutilization


def measurements_handler(address, *args):
    global state
    if state == "CALM_CALIBRATION":
        if "PPG:GRN" in address:
            for arg in args:
                calm_ppg_queue.put(arg)
        elif "EDA" in address:
            for arg in args:
                calm_eda_queue.put(arg)

    elif state == "STRESSED_CALIBRATION_1":
        if "PPG:GRN" in address:
            for arg in args:
                stressed_ppg_queue_1.put(arg)
        elif "EDA" in address:
            for arg in args:
                stressed_eda_queue_1.put(arg)

    elif state == "STRESSED_CALIBRATION_2":
        if "PPG:GRN" in address:
            for arg in args:
                stressed_ppg_queue_2.put(arg)
        elif "EDA" in address:
            for arg in args:
                stressed_eda_queue_2.put(arg)

    elif state == "STRESSED_CALIBRATION_3":
        if "PPG:GRN" in address:
            for arg in args:
                stressed_ppg_queue_3.put(arg)
        elif "EDA" in address:
            for arg in args:
                stressed_eda_queue_3.put(arg)

    elif state == "DATA_ANALYSIS":
        if "PPG:GRN" in address:
            for arg in args:
                analysis_ppg_queue.put(arg)
        elif "EDA" in address:
            for arg in args:
                analysis_eda_queue.put(arg)


def run_osc_listener(ip, port):
    disp = dispatcher.Dispatcher()
    disp.set_default_handler(measurements_handler)
    server = osc_server.BlockingOSCUDPServer((ip, port), disp)
    print(f"Listening on {ip}:{port}")
    try:
        while not stop_event.is_set():
            server.handle_request()
    finally:
        server.server_close()


def run_calm_calibration():
    global calm_baseline_hrv, calm_calibration_value, general_start_time, calm_calibration_done
    while not stop_event.is_set():
        if state == "CALM_CALIBRATION":
            calm_baseline_hrv, general_start_time = calm_calibration(calm_ppg_queue, calm_eda_queue, stop_event)
            calm_calibration_value = 0
            calm_calibration_done = True
            if calm_baseline_hrv == 0:
                print("Error during calm calibration. Press Ctrl+C to shut down the server.")
                break
            print(f"Calm calibration completed. Calm Baseline: {calm_baseline_hrv}")
            break

        threading.Event().wait(0.1)


def run_stressed_calibration_1():
    global stressed_baseline_hrv_1, stressed_calibration_value_1, general_start_time, stressed_calibration_done_1
    while not stop_event.is_set():
        if state == "STRESSED_CALIBRATION_1":
            if general_start_time == None:
                print("Error fetching general logging starting time. Press Ctrl+C to shut down the server.")
                break
            stressed_baseline_hrv_1 = stressed_calibration(stressed_ppg_queue_1, stressed_eda_queue_1, stop_event, general_start_time, 1)
            stressed_calibration_value_1 = 0
            stressed_calibration_done_1 = True
            if stressed_baseline_hrv_1 == 0:
                print("Error during stressed calibration 1. Press Ctrl+C to shut down the server.")
                break
            print(f"Stressed calibration 1 completed. Stressed Baseline 1: {stressed_baseline_hrv_1}")
            break
        threading.Event().wait(0.1)

def run_stressed_calibration_2():
    global stressed_baseline_hrv_2, stressed_calibration_value_2, general_start_time, stressed_calibration_done_2
    while not stop_event.is_set():
        if state == "STRESSED_CALIBRATION_2":
            if general_start_time == None:
                print("Error fetching general logging starting time. Press Ctrl+C to shut down the server.")
                break
            stressed_baseline_hrv_2 = stressed_calibration(stressed_ppg_queue_2, stressed_eda_queue_2, stop_event, general_start_time, 2)
            stressed_calibration_value_2 = 0
            stressed_calibration_done_2 = True
            if stressed_baseline_hrv_2 == 0:
                print("Error during stressed calibration. Press Ctrl+C to shut down the server.")
                break
            print(f"Stressed calibration 2 completed. Stressed Baseline 2: {stressed_baseline_hrv_2}")
            break
        threading.Event().wait(0.1)

def run_stressed_calibration_3():
    global stressed_baseline_hrv_3, stressed_calibration_value_3, general_start_time, stressed_calibration_done_3
    while not stop_event.is_set():
        if state == "STRESSED_CALIBRATION_3":
            if general_start_time == None:
                print("Error fetching general logging starting time. Press Ctrl+C to shut down the server.")
                break
            stressed_baseline_hrv_3 = stressed_calibration(stressed_ppg_queue_3, stressed_eda_queue_3, stop_event, general_start_time, 3)
            stressed_calibration_value_3 = 0
            stressed_calibration_done_3 = True
            if stressed_baseline_hrv_3 == 0:
                print("Error during stressed calibration 3. Press Ctrl+C to shut down the server.")
                break
            print(f"Stressed calibration completed. Stressed Baseline 3: {stressed_baseline_hrv_3}")
            break
        threading.Event().wait(0.1)


def run_data_analysis():
    global general_start_time, calm_baseline_hrv, stressed_baseline_hrv_1, stressed_baseline_hrv_2, stressed_baseline_hrv_3
    while not stop_event.is_set():
        if state == "DATA_ANALYSIS":
            if general_start_time == None or calm_baseline_hrv == None or stressed_baseline_hrv_1 == None or stressed_baseline_hrv_2 == None or stressed_baseline_hrv_3 == None:
                print("Error fetching general logging starting time or baselines. Press Ctrl+C to shut down the server.")
                break
            print("Data analysis running...")
            data_analysis(analysis_ppg_queue, analysis_eda_queue, stop_event, general_start_time, calm_baseline_hrv) # Later pass stressed baselines as arguments
        threading.Event().wait(0.1)
    

def main():
    ip = '127.0.0.1'
    port = 12345
    stop_event = threading.Event()

    signal.signal(signal.SIGINT, handle_shutdown)
    
    # Handle Ctrl+C signal to stop the server
    #signal.signal(signal.SIGINT, lambda signal, frame: handle_shutdown(signal, frame, stop_event, ip, port))

    # Threads for OSC Listener, State Management, and Processing
    osc_thread = threading.Thread(target=run_osc_listener, args=(ip, port))
    state_thread = threading.Thread(target=handle_state_changes)
    calm_thread = threading.Thread(target=run_calm_calibration)
    stressed_thread_1 = threading.Thread(target=run_stressed_calibration_1)
    stressed_thread_2 = threading.Thread(target=run_stressed_calibration_2)
    stressed_thread_3 = threading.Thread(target=run_stressed_calibration_3)
    analysis_thread = threading.Thread(target=run_data_analysis)

    osc_thread.start()
    state_thread.start()
    calm_thread.start()
    stressed_thread_1.start()
    stressed_thread_2.start()
    stressed_thread_3.start()
    analysis_thread.start()

    # Run Flask in the main thread
    print("Starting Flask server...")
    app.run(host='0.0.0.0', port=5000, use_reloader=False)

    osc_thread.join()
    state_thread.join()
    calm_thread.join()
    stressed_thread_1.join()
    stressed_thread_2.join()
    stressed_thread_3.join()
    analysis_thread.join()


if __name__ == "__main__":
    main()

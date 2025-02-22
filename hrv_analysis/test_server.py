from pythonosc import dispatcher
from pythonosc import osc_server
from flask import Flask, jsonify, request, render_template
import threading
import queue
import signal
import sys
import socket
from functions.test_values import test_values
from functions.calm_calibration import calm_calibration
from functions.stressed_calibration import stressed_calibration
from functions.data_analysis import data_analysis, get_analysis_results
from functions.postprocess_eda import split_raw_eda
import time

# Flask app
app = Flask(__name__)

# Global variables for states and queues
state = "WAITING"  # States: WAITING, TEST, CALM_CALIBRATION, STRESSED_CALIBRATION, DATA_ANALYSIS
test_ppg_queue = queue.Queue()  # Queue for Test PPG Data
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
testing_value = 0
calm_calibration_value = 0
stressed_calibration_value_1 = 0
stressed_calibration_value_2 = 0
stressed_calibration_value_3 = 0
data_analysis_value = 0
ability_value = 0

# Global booleans to check whether we can change the state or not
testing_done = False
calm_calibration_done = False
stressed_calibration_done_1 = False
stressed_calibration_done_2 = False
stressed_calibration_done_3 = False

# Global variables for baselines
calm_baseline_hrv = None
stressed_baseline_hrv_1 = None
stressed_baseline_hrv_2 = None
stressed_baseline_hrv_3 = None
threshold = None

# Global variable for general logging start time
general_start_time = None


@app.route('/')
def index():
    global calm_calibration_value, stressed_calibration_value_1, stressed_calibration_value_2, stressed_calibration_value_3, data_analysis_value, ability_value
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
    global testing_value, calm_calibration_value, stressed_calibration_value_1, stressed_calibration_value_2, stressed_calibration_value_3, data_analysis_value, ability_value
    if request.method == 'POST':
        if request.is_json:
            data = request.get_json()
            testing_value = data.get('Testing', 0)
            calm_calibration_value = data.get('CalmCalib', 0)
            stressed_calibration_value_1 = data.get('StressedCalib1', 0)
            stressed_calibration_value_2 = data.get('StressedCalib2', 0)
            stressed_calibration_value_3 = data.get('StressedCalib3', 0)
            data_analysis_value = data.get('DataAnalysis', 0)
            ability_value = data.get('Breathing', 0)
        else:
            testing_value = 0
            calm_calibration_value = 0
            stressed_calibration_value_1 = 0
            stressed_calibration_value_2 = 0
            stressed_calibration_value_3 = 0
            data_analysis_value = 0
            ability_value = 0
        return jsonify(
            message="Game flags updated",
            testing_value=testing_value,
            calm_calibration_value=calm_calibration_value,
            stressed_calibration_value_1=stressed_calibration_value_1,
            stressed_calibration_value_2=stressed_calibration_value_2,
            stressed_calibration_value_3=stressed_calibration_value_3,
            data_analysis_value=data_analysis_value,
            ability_value=ability_value
        ), 200
    elif request.method == 'GET':
        return jsonify(
            testing_value=testing_value,
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


def handle_state_changes():
    """Thread to manage state changes based on game flags."""
    global state
    while not stop_event.is_set():
        # Wait in WAITING state until we receive the DataAnalysis flag
        if data_analysis_value == 1 and state == "WAITING":
            state = "DATA_ANALYSIS"
            print("State changed to DATA_ANALYSIS")
        threading.Event().wait(0.1)  # Prevent CPU overutilization


def measurements_handler(address, *args):
    global state
    # In test mode, direct all OSC data to analysis queues.
    if state == "DATA_ANALYSIS":
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


def run_data_analysis():
    global general_start_time, threshold
    while not stop_event.is_set():
        if state == "DATA_ANALYSIS":
            # Set dummy calibration values
            general_start_time = time.time()
            threshold = 300
            print("Decision Threshold: ", threshold)
            print("Data analysis running...")
            data_analysis(analysis_ppg_queue, analysis_eda_queue, stop_event, general_start_time, threshold)
            # data_analysis(analysis_ppg_queue, analysis_eda_queue, stop_event, general_start_time, calm_baseline_hrv)
        threading.Event().wait(0.1)

def handle_shutdown(signal, frame):
    global state
    stop_event.set()
    # Postprocess raw EDA values from data analysis before shuting down
    if state == "DATA_ANALYSIS":
        split_raw_eda()
        print("Data Analysis EDA values postprocessed.")
    
    print("Shutdown signal received. Stopping server...")
    sys.exit(0)
    
    
def main():
    ip = '127.0.0.1'
    port = 12345
    stop_event = threading.Event()

    signal.signal(signal.SIGINT, handle_shutdown)
    
    # Handle Ctrl+C signal to stop the server
    #signal.signal(signal.SIGINT, lambda signal, frame: handle_shutdown(signal, frame, stop_event, ip, port))

    # In test mode, we only use the OSC listener, state management, and data analysis threads.
    osc_thread = threading.Thread(target=run_osc_listener, args=(ip, port))
    state_thread = threading.Thread(target=handle_state_changes)
    analysis_thread = threading.Thread(target=run_data_analysis)

    osc_thread.start()
    state_thread.start()
    analysis_thread.start()

    # Run Flask in the main thread
    print("Starting Flask server...")
    app.run(host='0.0.0.0', port=5000, use_reloader=False)

    osc_thread.join()
    state_thread.join()
    analysis_thread.join()


if __name__ == "__main__":
    main()

import warnings
from neurokit2 import NeuroKitWarning
warnings.filterwarnings('ignore', category=NeuroKitWarning)

import neurokit2 as nk
import numpy as np
import queue
from peaks_hrv_functions import detect_peaks, calculate_rmssd
#from server import get_ability_value
from collections import deque
import threading
import requests
import time

output_value_lock = threading.Lock()
results = {}  # This will store all the results

log_file = "data_processor_log.txt"  # The log file name

# Function to add log entries to both files (data_processor_log.txt and ability_log.txt)
def add_log_entry(entry, ability_measurement=False):
    # Write log entry to the main file (data_processor_log.txt)
    with open(log_file, 'a') as f:
        f.write(entry)

    # If this is an ability measurement, also log it to ability_log.txt
    if ability_measurement:
        with open("ability_log.txt", 'a') as f:
            f.write(entry)

# Function to set all the output values in a dictionary
def set_results(binary_output, categorical_output, current_hrv):
    with output_value_lock:
        results['current_hrv'] = current_hrv
        results['binary_output'] = binary_output
        results['previous_hrv'] = categorical_output


# Function to get all the results
def get_results():
    with output_value_lock:
        return results

def fetch_ability_value():
    """
    This function retrieves the ability value from the server running at localhost:5000.
    It expects a response in the form of {ability_value: value}.
    """
    try:
        # Send a GET request to the ability_value_json route
        response = requests.get("http://localhost:5000/ability_value_json")
        response.raise_for_status()  # Raise an error if the request failed

        # Parse the JSON response
        data = response.json()
        return data.get("ability_value", 0)  # Default to 0 if ability_value is not found
    except requests.exceptions.RequestException as e:
        print(f"Error fetching ability value: {e}")
        return 0  # Return 0 if there's an error
    
def data_processor(data_queue, stop_event):
    sampling_rate = 100
    window_size = int(30 * sampling_rate)
    step_size = int(5 * sampling_rate)
    step_counter = 0
    num_steps_for_baseline = 10
    num_steps_skipped = 10
    current_step = 0
    buffer = deque(maxlen=window_size)
    baseline_hrv = []
    previous_hrv_values = deque(maxlen=3)
    in_analysis = False  # Flag to indicate if ability analysis is currently running
    ability_detected = False  # To track if the ability was activated
    ability_measurement_step = None  # Tracks when ability measurements should start
    ability_logging_active = False  # Tracks whether ability logging should be done
    
    # Start the time counter when the function is called
    start_time = time.time()
    
    # Clear the log file at the start
    with open(log_file, 'w') as f:
        f.write("Starting data processor logging...\n")
        
    # Create or clear the ability_log.txt file at the start
    with open("ability_log.txt", 'w') as f:
        f.write("Starting ability logging...\n")
    
    while not stop_event.is_set() or not data_queue.empty():
        
        try:
            data_point = data_queue.get(timeout=0.01)
            buffer.append(data_point)  # Add data to the buffer

            # Increment step counter and check if it's time to analyze the data
            step_counter += 1
            if step_counter >= step_size:
                if len(buffer) >= window_size:
                    # Process the PPG signal in the buffer
                    peaks, _ = detect_peaks(buffer, lowcut=0.5, highcut=1.5, fs=100, order=3, peak_distance=20)
                    rr_intervals = np.diff(peaks) / sampling_rate
                    current_hrv = calculate_rmssd(rr_intervals)

                    current_step += 1  
                    
                    # Calculate the actual timestamp by getting the elapsed time since the function was called
                    elapsed_time = time.time() - start_time
                    actual_timestamp_minutes = int(elapsed_time // 60)  # Convert to minutes
                    actual_timestamp_seconds = int(elapsed_time % 60)  # Get remaining seconds

                    # Calculate the expected timestamp based on the current step and expected intervals
                    expected_timestamp = (current_step + 6) * 5  # Assuming each step is 5 seconds - +6 because of initial window
                    expected_timestamp_minutes = expected_timestamp // 60
                    expected_timestamp_seconds = expected_timestamp % 60

                    if current_step <= num_steps_skipped:
                        log_msg = (f"Segment {current_step} skipped - "
                        f"Expected Timestamp {expected_timestamp_minutes}min {expected_timestamp_seconds}sec, "
                        f"Actual Timestamp {actual_timestamp_minutes}min {actual_timestamp_seconds}sec\n")
                        add_log_entry(log_msg)
                        
                        log_msg = f"Current HRV: {current_hrv}\n\n"
                        add_log_entry(log_msg)
                        
                        if current_step == num_steps_skipped:
                            log_msg = "\n"
                            add_log_entry(log_msg)
                    
                    elif current_step <= num_steps_skipped + num_steps_for_baseline:
                        baseline_hrv.append(current_hrv)

                        log_msg = (f"Segment {current_step} - "
                        f"Expected Timestamp {expected_timestamp_minutes}min {expected_timestamp_seconds}sec, "
                        f"Actual Timestamp {actual_timestamp_minutes}min {actual_timestamp_seconds}sec\n")
                        add_log_entry(log_msg)
                        
                        log_msg = f"Current HRV: {current_hrv}\n\n"
                        add_log_entry(log_msg)
                        
                        if current_step == num_steps_skipped + num_steps_for_baseline:
                            # Calculate average baseline HRV after collecting enough data
                            baseline_hrv = np.mean(baseline_hrv)
                            add_log_entry(f"Baseline HRV established: {baseline_hrv}\n\n\n")
                            
                    else:
                        # Fetch the ability_value from localhost at /ability_value_json
                        ability_value = fetch_ability_value()

                        # Detect when ability_value turns to 1 and start tracking the ability measurement
                        if ability_value == 1 and not in_analysis and current_step > num_steps_for_baseline + num_steps_skipped:
                            in_analysis = True
                            ability_detected = True
                            ability_measurement_step = current_step + 5  # Start measurement after 5 steps
                            log_msg = (f"\nAbility activated in step {current_step}. \nExpect ability measurements in "
                                    f"segments {ability_measurement_step}, {ability_measurement_step + 1}, and {ability_measurement_step + 2}.\n\n")
                            add_log_entry(log_msg, ability_measurement=True)
                            
                        # Start logging only for steps 6, 7, and 8 after detection
                        if ability_detected and current_step == ability_measurement_step:
                            ability_logging_active = True  # Activate ability logging for this and the next two steps
                        
                        # Reset the in_analysis flag when ability_value returns to 0
                        elif ability_value == 0 and in_analysis:
                            in_analysis = False
                            ability_detected = False  # Reset detection flag
            
                        # Compare to baseline and previous HRV
                        change_from_baseline = (current_hrv - baseline_hrv) / baseline_hrv * 100

                        if len(previous_hrv_values) > 0:
                            change_from_previous = (current_hrv - previous_hrv_values[-1]) / previous_hrv_values[-1] * 100
                            previous_hrv_for_change = previous_hrv_values[-1]
                        else:
                            change_from_previous = 0
                            previous_hrv_for_change = None

                        previous_hrv_values.append(current_hrv)

                        if len(previous_hrv_values) == 3:
                            previous_hrv_array = np.array(previous_hrv_values)
                            mean_last_three_hrv = np.mean(previous_hrv_array)
                            excluded_value = None

                            # Detect and exclude outliers from the mean calculation if they differ more than 20%
                            diffs = np.abs(np.diff(previous_hrv_array) / previous_hrv_array[:-1])
                            if np.any(diffs > 0.2):
                                excluded_indices = np.where(diffs > 0.2)[0]
                                if len(excluded_indices) > 0:
                                    excluded_index = excluded_indices[0]
                                    excluded_value = previous_hrv_array[excluded_index + 1]
                                    mean_last_three_hrv = np.mean([value for value, diff in zip(previous_hrv_array, diffs) if diff <= 0.3])

                            change_from_mean_last_three = (current_hrv - mean_last_three_hrv) / mean_last_three_hrv * 100
                        else:
                            change_from_mean_last_three = 0
                            excluded_value = None

                        log_msg = (f"Segment {current_step} - "
                        f"Expected Timestamp {expected_timestamp_minutes}min {expected_timestamp_seconds}sec, "
                        f"Actual Timestamp {actual_timestamp_minutes}min {actual_timestamp_seconds}sec\n")
                        add_log_entry(log_msg, ability_measurement=ability_logging_active)
                        
                        # Handle ability measurements during steps 6, 7, 8 after detection
                        if ability_detected and current_step in [ability_measurement_step, ability_measurement_step + 1, ability_measurement_step + 2]:
                            label_number = current_step - ability_measurement_step + 1
                            log_msg = f"ABILITY MEASUREMENT {label_number} for segment {current_step}.\n"
                            add_log_entry(log_msg, ability_measurement=True)  # Log this to both files
                            
                        log_msg = f"Current HRV: {current_hrv}\n"
                        add_log_entry(log_msg, ability_measurement=ability_logging_active)

                        log_msg = f"Change from Baseline: {change_from_baseline:.2f}%\n"
                        add_log_entry(log_msg, ability_measurement=ability_logging_active)

                        log_msg = f"Change from Previous: {change_from_previous:.2f}%\n"
                        add_log_entry(log_msg, ability_measurement=ability_logging_active)
                        
                        if previous_hrv_for_change is not None:
                            log_msg = f"Previous HRV for Change: {previous_hrv_for_change}\n"
                            add_log_entry(log_msg, ability_measurement=ability_logging_active)
                            
                        log_msg = f"Previous Three HRV Values: {list(previous_hrv_values)}\n"
                        add_log_entry(log_msg, ability_measurement=ability_logging_active)
                        log_msg = f"Change from Mean of Last Three: {change_from_mean_last_three:.2f}%\n"
                        add_log_entry(log_msg, ability_measurement=ability_logging_active)

                        if excluded_value is not None:
                            log_msg = f"Excluded Value from Mean Calculation: {excluded_value}\n"
                            add_log_entry(log_msg, ability_measurement=ability_logging_active)
                        else:
                            log_msg = f"No Value Excluded from Mean Calculation\n"
                            add_log_entry(log_msg, ability_measurement=ability_logging_active)
                        
                        # Binary output (0: calm, 1: anxious)
                        binary_output = 0
                        total_change_from_previous = 0.8 * change_from_mean_last_three + 0.2 * change_from_previous
                        if change_from_baseline <= -5:
                            binary_output = 1
                        elif change_from_baseline <= 5 and total_change_from_previous <= -5:
                            binary_output = 1

                        # Categorical output (0: very calm, 1: calm, 2: anxious, 3: very anxious)
                        categorical_output = 0  # default to very calm

                        # Determine the value for the new categorical output based on the change from baseline
                        if change_from_baseline <= -10:
                            categorical_output = 3  # very anxious
                        if -10 < change_from_baseline <= -5:
                            categorical_output = 2  # anxious
                        elif -5 < change_from_baseline < 0:
                            categorical_output = 1  # calm

                        # Further refine based on total change from previous
                        elif change_from_baseline <= 5 and total_change_from_previous <= -7.5:
                            categorical_output = 3  # very anxious
                        elif change_from_baseline <= 5 and total_change_from_previous <= -5:
                            categorical_output = 2  # anxious
                        elif change_from_baseline <= 5 and total_change_from_previous <= -2.5:
                            categorical_output = 1  # calm

                        # Log both the binary and categorical output
                        add_log_entry(f"Binary Output (0: calm, 1: anxious) = {binary_output}\n", ability_measurement=ability_logging_active)
                        add_log_entry(f"Categorical Output (0: very calm, 1: calm, 2: anxious, 3: very anxious) = {categorical_output}\n\n", ability_measurement=ability_logging_active)
                        
                        if ability_detected and current_step == (ability_measurement_step + 2):
                            ability_logging_active = False
                            
                        # Set the full result with all required values
                        set_results(
                            current_hrv=current_hrv,
                            binary_output=binary_output,
                            categorical_output=categorical_output
                        )

                step_counter = 0  # Reset step counter after processing

        except queue.Empty:
            if stop_event.is_set():
                print("Stopping data processor.")
                break  # Exit the loop if stop_event is set


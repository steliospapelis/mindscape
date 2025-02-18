import numpy as np
import queue
from functions.hrv_calculation_functions import detect_peaks, calculate_rmssd
from collections import deque
import threading
import requests
import time
import csv
import json
import os

output_value_lock = threading.Lock()

results = {}    # This will store all the analysis results

general_log_file = "./logs/general_log.txt"
current_log_file = "./logs/data_analysis_log.txt"
ability_log_file = "./logs/ability_log.txt"
ppg_csv_file = "./measurements/analysis_ppg_values.csv"
eda_csv_file = "./measurements/analysis_eda_values.csv"
analysis_json_file = "./hrv_values/analysis_values.json"

# Write headers only once at the beginning
with open(ppg_csv_file, 'w', newline='') as csvfile:
    writer = csv.writer(csvfile)
    writer.writerow(["Segment", "PPG Values"])

with open(eda_csv_file, 'w', newline='') as csvfile:
    writer = csv.writer(csvfile)
    writer.writerow(["Segment", "EDA Values"])


# Function to add log entries to current_log_file and general_log_file
def add_log_entry(entry, only_general_log=False, ability_log=False):
        
    with open(general_log_file, 'a') as f:
        f.write(entry)
        
    if not only_general_log:
        with open(current_log_file, 'a') as f:
            f.write(entry)
            
    # If this is an ability measurement, also log it to ability_log.txt
    if ability_log:
        with open(ability_log_file, 'a') as f:
            f.write(entry)


def fetch_ability_value(stop_flag):
    """
    This function retrieves the ability value from the server running at localhost:5000.
    It expects a response in the form of {ability_value: value}.
    """
    try:
        if not stop_flag.is_set():
            # Send a GET request to the ability_value_json route
            response = requests.get("http://localhost:5000/game_flags")
            response.raise_for_status()  # Raise an error if the request failed

            # Parse the JSON response
            data = response.json()
            return data.get("ability_value", 0)  # Default to 0 if ability_value is not found
        # else:
        #     add_log_entry("\n\nServer was stopped. The data below are invalid.\n\n")
        #     return 1
        
    except requests.exceptions.RequestException as e:
        if not stop_flag.is_set(): 
            print(f"Error fetching ability value: {e}")
            return 0  # Return 0 if there's an error
        # else:
        #     add_log_entry("\n\nServer was stopped. The data below are invalid.\n\n")
        #     return 1


# Function to set all the output values in a dictionary
def set_analysis_results(binary_output, categorical_output, current_hrv):
    with output_value_lock:
        results['current_hrv'] = current_hrv
        results['binary_output'] = binary_output
        # results['categorical_output'] = categorical_output


# Function to get all the results
def get_analysis_results():
    with output_value_lock:
        return results


def data_analysis(ppg_data_queue, eda_data_queue, stop_event, general_start_time, calm_baseline_hrv, stressed_baseline, std_dev): 
    sampling_rate = 100
    window_size = int(30 * sampling_rate)
    step_size = int(5 * sampling_rate)
    step_counter = 0
    current_step = 0
    buffer = deque(maxlen=window_size)
    
    previous_hrv_values = deque(maxlen=3)

    in_analysis = False  # Flag to indicate if ability analysis is currently running
    ability_detected = False  # To track if the ability was activated
    ability_measurement_step = None  # Tracks when ability measurements should start
    ability_logging_active = False  # Tracks whether ability logging should be done
    ability_measurement = False # Tracks the logged ability measurements for the json logging
    
    # Start the time counter when the function is called
    start_time = time.time()
    
    # Clear the current_log_file at the start
    with open(current_log_file, 'w') as f:
        f.write("Starting data analysis logging...\n")
        f.write("Waiting for the first window to fill (about 30 seconds)\n\n")
        
    # Create or clear the general_log_file at the start
    with open(general_log_file, 'a') as f:
        f.write("Starting data analysis logging...\n")
        f.write("Waiting for the first window to fill (about 30 seconds)\n\n")
        
    # Create or clear the ability_log_file at the start
    with open(ability_log_file, 'w') as f:
        f.write("Starting ability logging...\n\n")
        
    # Clear the analysis JSON file at the start (NDJSON logging)
    with open(analysis_json_file, 'w') as f:
        f.write("")
        
    try:
        while not stop_event.is_set() or not ppg_data_queue.empty():   
            
            try:
                data_point = ppg_data_queue.get(timeout=0.01)
                buffer.append(data_point)  # Add data to the buffer

                # Increment step counter and check if it's time to analyze the data
                step_counter += 1
                if step_counter >= step_size:
                    if len(buffer) >= window_size:
                        
                        current_step += 1

                        # Process the PPG signal in the buffer
                        peaks, _ = detect_peaks(buffer, lowcut=0.5, highcut=1.5, fs=100, order=3, peak_distance=20)
                        rr_intervals = np.diff(peaks) / sampling_rate
                        current_hrv = calculate_rmssd(rr_intervals)

                        
                        # Calculate the timestamp by getting the elapsed time since the function was called
                        elapsed_time = time.time() - start_time
                        timestamp_minutes = int(elapsed_time // 60)  # Convert to minutes
                        timestamp_seconds = int(elapsed_time % 60)  # Get remaining seconds
                        
                        # Calculate the timestamp from when the calm calibration started
                        general_elapsed_time = time.time() - general_start_time
                        general_timestamp_minutes = int(general_elapsed_time // 60)  # Convert to minutes
                        general_timestamp_seconds = int(general_elapsed_time % 60)  # Get remaining seconds
                        
                        # Append the segment data to the CSV file
                        with open(ppg_csv_file, 'a', newline='') as csvfile:
                            writer = csv.writer(csvfile)
                            writer.writerow([current_step, list(buffer)])  # Segment number and buffer values
                            
                        # Empty the eda_data_queue into a list
                        # Since sampling rate is 15hz then we should have 75 measurements in a 5 second step
                        # We save only the new 5 seconds of data for each segment
                        eda_values = []
                        while not eda_data_queue.empty() and len(eda_values) < 75:
                            eda_values.append(eda_data_queue.get())

                        # Write the segment number and EDA values to the EDA CSV file
                        with open(eda_csv_file, 'a', newline='') as csvfile:
                            writer = csv.writer(csvfile)
                            writer.writerow([current_step, eda_values])  # Segment number and EDA values 
                            
                        # Fetch the ability_value from localhost
                        ability_value = fetch_ability_value(stop_event)

                        # Detect when ability_value turns to 1 and start tracking the ability measurement
                        if ability_value == 1 and not in_analysis: # and current_step > num_steps_for_baseline + num_steps_skipped:
                            in_analysis = True
                            ability_detected = True
                            ability_measurement_step = current_step + 5  # Start measurement after 5 steps
                            log_msg = (f"\nAbility activated in segment {current_step}. \nExpect ability measurements in "
                                    f"segments {ability_measurement_step}, {ability_measurement_step + 1}, and {ability_measurement_step + 2}.\n\n")
                            add_log_entry(log_msg, ability_log=True)
                            
                        # Start logging only for steps 6, 7, and 8 after detection
                        if ability_detected and current_step in [ability_measurement_step, ability_measurement_step + 1, ability_measurement_step + 2]:
                            ability_logging_active = True  # Activate ability logging for these segments
                        else:
                            ability_logging_active = False
                        
                        # Reset the in_analysis flag when ability_value returns to 0
                        if ability_value == 0 and in_analysis:
                            in_analysis = False
                            ability_detected = False  # Reset detection flag
            
                        # Simplified rule-based logic:
                        # Define the threshold as the stressed baseline plus the standard deviation.
                        threshold = stressed_baseline + std_dev

                        # If the current HRV is below the threshold, mark as stressed (1); otherwise, calm (0).
                        if current_hrv < threshold:
                            binary_output = 1  # stressed
                        else:
                            binary_output = 0  # calm
                            
                        # Compare to threshold
                        change_from_threshold = (current_hrv - threshold) / threshold * 100

                        log_msg = (f"Segment {current_step} - "
                            f"General Timestamp {general_timestamp_minutes}min {general_timestamp_seconds}sec\n"
                            f"(Data Timestamp {timestamp_minutes}min {timestamp_seconds}sec)\n")
                        add_log_entry(log_msg, ability_log=ability_logging_active)
                        
                        # Handle ability measurements during steps 6, 7, 8 after detection
                        if ability_detected and current_step in [ability_measurement_step, ability_measurement_step + 1, ability_measurement_step + 2]:
                            label_number = current_step - ability_measurement_step + 1
                            ability_measurement = True
                            log_msg = f"ABILITY MEASUREMENT {label_number} for segment {current_step}.\n"
                            add_log_entry(log_msg, ability_log=True)  # Log this to both files
                            
                        log_msg = f"Current HRV: {current_hrv}\n"
                        add_log_entry(log_msg, ability_log=ability_logging_active)

                        log_msg = f"Change from Threshold: {change_from_threshold:.2f}%\n"
                        add_log_entry(log_msg, ability_log=ability_logging_active)

                        # Log the binary output
                        add_log_entry(f"Binary Output (0: calm, 1: stressed) = {binary_output}\n", ability_log=ability_logging_active)
                            
                        # Set the full result with all required values
                        set_analysis_results(
                            current_hrv=current_hrv,
                            binary_output=binary_output,
                            # categorical_output=0
                        )
                        
                        # Create a JSON log entry for this analysis segment, including output and ability measurement flag
                        analysis_entry = {
                            "segment": current_step,
                            "timestamp_min": timestamp_minutes,
                            "timestamp_sec": timestamp_seconds,
                            "HRV": current_hrv,
                            "binary_output": binary_output,
                            "ability_measurement": ability_measurement
                        }
                        # Immediately append the JSON entry to file in NDJSON format
                        with open(analysis_json_file, "a") as json_file:
                            json_file.write(json.dumps(analysis_entry) + "\n")
                            
                        if ability_detected and current_step == (ability_measurement_step + 2):
                            ability_logging_active = False
                            ability_measurement = False
  
                    step_counter = 0  # Reset step counter after processing

            except queue.Empty:
                if stop_event.is_set():
                    print("Stopping data processor.")
                    break  # Exit the loop if stop_event is set
                
    except Exception as e:
        error_msg = f"An error occurred during data analysis: {e}\n"
        add_log_entry(error_msg)
        return 0  # Return 0 in case of an error
         
    return 0

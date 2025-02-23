import numpy as np
import queue
from functions.hrv_calculation_functions import detect_peaks, calculate_rmssd
from collections import deque
import threading
import requests
import time
import json
import os

output_value_lock = threading.Lock()

results = {}    # This will store all the analysis results


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
    except requests.exceptions.RequestException as e:
        if not stop_flag.is_set(): 
            print(f"Error fetching ability value: {e}")
            return 0  # Return 0 if there's an error


# Function to set all the output values in a dictionary
def set_analysis_results(binary_output, current_hrv, best_ability_index, best_ability_hrv):   
    with output_value_lock:
        results['current_hrv'] = current_hrv
        results['binary_output'] = binary_output
        results['best_ability_index'] = best_ability_index
        results['best_ability_hrv'] = best_ability_hrv
        


# Function to get all the results
def get_analysis_results():
    with output_value_lock:
        return results


def data_analysis(ppg_data_queue, eda_data_queue, stop_event, general_start_time, threshold): 
    
    general_log_file = "./logs/general_log.txt"
    current_log_file = "./logs/specific_logs/data_analysis_log.txt"
    ability_log_file = "./logs/specific_logs/ability_log.txt"
    ppg_json_file = "./measurements/analysis/analysis_ppg_values.json"
    eda_json_file = "./measurements/analysis/analysis_eda_values.json"
    raw_eda_json_file = "./measurements/analysis/analysis_raw_eda_values.json"
    analysis_json_file = "./hrv_values/analysis_values.json"
    
    # Ensure the directories exist
    os.makedirs(os.path.dirname(general_log_file), exist_ok=True)
    os.makedirs(os.path.dirname(ppg_json_file), exist_ok=True)
    os.makedirs(os.path.dirname(eda_json_file), exist_ok=True)
    os.makedirs(os.path.dirname(analysis_json_file), exist_ok=True)
    os.makedirs(os.path.dirname(raw_eda_json_file), exist_ok=True)
    
    
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


    sampling_rate = 100
    window_size = int(30 * sampling_rate)
    step_size = int(5 * sampling_rate)
    step_counter = 0
    current_step = 0
    buffer = deque(maxlen=window_size)


    in_analysis = False  # Flag to indicate if ability analysis is currently running
    ability_detected = False  # To track if the ability was activated
    ability_measurement_step = None  # Tracks when ability measurements should start
    ability_logging_active = False  # Tracks whether ability logging should be done
    ability_measurement = False  # Tracks if the ability measurement is logged for this segment
    
    # Variables for finding the best breathing rate
    ability_index = 0
    best_ability_index = 0
    best_ability_hrv = 0
    ability_hrv_values = []
    
    # For EDA, assume sampling rate of 15Hz; define window and step sizes accordingly
    eda_sampling_rate = 15
    window_size_eda = int(30 * eda_sampling_rate)  # 450 samples (30 sec)
    step_size_eda = int(5 * eda_sampling_rate)       # 75 samples (5 sec)
    
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
        
    # Clear the analysis JSON file at the start by writing an empty JSON object with an empty array
    with open(analysis_json_file, 'w') as f:
        json.dump({"analysis_values": []}, f, indent=4)
    
    # Write headers only once at the beginning for JSON files (initialize with an empty array)
    with open(ppg_json_file, 'w') as f:
        json.dump({"segments": []}, f, indent=4)
    with open(eda_json_file, 'w') as f:
        json.dump({"segments": []}, f, indent=4)
    # Initialize the raw EDA file with an empty object
    with open(raw_eda_json_file, 'w') as f:
        json.dump({"raw_eda": []}, f, indent=4)
        
        
    try:
        while not stop_event.is_set() or not ppg_data_queue.empty():   
            try:
                data_point = ppg_data_queue.get(timeout=0.01)
                buffer.append(data_point) 

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
                        
                        # PPG JSON logging: For current_step==1, store two segments:
                        #   segment 0: first (window_size - step_size) samples (i.e. initial 25 seconds)
                        #   segment 1: last step_size samples (i.e. last 5 seconds)
                        if current_step == 1:
                            first_ppg = list(buffer)[:window_size - step_size]
                            second_ppg = list(buffer)[-step_size:]
                            if os.path.exists(ppg_json_file):
                                with open(ppg_json_file, 'r') as f:
                                    try:
                                        ppg_data = json.load(f)
                                    except json.JSONDecodeError:
                                        ppg_data = {"segments": []}
                            else:
                                ppg_data = {"segments": []}
                            ppg_data["segments"].append({"segment": 0, "timestamp": 0, "general_timestamp": 0, "ppg_values": first_ppg})
                            ppg_data["segments"].append({"segment": 1, "timestamp": elapsed_time, "general_timestamp": general_elapsed_time, "ppg_values": second_ppg})
                            with open(ppg_json_file, 'w') as f:
                                json.dump(ppg_data, f, indent=4)
                        else:
                            # For subsequent segments, store only the last step_size samples
                            new_ppg = list(buffer)[-step_size:]
                            if os.path.exists(ppg_json_file):
                                with open(ppg_json_file, 'r') as f:
                                    try:
                                        ppg_data = json.load(f)
                                    except json.JSONDecodeError:
                                        ppg_data = {"segments": []}
                            else:
                                ppg_data = {"segments": []}
                            ppg_data["segments"].append({"segment": current_step, "timestamp": elapsed_time, "general_timestamp": general_elapsed_time, "ppg_values": new_ppg})
                            with open(ppg_json_file, 'w') as f:
                                json.dump(ppg_data, f, indent=4)
                        
                        # EDA raw logging: Instead of splitting in real time, simply drain the eda queue for each iteration.
                        # For each iteration drain all the values from the eda queue and append them to the raw file.
                        if os.path.exists(raw_eda_json_file):
                            with open(raw_eda_json_file, 'r') as f:
                                try:
                                    raw_eda_data = json.load(f)
                                except json.JSONDecodeError:
                                    raw_eda_data = {"raw_eda": []}
                        else:
                            raw_eda_data = {"raw_eda": []}
                        new_eda = []
                        while not eda_data_queue.empty():
                            new_eda.append(eda_data_queue.get())
                        raw_eda_data["raw_eda"].append({"eda_values": new_eda})
                        with open(raw_eda_json_file, 'w') as f:
                            json.dump(raw_eda_data, f, indent=4)
                            
                        # Fetch the ability_value from localhost
                        ability_value = fetch_ability_value(stop_event)

                        # Detect when ability_value turns to 1 and start tracking the ability measurement
                        if ability_value == 1 and not in_analysis:  # and current_step > num_steps_for_baseline + num_steps_skipped:
                            in_analysis = True
                            ability_detected = True
                            ability_index += 1
                            ability_hrv_values = []
                            ability_measurement_step = current_step + 5  # Start measurement after 5 steps
                            log_msg = (f"\nAbility activated in segment {current_step}. \nExpect ability measurements in "
                                    f"segments {ability_measurement_step}, {ability_measurement_step + 1}, and {ability_measurement_step + 2}.\n\n")
                            add_log_entry(log_msg, ability_log=True)
                            
                        # Start logging only for steps 6, 7, and 8 after detection
                        if ability_detected and current_step in [ability_measurement_step, ability_measurement_step + 1, ability_measurement_step + 2]:
                            ability_logging_active = True  # Activate ability logging for these segments
                            ability_hrv_values.append(current_hrv)
                        else:
                            ability_logging_active = False
                        
                        # Reset the in_analysis flag when ability_value returns to 0
                        if ability_value == 0 and in_analysis:
                            in_analysis = False
                            ability_detected = False  # Reset detection flag
            
                        # Simplified rule-based logic:

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
                            log_msg = f"ABILITY MEASUREMENT {label_number} for segment {current_step}.\n"
                            add_log_entry(log_msg, ability_log=True)
                            ability_measurement = True  # Mark that this segment is an ability measurement
                            
                        log_msg = f"Current HRV: {current_hrv}\n"
                        add_log_entry(log_msg, ability_log=ability_logging_active)

                        log_msg = f"Change from Threshold: {change_from_threshold:.2f}%\n"
                        add_log_entry(log_msg, ability_log=ability_logging_active)

                        add_log_entry(f"Binary Output (0: calm, 1: stressed) = {binary_output}\n\n", ability_log=ability_logging_active)
                        
                        
                        # Create a JSON log entry for this analysis segment, including output and ability measurement flag
                        analysis_entry = {
                            "segment": current_step,
                            "timestamp": elapsed_time,
                            "general_timestamp": general_elapsed_time,
                            "HRV": current_hrv,
                            "binary_output": binary_output,
                            "ability_measurement": ability_measurement
                        }
                        
                        # Update the JSON file to maintain a valid JSON object with key "analysis_values"
                        if os.path.exists(analysis_json_file):
                            with open(analysis_json_file, "r") as f:
                                try:
                                    data = json.load(f)
                                except json.JSONDecodeError:
                                    data = {"analysis_values": []}
                        else:
                            data = {"analysis_values": []}
                        data["analysis_values"].append(analysis_entry)
                        with open(analysis_json_file, "w") as f:
                            json.dump(data, f, indent=4)
                            
                        # When deep breathing measurement period is over (after 3 segments), compare the mean HRV
                        if ability_detected and current_step == (ability_measurement_step + 2):
                            mean_ability_hrv = round(np.mean(ability_hrv_values), 3)
                            log_msg = f"Ability {ability_index} comlpeted with mean HRV: {mean_ability_hrv}\n"
                            add_log_entry(log_msg, ability_log=True)
                            if ability_index <= 5 and mean_ability_hrv > best_ability_hrv:
                                best_ability_hrv = mean_ability_hrv
                                best_ability_index = ability_index
                                log_msg = f"New Best Ability with number {ability_index} and mean HRV: {mean_ability_hrv}\n\n\n"
                                add_log_entry(log_msg, ability_log=True)
                            
                            # Reset ability measurement flags after deep breathing measurement period
                            ability_logging_active = False
                            ability_measurement = False
                        
                        
                        # Set the full result with all required values
                        set_analysis_results(
                            current_hrv=current_hrv,
                            binary_output=binary_output,
                            best_ability_index=best_ability_index,
                            best_ability_hrv=best_ability_hrv
                        )
                        
                    step_counter = 0  # Reset step counter after processing

            except queue.Empty:
                if stop_event.is_set():
                    print("Stopping data analysis.")
                    break  # Exit the loop if stop_event is set
                
    except Exception as e:
        error_msg = f"An error occurred during data analysis: {e}\n"
        add_log_entry(error_msg)
        return 0  
         
    return 0

import numpy as np
import queue
from functions.hrv_calculation_functions import detect_peaks, calculate_rmssd
from functions.postprocess_eda import split_raw_eda
from collections import deque
import threading
import time
import json
import os

output_value_lock = threading.Lock()

def calm_calibration(ppg_data_queue, eda_data_queue, stop_event):
    general_log_file = "./logs/general_log.txt"
    current_log_file = "./logs/specific_logs/calm_calibration_log.txt"
    ppg_json_file = "./measurements/calm/calm_ppg_values.json"
    eda_json_file = "./measurements/calm/calm_eda_values.json"
    raw_eda_json_file = "./measurements/calm/calm_raw_eda_values.json"
    calibration_json_file = "./hrv_values/calibration_values.json"

    # Ensure the directories exist
    os.makedirs(os.path.dirname(general_log_file), exist_ok=True)
    os.makedirs(os.path.dirname(ppg_json_file), exist_ok=True)
    os.makedirs(os.path.dirname(eda_json_file), exist_ok=True)
    os.makedirs(os.path.dirname(calibration_json_file), exist_ok=True)
    os.makedirs(os.path.dirname(raw_eda_json_file), exist_ok=True)

    # Function to add log entries to current_log_file and general_log_file
    def add_log_entry(entry, only_general_log=False):
        with open(general_log_file, 'a') as f:
            f.write(entry)
        if not only_general_log:
            with open(current_log_file, 'a') as f:
                f.write(entry)
    
    sampling_rate = 100
    window_size = int(30 * sampling_rate)
    step_size = int(5 * sampling_rate)
    step_counter = 0
    num_steps_for_baseline = 3 #30 for 5 minutes session
    num_steps_skipped = 3      #23 for 5 minutes session
    current_step = 0
    buffer = deque(maxlen=window_size)
    calm_baseline_hrv = []

    # New list to accumulate JSON log entries
    calm_values_list = []

    # For EDA, assume sampling rate of 15Hz; define window and step sizes accordingly
    eda_sampling_rate = 15
    window_size_eda = int(30 * eda_sampling_rate)  # 450 samples (30 sec)
    step_size_eda = int(5 * eda_sampling_rate)       # 75 samples (5 sec)

    # Start the time counter when the function is called
    start_time = time.time()
    
    # Clear the current_log_file at the start
    with open(current_log_file, 'w') as f:
        f.write("Starting calm calibration logging...\n")
        f.write("Waiting for the first window to fill (about 30 seconds)\n\n")
        
    # Create or clear the general_log_file at the start
    with open(general_log_file, 'w') as f:
        f.write("Starting general logging...\n\n")
        f.write("Starting calm calibration logging...\n")
        f.write("Waiting for the first window to fill (about 30 seconds)\n\n")
        
    # Clear the calibration JSON file at the start
    with open(calibration_json_file, 'w') as f:
        json.dump({"calm_values": []}, f, indent=4)
        
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
                buffer.append(data_point)  # Add data to the PPG buffer

                # For EDA, we get values directly from the eda queue (raw logging)
                
                # Increment step counter and check if it's time to analyze the data
                step_counter += 1
                if step_counter >= step_size:
                    if len(buffer) >= window_size:
                        current_step += 1

                        # Process the PPG signal in the buffer
                        peaks, _ = detect_peaks(buffer, lowcut=0.5, highcut=1.5, fs=100, order=3, peak_distance=20)
                        rr_intervals = np.diff(peaks) / sampling_rate
                        current_hrv = calculate_rmssd(rr_intervals)

                        # Calculate the actual timestamp by getting the elapsed time since the function was called
                        elapsed_time = time.time() - start_time
                        timestamp_minutes = int(elapsed_time // 60)  # Convert to minutes
                        timestamp_seconds = int(elapsed_time % 60)   # Get remaining seconds
                        
                        
                        
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
                            ppg_data["segments"].append({"segment": 1, "timestamp": elapsed_time, "general_timestamp": elapsed_time, "ppg_values": second_ppg})
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
                            ppg_data["segments"].append({"segment": current_step, "timestamp": elapsed_time, "general_timestamp": elapsed_time, "ppg_values": new_ppg})
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
                        
                        
                        
                        if current_step == 1:
                            log_msg = f"\nCalm Calibration Starting...\n\n"
                            add_log_entry(log_msg)
                            log_msg = f"\nThe next {num_steps_skipped} segments will be skipped\n\n"
                            add_log_entry(log_msg)

                        if current_step <= num_steps_skipped and current_step >= 1:
                            log_msg = (f"Segment {current_step} skipped - "
                                       f"Timestamp {timestamp_minutes}min {timestamp_seconds}sec\n")
                            add_log_entry(log_msg)
                            
                            log_msg = f"Current HRV: {current_hrv}\n\n"
                            add_log_entry(log_msg)
                            
                            if current_step == num_steps_skipped:
                                log_msg = f"\nThe next {num_steps_for_baseline} segments will be used to calculate the calm baseline.\n\n"
                                add_log_entry(log_msg)
                        
                        elif current_step <= num_steps_skipped + num_steps_for_baseline:
                            calm_baseline_hrv.append(current_hrv)

                            log_msg = (f"Segment {current_step} - "
                                       f"Timestamp {timestamp_minutes}min {timestamp_seconds}sec\n")
                            add_log_entry(log_msg)
                            
                            log_msg = f"Current HRV: {current_hrv}\n\n"
                            add_log_entry(log_msg)
                            
                            # Create a JSON log entry for this segment
                            log_entry = {
                                "segment": current_step,
                                "timestamp": elapsed_time,
                                "general_timestamp": elapsed_time,
                                "HRV": current_hrv
                            }
                            calm_values_list.append(log_entry)
                            
                            # Load existing JSON object, update and save it so we see changes in real time
                            if os.path.exists(calibration_json_file):
                                with open(calibration_json_file, 'r') as json_file:
                                    try:
                                        data = json.load(json_file)
                                    except json.JSONDecodeError:
                                        data = {"calm_values": []}
                            else:
                                data = {"calm_values": []}
                            data["calm_values"].append(log_entry)
                            with open(calibration_json_file, 'w') as json_file:
                                json.dump(data, json_file, indent=4)
                            
                            if current_step == num_steps_skipped + num_steps_for_baseline:
                                # Calculate average baseline HRV after collecting enough data
                                calm_baseline_hrv = round(np.mean(calm_baseline_hrv), 3)
                                add_log_entry(f"Calm Baseline HRV established: {calm_baseline_hrv}\n\n")
                                split_raw_eda(raw_eda_json_file, eda_json_file, ppg_json_file, window_size_eda, step_size_eda)
                                add_log_entry("Calm calibration EDA values postprocessed.\n\n\n", only_general_log=True)
                                add_log_entry("-------------------------------------------------------------\n\n\n", only_general_log=True)
                                add_log_entry("Waiting to recieve the stressed calibration 1 flag...\n\n\n", only_general_log=True)
                                return calm_baseline_hrv, start_time
  
                    step_counter = 0  # Reset step counter after processing

            except queue.Empty:
                if stop_event.is_set():
                    print("Stopping calm calibration.")
                    break  # Exit the loop if stop_event is set
                
    except Exception as e:
        error_msg = f"An error occurred during calm calibration: {e}\n"
        add_log_entry(error_msg)
        return 0, start_time  # Return 0 in case of an error
         
    return 0, start_time

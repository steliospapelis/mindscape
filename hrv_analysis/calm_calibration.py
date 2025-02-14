import numpy as np
import queue
from hrv_calculation_functions import detect_peaks, calculate_rmssd
from collections import deque
import threading
import time
import csv
import json
import os

output_value_lock = threading.Lock()

def calm_calibration(ppg_data_queue, eda_data_queue, stop_event):
    general_log_file = "./logs/general_log.txt"
    current_log_file = "./logs/calm_calibration_log.txt"
    ppg_csv_file = "./measurements/calm_ppg_values.csv"
    eda_csv_file = "./measurements/calm_eda_values.csv"
    hrv_json_file = "./hrv_values/calibration_values.json"

    # Ensure the directories exist
    os.makedirs(os.path.dirname(general_log_file), exist_ok=True)
    os.makedirs(os.path.dirname(ppg_csv_file), exist_ok=True)
    os.makedirs(os.path.dirname(eda_csv_file), exist_ok=True)
    os.makedirs(os.path.dirname(hrv_json_file), exist_ok=True)

    # Write headers only once at the beginning for CSV files
    with open(ppg_csv_file, 'w', newline='') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow(["Segment", "PPG Values"])

    with open(eda_csv_file, 'w', newline='') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow(["Segment", "EDA Values"])

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

                        # Calculate the actual timestamp by getting the elapsed time since the function was called
                        elapsed_time = time.time() - start_time
                        timestamp_minutes = int(elapsed_time // 60)  # Convert to minutes
                        timestamp_seconds = int(elapsed_time % 60)   # Get remaining seconds
                        
                        # Append the segment data to the CSV file
                        with open(ppg_csv_file, 'a', newline='') as csvfile:
                            writer = csv.writer(csvfile)
                            writer.writerow([current_step, list(buffer)])  # Segment number and buffer values
                            
                        # Empty the eda_data_queue into a list
                        # Since sampling rate is 15hz then we should have 75 measurements in a 5 second step
                        # We save only the new 5 second of data for each segment
                        eda_values = []
                        while not eda_data_queue.empty() and len(eda_values) < 75:
                            eda_values.append(eda_data_queue.get())

                        # Write the segment number and EDA values to the EDA CSV file
                        with open(eda_csv_file, 'a', newline='') as csvfile:
                            writer = csv.writer(csvfile)
                            writer.writerow([current_step, eda_values])  # Segment number and EDA values 
                            
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
                                "timestamp_min": timestamp_minutes,
                                "timestamp_sec": timestamp_seconds,
                                "HRV": current_hrv
                            }
                            calm_values_list.append(log_entry)
                            
                            if current_step == num_steps_skipped + num_steps_for_baseline:
                                # Calculate average baseline HRV after collecting enough data
                                calm_baseline_hrv = np.mean(calm_baseline_hrv)
                                add_log_entry(f"Calm Baseline HRV established: {calm_baseline_hrv}\n\n\n")
                                add_log_entry("-------------------------------------------------------------\n\n\n", only_general_log=True)
                                add_log_entry("Waiting to recieve the stressed calibration 1 flag...\n\n\n", only_general_log=True)
                                
                                # Write the JSON object to file
                                with open(hrv_json_file, 'w') as json_file:
                                    json.dump({"calm_values": calm_values_list}, json_file, indent=4)
                                return calm_baseline_hrv, start_time
  
                    step_counter = 0  # Reset step counter after processing

            except queue.Empty:
                if stop_event.is_set():
                    print("Stopping data processor.")
                    break  # Exit the loop if stop_event is set
                
    except Exception as e:
        error_msg = f"An error occurred during calm calibration: {e}\n"
        add_log_entry(error_msg)
        return 0, start_time  # Return 0 in case of an error
         
    return 0, start_time

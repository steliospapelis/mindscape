import warnings
from neurokit2 import NeuroKitWarning
warnings.filterwarnings('ignore', category=NeuroKitWarning)

import neurokit2 as nk
import numpy as np
import queue
from peaks_hrv_functions import detect_peaks, calculate_rmssd
from collections import deque
import threading

output_value_lock = threading.Lock()
results = {}  # This will store all the results

log_file = "data_processor_output.txt"  # The log file name

def add_log_entry(entry):
    # Write log entry to the file in real time
    with open(log_file, 'a') as f:
        f.write(entry)

# Function to set all the output values in a dictionary
def set_results(output_value_local, current_hrv, previous_hrv, previous_three_hrv, change_from_mean_last_three, excluded_value):
    with output_value_lock:
        results['output_value'] = output_value_local
        results['current_hrv'] = current_hrv
        results['previous_hrv'] = previous_hrv
        results['previous_three_hrv'] = previous_three_hrv
        results['change_from_mean_last_three'] = change_from_mean_last_three
        results['excluded_value'] = excluded_value

# Function to get all the results
def get_results():
    with output_value_lock:
        return results

def data_processor(data_queue, stop_event):
    sampling_rate = 100
    window_size = int(30 * sampling_rate)
    step_size = int(5 * sampling_rate)
    step_counter = 0
    num_steps_for_baseline = 12
    current_step = 0
    current_timestamp = 0
    buffer = deque(maxlen=window_size)
    baseline_hrv = []
    previous_hrv_values = deque(maxlen=3)
    
    # Clear the log file at the start
    with open(log_file, 'w') as f:
        f.write("Starting data processor logging...\n")
    
    while not stop_event.is_set() or not data_queue.empty():
        try:
            data_point = data_queue.get(timeout=0.01)
            buffer.append(data_point)  # Add data to the buffer

            # Increment step counter and check if it's time to analyze the data
            step_counter += 1
            if step_counter >= step_size:
                if len(buffer) >= window_size:
                    # Process the PPG signal in the buffer
                    peaks, _ = detect_peaks(buffer, lowcut=1.0, highcut=1.8, fs=25, order=3, peak_distance=2.272)
                    rr_intervals = np.diff(peaks) / sampling_rate
                    current_hrv = calculate_rmssd(rr_intervals)

                    current_step += 1  
                    current_timestamp += 5

                    if current_step < 6:
                        # print(f"Segment {current_step} skipped.")
                        # print(f"    Current HRV: {current_hrv}\n")
                        log_msg = f"Segment {current_step} skipped. Current HRV: {current_hrv}\n"
                        add_log_entry(log_msg)
                        if current_step == 5:
                            # print("\n")
                            log_msg = "\n"
                            add_log_entry(log_msg)
                    
                    elif current_step <= num_steps_for_baseline:
                        baseline_hrv.append(current_hrv)
                        # print(f"Segment {current_step} - Timestamp {current_timestamp//60}min {current_timestamp%60}sec:")
                        # print(f"    Current HRV: {current_hrv}\n")
                        log_msg = f"Segment {current_step} - Timestamp {current_timestamp//60}min {current_timestamp%60}sec: Current HRV: {current_hrv}\n"
                        add_log_entry(log_msg)
                        if current_step == num_steps_for_baseline:
                            # Calculate average baseline HRV after collecting enough data
                            baseline_hrv = np.mean(baseline_hrv)
                            # print(f"Baseline HRV established: {baseline_hrv}\n\n\n")
                            add_log_entry(f"Baseline HRV established: {baseline_hrv}\n\n\n")
                            
                    else:
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

                        # print(f"Segment {current_step} - Timestamp {current_timestamp//60}min {current_timestamp%60}sec:")
                        # print(f"  Current HRV: {current_hrv}")
                        # print(f"  Change from Baseline: {change_from_baseline:.2f}%")
                        # print(f"  Change from Previous: {change_from_previous:.2f}%")
                        log_msg = f"Segment {current_step} - Timestamp {current_timestamp//60}min {current_timestamp%60}sec: Current HRV: {current_hrv}, Change from Baseline: {change_from_baseline:.2f}%, Change from Previous: {change_from_previous:.2f}%\n"
                        add_log_entry(log_msg)
                        
                        if previous_hrv_for_change is not None:
                            # print(f"  Previous HRV for Change: {previous_hrv_for_change}")
                            log_msg = f"  Previous HRV for Change: {previous_hrv_for_change}\n"
                            add_log_entry(log_msg)
                            
                        # print(f"  Previous Three HRV Values: {list(previous_hrv_values)}")
                        # print(f"  Change from Mean of Last Three: {change_from_mean_last_three:.2f}%")
                        log_msg = f"  Previous Three HRV Values: {list(previous_hrv_values)}\n"
                        add_log_entry(log_msg)
                        log_msg = f"  Change from Mean of Last Three: {change_from_mean_last_three:.2f}%\n"
                        add_log_entry(log_msg)

                        if excluded_value is not None:
                            # print(f"  Excluded Value from Mean Calculation: {excluded_value}\n")
                            log_msg = f"  Excluded Value from Mean Calculation: {excluded_value}\n"
                            add_log_entry(log_msg)
                            
                        else:
                            # print(f"  No Value Excluded from Mean Calculation\n")
                            log_msg = f"  No Value Excluded from Mean Calculation\n"
                            add_log_entry(log_msg)                
                                                    
                        output_value_local = 0
                        total_change_from_previous = 0.8 * change_from_mean_last_three + 0.2 * change_from_previous
                        if change_from_baseline <= -6:
                            output_value_local = 1
                        elif change_from_baseline <= 5 and total_change_from_previous <= -5:
                            output_value_local = 1
                            
                        # print(f"Output value = {output_value_local}")
                        add_log_entry(f"Output value = {output_value_local}\n\n")

                        # Set the full result with all required values
                        set_results(
                            output_value_local=output_value_local,
                            current_hrv=current_hrv,
                            previous_hrv=previous_hrv_for_change,
                            previous_three_hrv=list(previous_hrv_values),
                            change_from_mean_last_three=change_from_mean_last_three,
                            excluded_value=excluded_value
                        )

                step_counter = 0  # Reset step counter after processing

        except queue.Empty:
            if stop_event.is_set():
                print("Stopping data processor.")
                break  # Exit the loop if stop_event is set

import warnings
from neurokit2 import NeuroKitWarning
import numpy as np
import pandas as pd
from collections import deque
from peaks_hrv_functions import detect_peaks, calculate_rmssd  # Assuming these functions are defined elsewhere
import matplotlib.pyplot as plt

warnings.filterwarnings('ignore', category=NeuroKitWarning)

# Read the CSV data
data = pd.read_csv("./sd_data/100hz_18-10-24/2024-10-18_12-48-08-471817_PG.csv")
ppg_signal = data['PG']
ppg_timestamp = data['EmotiBitTimestamp']

# Calculate the sampling rate
sampling_rate = 1000 / (ppg_timestamp.diff().median())  # Assuming timestamps are in milliseconds

# Initialize parameters
window_size = int(30 * sampling_rate)  
step_size = int(5 * sampling_rate)
buffer = deque(maxlen=window_size)
baseline_hrv = []
previous_hrv_values = deque(maxlen=3)

num_steps_for_baseline = 10
num_steps_skipped = 10
current_step = 0
current_timestamp = 0

# Open a log file to record the results (optional logging similar to real-time implementation)
log_file = 'hrv_analysis_log.txt'
with open(log_file, 'w') as f:
    f.write("Starting HRV analysis from CSV data...\n")

def add_log_entry(message):
    """Add log entry to a file"""
    with open(log_file, 'a') as f:
        f.write(message)

# Loop over the signal in chunks
for start in range(0, len(ppg_signal), step_size):
    end = start + window_size
    chunk = ppg_signal[start:end]
    buffer.extend(chunk)

    # Process the PPG signal to find peaks and calculate HRV
    if len(buffer) >= window_size:
        # Calculate peaks and HRV using the provided functions
        peaks, filtered_signal = detect_peaks(buffer, lowcut=0.5, highcut=2, fs=sampling_rate, order=3, peak_distance=20)
        rr_intervals = np.diff(peaks) / sampling_rate
        current_hrv = calculate_rmssd(rr_intervals)

        current_step += 1
        current_timestamp += 5

        # Skip the first few steps to gather baseline data
        if current_step <= num_steps_skipped:
            log_msg = f"Segment {current_step} skipped. Current HRV: {current_hrv}\n"
            add_log_entry(log_msg)
            if current_step == num_steps_skipped:
                add_log_entry("\n")

        # Collect baseline HRV data
        elif current_step <= num_steps_skipped + num_steps_for_baseline:
            baseline_hrv.append(current_hrv)
            log_msg = f"Segment {current_step} - Timestamp {current_timestamp//60}min {current_timestamp%60}sec: Current HRV: {current_hrv}\n"
            add_log_entry(log_msg)
            if current_step == num_steps_skipped + num_steps_for_baseline:
                baseline_hrv = np.mean(baseline_hrv)
                add_log_entry(f"Baseline HRV established: {baseline_hrv}\n\n\n")

        # Compare current HRV to baseline and previous HRV
        else:
            change_from_baseline = (current_hrv - baseline_hrv) / baseline_hrv * 100
            if len(previous_hrv_values) > 0:
                change_from_previous = (current_hrv - previous_hrv_values[-1]) / previous_hrv_values[-1] * 100
                previous_hrv_for_change = previous_hrv_values[-1]
            else:
                change_from_previous = 0
                previous_hrv_for_change = None

            previous_hrv_values.append(current_hrv)

            # Calculate mean of the last three HRV values and detect outliers
            if len(previous_hrv_values) == 3:
                previous_hrv_array = np.array(previous_hrv_values)
                mean_last_three_hrv = np.mean(previous_hrv_array)
                excluded_value = None

                # Exclude outliers differing more than 20%
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

            log_msg_hrv = f"Segment {current_step} - Timestamp {current_timestamp//60}min {current_timestamp%60}sec\n" 
            add_log_entry(log_msg_hrv)
            
            log_msg_hrv = f"Current HRV: {current_hrv}\n"
            add_log_entry(log_msg_hrv)

            log_msg_change_baseline = f"Change from Baseline: {change_from_baseline:.2f}%\n"
            add_log_entry(log_msg_change_baseline)

            log_msg_change_previous = f"Change from Previous: {change_from_previous:.2f}%\n"
            add_log_entry(log_msg_change_previous)

            if previous_hrv_for_change is not None:
                add_log_entry(f"  Previous HRV for Change: {previous_hrv_for_change}\n")

            add_log_entry(f"  Previous Three HRV Values: {list(previous_hrv_values)}\n")
            add_log_entry(f"  Change from Mean of Last Three: {change_from_mean_last_three:.2f}%\n")

            if excluded_value is not None:
                add_log_entry(f"  Excluded Value from Mean Calculation: {excluded_value}\n")
            else:
                add_log_entry(f"  No Value Excluded from Mean Calculation\n")

            # Binary and categorical outputs
            binary_output = 0
            total_change_from_previous = 0.8 * change_from_mean_last_three + 0.2 * change_from_previous
            if change_from_baseline <= -5:
                binary_output = 1
            elif change_from_baseline <= 5 and total_change_from_previous <= -5:
                binary_output = 1

            categorical_output = 0  # Default to very calm
            if change_from_baseline <= -10:
                categorical_output = 3  # Very anxious
            if -10 < change_from_baseline <= -5:
                categorical_output = 2  # Anxious
            elif -5 < change_from_baseline < 0:
                categorical_output = 1  # Calm
            elif change_from_baseline <= 5 and total_change_from_previous <= -7.5:
                categorical_output = 3  # Very anxious
            elif change_from_baseline <= 5 and total_change_from_previous <= -5:
                categorical_output = 2  # Anxious
            elif change_from_baseline <= 5 and total_change_from_previous <= -2.5:
                categorical_output = 1  # Calm

            # Log both binary and categorical outputs
            add_log_entry(f"  Binary Output (0: calm, 1: anxious) = {binary_output}\n")
            add_log_entry(f"  Categorical Output (0: very calm, 1: calm, 2: anxious, 3: very anxious) = {categorical_output}\n\n")

# End of analysis
add_log_entry("HRV analysis from CSV data completed.\n")

import warnings
from neurokit2 import NeuroKitWarning

warnings.filterwarnings('ignore', category=NeuroKitWarning)

#pip install neurokit2
import neurokit2 as nk
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np


#data = pd.read_csv("C:/Users/nikol/Desktop/University/8th_semester/biomedical_technologies/data/2024-04-15_12-23-39-837583_PG.csv")
data = pd.read_csv("C:/Users/nikol/Desktop/University/8th_semester/biomedical_technologies/data_3/2024-04-22_16-49-57-721143_PG.csv")
ppg_signal = data['PG']
ppg_timestamp = data['EmotiBitTimestamp']

# Assuming the EmotiBitTimestamp is in milliseconds and represents the sampling frequency
# Calculate sampling rate based on median diff
sampling_rate = 1000 / (ppg_timestamp.diff().median())  #25 with osc prototcol no timestamp

window_size = int(30 * sampling_rate)  # 30 seconds window
step_size = int(5 * sampling_rate)  # 10 seconds step (how often hrv is updated)

buffer = np.array([])  # Buffer to store windowed data
baseline_hrv = []  # List to store baseline HRV values

num_steps_for_baseline = 12
current_step = 0
current_timestamp = 0

# Loop over the signal in chunks
for start in range(0, len(ppg_signal), step_size):
    end = start + window_size
    chunk = ppg_signal[start:end]
    buffer = np.concatenate((buffer, chunk))[-window_size:]  # Keep the last 'window_size' samples

    # Process the PPG signal to find peaks and calculate HRV
    if len(buffer) >= window_size:
        signals, info = nk.ppg_process(buffer, sampling_rate=sampling_rate)
        rpeaks = info['PPG_Peaks']
        hrv_indices = nk.hrv(rpeaks, sampling_rate=sampling_rate, show=False, domain=["time", "frequency"])
        current_hrv = hrv_indices['HRV_RMSSD'].iloc[0]  # Extract RMSSD

        current_step += 1  # Increment the step counter
        current_timestamp += 5
        if current_step < 6:
            print(f"Segment {current_step} skipped.")
            print(f"    Current HRV: {current_hrv}\n")
            if current_step == 5:
                print("\n")
            
        elif current_step <= num_steps_for_baseline:
            baseline_hrv.append(current_hrv)  # Collect HRV values for baseline
            print(f"Segment {current_step} - Timestamp {current_timestamp//60}min {current_timestamp%60}sec:")
            print(f"    Current HRV: {current_hrv}\n")
            if current_step == num_steps_for_baseline:
                # Calculate average baseline HRV after collecting enough data
                baseline_hrv = np.mean(baseline_hrv)
                print(f"Baseline HRV established: {baseline_hrv}\n\n\n")
                
        else:
            # Compare to baseline and previous HRV
            change_from_baseline = (current_hrv - baseline_hrv) / baseline_hrv * 100
            change_from_previous = (current_hrv - previous_hrv) / previous_hrv * 100 if 'previous_hrv' in locals() else 0
            print(f"Segment {current_step} - Timestamp {current_timestamp//60}min {current_timestamp%60}sec:")
            print(f"  Current HRV: {current_hrv}")
            print(f"  Change from Baseline: {change_from_baseline:.2f}%")
            print(f"  Change from Previous: {change_from_previous:.2f}%\n")
        # Store current HRV as previous HRV for the next iteration
        previous_hrv = current_hrv
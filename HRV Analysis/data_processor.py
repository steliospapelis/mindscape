#function used to calculate hrv in real time (based on hrv_calculator)

import warnings
from neurokit2 import NeuroKitWarning

warnings.filterwarnings('ignore', category=NeuroKitWarning)

#pip install neurokit2
import neurokit2 as nk
import pandas as pd
import numpy as np
import queue
from peaks_hrv_functions import detect_peaks, calculate_rmssd
from collections import deque

def data_processor(data_queue, stop_event):
    """
    Continuously process data from the queue until the stop event is set.
    
    Parameters:
        data_queue (queue.Queue): The queue from which data will be processed.
        stop_event (threading.Event): Event to signal the stop of the data processing.
    """
    
    sampling_rate = 25
    window_size = int(30 * sampling_rate)
    step_size = int(5 * sampling_rate)
    step_counter = 0
    num_steps_for_baseline = 12
    current_step = 0
    current_timestamp = 0
    buffer = deque(maxlen=window_size)
    baseline_hrv = []
    
    while not stop_event.is_set() or not data_queue.empty():
        try:
            data_point = data_queue.get(timeout=0.01)
            buffer.append(data_point)  # Add data to the buffer

            # Increment step counter and check if it's time to analyze the data
            step_counter += 1
            if step_counter >= step_size:
                if len(buffer) >= window_size:
                    # Process the PPG signal in the buffer
                    #neurokit had many errors for hrv calculation -> used our functions for both praks and hrv
                    peaks, filtered_signal = detect_peaks(buffer, lowcut=1.0, highcut=1.8, fs=25, order=3, peak_distance=2.272)
                    rr_intervals = np.diff(peaks) / sampling_rate
                    current_hrv = calculate_rmssd(rr_intervals)
                    #print(f"My peaks: {peaks}\n")
                    #hrv_indices = nk.hrv(peaks, sampling_rate=sampling_rate, show=False)
                    #current_hrv = hrv_indices.iloc[0]  # Extract HRV indices

                    print(f"HRV Calculated: {current_hrv}")
                    print(f"My peaks: {peaks}")
                    #print(f"Buffer: {buffer}\n")

                step_counter = 0  # Reset step counter after processing

        except queue.Empty:
            continue  # Continue if no data is available
        


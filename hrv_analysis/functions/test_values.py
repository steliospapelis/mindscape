import numpy as np
import queue
from functions.hrv_calculation_functions import detect_peaks, calculate_rmssd
from collections import deque
import time

def test_values(ppg_data_queue, stop_event, get_state):
    current_log_file = "./logs/test_log.txt"

    def add_log_entry(entry):
        with open(current_log_file, 'a') as f:
            f.write(entry)

    sampling_rate = 100
    window_size = int(30 * sampling_rate)    # 30-second window
    step_size = int(5 * sampling_rate)         # 5-second step
    step_counter = 0
    current_step = 0
    buffer = deque(maxlen=window_size)

    start_time = time.time()

    # Clear the test log file at the start
    with open(current_log_file, 'w') as f:
        f.write("Testing HRV values logging...\n")
        f.write("Waiting for the first window to fill (about 30 seconds)...\n\n")

    try:
        while not stop_event.is_set() or not ppg_data_queue.empty():
            # Check if state is still TESTING; if not, exit
            if get_state() != "TESTING":
                break
            try:
                data_point = ppg_data_queue.get(timeout=0.01)
                buffer.append(data_point)

                step_counter += 1
                if step_counter >= step_size:
                    if len(buffer) >= window_size:
                        current_step += 1

                        # Process the PPG signal
                        peaks, _ = detect_peaks(
                            buffer, 
                            lowcut=0.5, 
                            highcut=1.5, 
                            fs=sampling_rate, 
                            order=3, 
                            peak_distance=20
                        )
                        rr_intervals = np.diff(peaks) / sampling_rate
                        current_hrv = calculate_rmssd(rr_intervals)

                        elapsed_time = time.time() - start_time
                        timestamp_minutes = int(elapsed_time // 60)
                        timestamp_seconds = int(elapsed_time % 60)
                        log_msg = (f"({current_step}) - "
                                   f"({timestamp_minutes}min {timestamp_seconds}sec) - "
                                   f"Current HRV: {current_hrv}\n")
                        add_log_entry(log_msg)
                    step_counter = 0
            except queue.Empty:
                if stop_event.is_set():
                    break
    except Exception as e:
        error_msg = f"An error occurred during HRV testing: {e}\n"
        add_log_entry(error_msg)
        return 0
    return 0

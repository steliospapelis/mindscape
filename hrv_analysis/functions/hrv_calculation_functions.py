import numpy as np
from scipy.signal import butter, filtfilt, find_peaks

def butter_bandpass(lowcut, highcut, fs, order=3):
    nyq = 0.5 * fs
    low = lowcut / nyq
    high = highcut / nyq
    b, a = butter(order, [low, high], btype='band')
    return b, a

def butter_bandpass_filter(data, lowcut, highcut, fs, order=3):
    b, a = butter_bandpass(lowcut, highcut, fs, order)
    y = filtfilt(b, a, data)
    return y

def detect_peaks(data, lowcut, highcut, fs, order, peak_distance):
    filtered_signal = butter_bandpass_filter(data, lowcut, highcut, fs, order)
    peaks, _ = find_peaks(filtered_signal, distance=peak_distance)
    return peaks, filtered_signal

def calculate_rmssd(rr_intervals):
    if len(rr_intervals) > 1:
        rr_diff = np.diff(rr_intervals)
        rr_diff_squared = np.square(rr_diff)
        mean_rr_diff_squared = np.mean(rr_diff_squared)
        rmssd = np.sqrt(mean_rr_diff_squared) * 1000 # Convert from seconds to milliseconds
        return round(rmssd, 3)
    else:
        return None
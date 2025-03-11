import os
import json
import numpy as np
import scipy.signal as signal

# Function to apply a bandpass filter to isolate respiratory components
def bandpass_filter(ppg_signal, lowcut=0.1, highcut=0.4, fs=100, order=3):
    nyquist = 0.5 * fs
    low = lowcut / nyquist
    high = highcut / nyquist
    b, a = signal.butter(order, [low, high], btype='band')
    return signal.filtfilt(b, a, ppg_signal)

# Function to calculate respiratory rate using FFT
def calculate_respiration_rate(ppg_signal, fs=100):
    n = len(ppg_signal)
    freqs = np.fft.rfftfreq(n, d=1/fs)
    fft_values = np.abs(np.fft.rfft(ppg_signal))
    
    # Restrict to respiratory range (0.1-0.4 Hz)
    valid_idx = np.where((freqs >= 0.1) & (freqs <= 0.4))[0]
    
    if len(valid_idx) == 0:
        return None  # No valid frequency found
    
    peak_freq = freqs[valid_idx[np.argmax(fft_values[valid_idx])]]
    respiration_rate_bpm = peak_freq * 60  # Convert Hz to BPM
    return respiration_rate_bpm

# Main function to process the PPG JSON file
def process_ppg_json(results_folder):
    base_path = f'../results/{results_folder}/measurements/analysis/analysis_ppg_values.json'
    
    if not os.path.exists(base_path):
        print(f"Error: File not found at {base_path}")
        return
    
    with open(base_path, 'r') as f:
        data = json.load(f)
    
    segments = data['segments']
    segment_rr = {}

    for i in range(6, len(segments)):
        current_segment = segments[i]['segment']
        ppg_values = segments[i]['ppg_values']

        # Ensure 30-second window by collecting last 6 segments (each 5 sec)
        window_ppg = []
        for j in range(max(0, i-5), i+1):
            window_ppg.extend(segments[j]['ppg_values'])
        
        # if len(window_ppg) < 3000:  # Ensure at least 30 seconds of data (assuming 100 Hz)
        #     continue
        
        # Preprocess PPG signal
        filtered_ppg = bandpass_filter(np.array(window_ppg))
        
        # Calculate respiration rate
        rr_bpm = calculate_respiration_rate(filtered_ppg)
        
        # Store result
        segment_rr[current_segment] = rr_bpm

    # Save results
    # output_path = 'respiration_rate_results.json'
    output_path = f'../results/{results_folder}/respiration_rate_results.json'
    with open(output_path, 'w') as f:
        json.dump(segment_rr, f, indent=4)
    
    print(f"Respiration rate results saved to {output_path}")

# Example usage
if __name__ == "__main__":
    results_folder = "results_2025-03-10_12-35-56"  # Change this as needed
    process_ppg_json(results_folder)

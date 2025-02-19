import json 
import os
import numpy as np

def split_raw_eda(raw_eda_file, output_file, ppg_json_file, window_size_eda, step_size_eda):
    """
    Reads the raw EDA values from raw_eda_file (which is a JSON object with key "raw_eda" 
    containing a list of entries, each with a "eda_values" list).
    It then flattens all the EDA values in order and splits them into segments:
      - Segment 0: first (window_size_eda - step_size_eda) samples (i.e. initial 25 sec)
      - Each subsequent segment: step_size_eda samples.
    Additionally, for each segment, it extracts the timestamp fields ("timestamp", "timestamp_min", "timestamp_sec")
    from the corresponding segment in the ppg_json_file and adds them to the EDA segment.
    The resulting segmentation is saved as a JSON object with key "segments" in output_file.
    """
    # Load raw EDA values
    if os.path.exists(raw_eda_file):
        with open(raw_eda_file, 'r') as f:
            try:
                raw_data = json.load(f)
            except json.JSONDecodeError:
                raw_data = {"raw_eda": []}
    else:
        raw_data = {"raw_eda": []}
    
    # Load PPG JSON data
    if os.path.exists(ppg_json_file):
        with open(ppg_json_file, 'r') as f:
            try:
                ppg_data = json.load(f)
            except json.JSONDecodeError:
                ppg_data = {"segments": []}
    else:
        ppg_data = {"segments": []}
    
    # Flatten the raw EDA values
    all_eda = []
    for entry in raw_data["raw_eda"]:
        all_eda.extend(entry.get("eda_values", []))
    
    segments = []
    # Segment 0: first (window_size_eda - step_size_eda) samples
    first_segment_length = window_size_eda - step_size_eda
    segment0 = {"segment": 0}
    # Extract timestamp from corresponding PPG segment (if available)
    for seg in ppg_data.get("segments", []):
        if seg.get("segment") == 0:
            segment0["timestamp"] = seg.get("timestamp")
            break
    segment0["eda_values"] = all_eda[:first_segment_length]
    segments.append(segment0)
    
    # Remaining samples: split into chunks of step_size_eda
    remaining = all_eda[first_segment_length:]
    seg_num = 1
    for i in range(0, len(remaining), step_size_eda):
        chunk = remaining[i:i+step_size_eda]
        segment_entry = {"segment": seg_num}
        # Extract timestamp from corresponding PPG segment
        for seg in ppg_data.get("segments", []):
            if seg.get("segment") == seg_num:
                segment_entry["timestamp"] = seg.get("timestamp")
                break
        segment_entry["eda_values"] = chunk
        segments.append(segment_entry)
        seg_num += 1

    # Save the segmented EDA values to the output file
    with open(output_file, 'w') as f:
        json.dump({"segments": segments}, f, indent=4)

# Example usage (this block can be removed or commented out when integrating):
if __name__ == "__main__":
    raw_eda_file = "./measurements/raw_eda_values.json"
    output_file = "./measurements/processed_eda_values.json"
    ppg_json_file = "./measurements/calm_ppg_values.json"
    # For a 30-second window at 15Hz, window_size_eda = 450, step_size_eda = 75
    split_raw_eda(raw_eda_file, output_file, ppg_json_file=ppg_json_file, window_size_eda=450, step_size_eda=75)

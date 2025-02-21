import json
import os
import numpy as np

def compute_threshold():
    """
    This function reads the HRV calibration data from the JSON file (which contains 4 objects: 
    calm_values and stressed_values_1, stressed_values_2, stressed_values_3).
    It computes the calm baseline from the calm_values object, and for each of the stressed 
    calibration objects it computes the stressed baseline as the mean of the HRV values.
    It then computes:
      - mean_all_stressed: the mean of the three stressed baselines,
      - standard_deviation_all: the standard deviation of the three stressed baselines,
      - threshold: the mean of only the last two stressed baselines (stressed_values_2 and stressed_values_3),
    which will be used as the decision threshold in data analysis.
    It logs back to the JSON file the computed values under a new key "baselines" with the following fields:
        - calm_baseline
        - stressed_baseline_1
        - stressed_baseline_2
        - stressed_baseline_3
        - threshold
    The updated JSON object is written back to the same calibration file.
    The function returns the threshold.
    """
    # Define the path to the existing calibration file
    calibrations_file = "./hrv_values/calibration_values.json"
    
    # Load the calibration data from file; if the file doesn't exist or is empty, use an empty dictionary
    if os.path.exists(calibrations_file):
        with open(calibrations_file, 'r') as f:
            try:
                data = json.load(f)
            except json.JSONDecodeError:
                data = {}
    else:
        data = {}
    
    # Compute calm baseline from calm_values object
    calm_values = []
    if "calm_values" in data:
        for entry in data["calm_values"]:
            if "HRV" in entry:
                calm_values.append(entry["HRV"])
    if calm_values:
        calm_baseline = float(np.mean(calm_values))
    else:
        calm_baseline = None

    # Compute stressed baselines for each stressed calibration object
    stressed_baseline_1 = None
    stressed_baseline_2 = None
    stressed_baseline_3 = None
    for key, var_name in zip(["stressed_values_1", "stressed_values_2", "stressed_values_3"],
                             ["stressed_baseline_1", "stressed_baseline_2", "stressed_baseline_3"]):
        values = []
        if key in data:
            for entry in data[key]:
                if "HRV" in entry:
                    values.append(entry["HRV"])
        if values:
            baseline = float(np.mean(values))
        else:
            baseline = None
        if var_name == "stressed_baseline_1":
            stressed_baseline_1 = baseline
        elif var_name == "stressed_baseline_2":
            stressed_baseline_2 = baseline
        elif var_name == "stressed_baseline_3":
            stressed_baseline_3 = baseline

    # Compute the mean of all 3 stressed baselines (if available)
    stressed_baselines = [b for b in [stressed_baseline_1, stressed_baseline_2, stressed_baseline_3] if b is not None]
    if stressed_baselines:
        mean_all_stressed = float(np.mean(stressed_baselines))
        standard_deviation_all = float(np.std(stressed_baselines))
    else:
        mean_all_stressed = None
        standard_deviation_all = None

    # Compute the mean of only the last two stressed baselines as the threshold
    if stressed_baseline_2 is not None and stressed_baseline_3 is not None:
        threshold = float(np.mean([stressed_baseline_2, stressed_baseline_3]))
    else:
        threshold = None

    # Create the baselines dictionary for logging
    baselines_dict = {
        "calm_baseline": calm_baseline,
        "stressed_baseline_1": stressed_baseline_1,
        "stressed_baseline_2": stressed_baseline_2,
        "stressed_baseline_3": stressed_baseline_3,
        "threshold": threshold
    }
    
    # Add or update the "baselines" key in the JSON data
    data["baselines"] = baselines_dict
    
    # Write the updated JSON object back to the same file
    with open(calibrations_file, 'w') as f:
        json.dump(data, f, indent=4)
    
    return threshold

# Example usage (this block can be removed when integrating into your pipeline)
if __name__ == "__main__":
    threshold_value = compute_threshold()
    print("Threshold:", threshold_value)

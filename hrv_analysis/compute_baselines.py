import json
import os
import numpy as np

def compute_baselines(calm_baseline, stressed_baseline_1, stressed_baseline_2, stressed_baseline_3):
    """
    This function takes as arguments the four baseline values (one calm and three stressed).
    It calculates the mean of the 3 stressed baselines (stressed_baseline_hrv).
    Then it opens the calibrations_values.json file (which contains the HRV values for the calibrations),
    extracts all HRV values from the three stressed calibrations (from the JSON objects stressed_values_1, stressed_values_2, stressed_values_3),
    and computes the standard deviation of these values.
    Finally, it adds a new key "baselines" to the JSON object with the following fields:
        - calm_baseline
        - stressed_baseline (the computed mean of the 3 stressed baselines)
        - standard_deviation
        - stressed_baseline_1
        - stressed_baseline_2
        - stressed_baseline_3
    The updated JSON object is written back to the same calibrations_values.json file.
    The function returns the computed stressed_baseline_hrv and the standard deviation.
    """
    # Calculate the mean of the three stressed baselines
    stressed_baseline_hrv = np.mean([stressed_baseline_1, stressed_baseline_2, stressed_baseline_3])
    
    # Define the path to the existing calibrations file
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
    
    # Extract HRV values from each of the three stressed calibration entries
    stressed_hrv_values = []
    for key in ["stressed_values_1", "stressed_values_2", "stressed_values_3"]:
        if key in data:
            for entry in data[key]:
                if "HRV" in entry:
                    stressed_hrv_values.append(entry["HRV"])
    
    # Compute the standard deviation of all stressed HRV values (if available)
    if stressed_hrv_values:
        standard_deviation = float(np.std(stressed_hrv_values))
    else:
        standard_deviation = None
    
    # Create the baselines dictionary
    baselines_dict = {
        "calm_baseline": calm_baseline,
        "stressed_baseline": float(stressed_baseline_hrv),
        "standard_deviation": standard_deviation,
        "stressed_baseline_1": stressed_baseline_1,
        "stressed_baseline_2": stressed_baseline_2,
        "stressed_baseline_3": stressed_baseline_3
    }
    
    # Add or update the "baselines" key in the JSON data
    data["baselines"] = baselines_dict
    
    # Write the updated JSON object back to the same file
    with open(calibrations_file, 'w') as f:
        json.dump(data, f, indent=4)
    
    return stressed_baseline_hrv, standard_deviation

# Example usage (this block can be removed when integrating into your pipeline)
if __name__ == "__main__":
    # Example baseline values (replace these with actual baseline values)
    calm_baseline = 500
    stressed_baseline_1 = 400
    stressed_baseline_2 = 420
    stressed_baseline_3 = 410

    stressed_mean, std_dev = compute_baselines(calm_baseline, stressed_baseline_1, stressed_baseline_2, stressed_baseline_3)
    print("Stressed Baseline HRV:", stressed_mean)
    print("Standard Deviation:", std_dev)

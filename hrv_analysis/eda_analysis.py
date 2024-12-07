import warnings
from neurokit2 import NeuroKitWarning

# Attempt to suppress specific neurokit warnings globally
warnings.filterwarnings('ignore', category=NeuroKitWarning)

import neurokit2 as nk
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

#EDA DATA - SAMPLING FREQ = 15HZ

file_path = "C:/Users/nikol/Desktop/unity/project_biomed/mindscape/hrv_analysis/sd_data/08-11-24_EDA_test/2024-11-08_15-17-42-389105_EA.csv"
data = pd.read_csv(file_path) 
# data.head(20)

eda_signal = data['EA']
# eda_signal.head(5)

eda_timestamp = data['EmotiBitTimestamp']
# eda_timestamp.head(5)

# Assuming the EmotiBitTimestamp is in milliseconds and represents the sampling frequency
# Calculate sampling rate based on median diff
sampling_rate = 1000 / (eda_timestamp.diff().median()) #~100hz
print("Emotibit Sampling Rate (Hz): ", sampling_rate)

# perfect_sampling_rate = 15
# print("Perfect Signal Sampling Rate (Hz): ", perfect_sampling_rate)


window_size = int(30 * sampling_rate)  # 30 seconds window

step_size = int(5 * sampling_rate)  # 10 seconds step (how often hrv is updated)
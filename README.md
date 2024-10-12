# Mindscape Odyssey

**Mindscape Odyssey** is a serious platformer game developed using Unity, with the aim of helping people cope with anxiety. The game uses real-time data from a sensor, **EmotiBit**, to detect whether the player is experiencing anxiety and dynamically adjusts the game experience based on that input.

## Features

- **Anxiety Management Techniques:** The game incorporates various methods for managing anxiety, such as deep breathing exercises. These techniques are not only taught but are also part of the game mechanics. For example, real-time deep breathing can help players restore health while playing.
- **Real-Time Data Integration:** By using the **EmotiBit** sensor, the game measures the player's **PPG (Photoplethysmography)** data in real-time to calculate **RMSSD (Root Mean Square of Successive Differences)** heart rate variability (HRV), which is used as an indicator of anxiety levels.

- **Adaptive Gameplay:** Based on the HRV values, the game adjusts its difficulty, character abilities, and other environmental factors to reflect the player's current anxiety state.

## Technology Stack

- **Game Engine:** Unity
- **Sensor:** EmotiBit
- **Data Metrics:** RMSSD HRV calculated from PPG data

## How It Works

1. **EmotiBit Sensor:** The game connects to an EmotiBit sensor that captures the player's physiological data, including PPG signals.
2. **Anxiety Detection:** The game computes the RMSSD HRV in real time from the PPG data to detect fluctuations in the player's anxiety levels.
3. **Dynamic Adjustments:** The gameplay experience, including difficulty levels and available abilities (e.g., deep breathing), changes dynamically based on the detected anxiety levels.

## Contributors

- **Papelis Stelios** (steliospapelis@gmail.com)
- **Bothos Vouterakos Nikolaos** (nikolasbv10@gmail.com)

## License

This project is licensed under the MIT License - see the LICENSE.md file for details.

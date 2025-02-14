import requests
import keyboard
import time
import threading

# Global variable for ability signal value
binary_value = 0
value_lock = threading.Lock()

# Function to send testing signal
def send_testing_signal(value):
    try:
        response = requests.post('http://127.0.0.1:5000/game_flags', json={"Testing": value})
        if response.status_code == 200:
            print(f"Testing signal sent with value: {value}")
        else:
            print(f"Failed to send testing signal: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")
        
# Function to send calm calibration signal
def send_calm_calibration_signal(value):
    try:
        response = requests.post('http://127.0.0.1:5000/game_flags', json={"CalmCalib": value})
        if response.status_code == 200:
            print(f"Calm calibration signal sent with value: {value}")
        else:
            print(f"Failed to send calm calibration signal: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")

# Function to send stressed calibration 1 signal
def send_stressed_calibration_signal_1(value):
    try:
        response = requests.post('http://127.0.0.1:5000/game_flags', json={"StressedCalib1": value})
        if response.status_code == 200:
            print(f"Stressed calibration signal sent with value: {value}")
        else:
            print(f"Failed to send stressed calibration 1 signal: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")
        
# Function to send stressed calibration 2 signal
def send_stressed_calibration_signal_2(value):
    try:
        response = requests.post('http://127.0.0.1:5000/game_flags', json={"StressedCalib2": value})
        if response.status_code == 200:
            print(f"Stressed calibration 2 signal sent with value: {value}")
        else:
            print(f"Failed to send stressed calibration 2 signal: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")

# Function to send stressed calibration 3 signal
def send_stressed_calibration_signal_3(value):
    try:
        response = requests.post('http://127.0.0.1:5000/game_flags', json={"StressedCalib3": value})
        if response.status_code == 200:
            print(f"stressed calibration 3 signal sent with value: {value}")
        else:
            print(f"Failed to send stressed calibration 3 signal: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")
        
# Function to send data analysis signal
def send_data_analysis_signal(value):
    try:
        response = requests.post('http://127.0.0.1:5000/game_flags', json={"DataAnalysis": value})
        if response.status_code == 200:
            print(f"Data analysis signal sent with value: {value}")
        else:
            print(f"Failed to send data analysis signal: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")

# Function to send ability signal
def send_ability_value(value):
    try:
        response = requests.post('http://127.0.0.1:5000/game_flags', json={"Breathing": value})
        if response.status_code == 200:
            print(f"Sent ability value: {value}")
        else:
            print(f"Failed to send ability value: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")

# Reset ability signal value after 45 seconds
def reset_ability_value_after_delay():
    global binary_value
    time.sleep(45)
    with value_lock:
        binary_value = 0
        print("Ability value reset to 0 after 45 seconds.")
        send_ability_value(binary_value)

# Main keypress check function
def check_for_keypress():
    global binary_value
    while True:
        # Testing: Press '!'
        if keyboard.is_pressed('!'):
            print("Testing signal triggered.")
            send_testing_signal(1)
            time.sleep(0.5)
            
        # Calm calibration: Press '@'
        elif keyboard.is_pressed('@'):
            print("Calm calibration signal triggered.")
            send_calm_calibration_signal(1)
            time.sleep(0.5)

        # Stressed calibration 1: Press '#'
        elif keyboard.is_pressed('#'):
            print("Stressed calibration 1 signal triggered.")
            send_stressed_calibration_signal_1(1)
            time.sleep(0.5)
            
        # Stressed calibration 2: Press '$'
        elif keyboard.is_pressed('$'):
            print("Stressed calibration 2 signal triggered.")
            send_stressed_calibration_signal_2(1)
            time.sleep(0.5)
            
        # Stressed calibration 3: Press '%'
        elif keyboard.is_pressed('%'):
            print("Stressed calibration 3 signal triggered.")
            send_stressed_calibration_signal_3(1)
            time.sleep(0.5)
            
        # Data analysis: Press '^'
        elif keyboard.is_pressed('^'):
            print("Data analysis signal triggered.")
            send_data_analysis_signal(1)
            time.sleep(0.5)

        # Ability: Press '&'
        elif keyboard.is_pressed('&'):
            with value_lock:
                if binary_value == 0:
                    binary_value = 1
                    print("Ability value set to 1, starting 45-second timer.")
                    send_ability_value(binary_value)
                    threading.Thread(target=reset_ability_value_after_delay, daemon=True).start()
            time.sleep(0.5)

if __name__ == "__main__":
    print("Press '!' for testing, '@' for calm calibration, '#' for stressed calibration 1, '$' for stressed calibration 2,") 
    print("'%' for stressed calibration 3, '^' for data analysis, and '&' for ability signal.")
    check_for_keypress()

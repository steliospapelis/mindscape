import requests
import keyboard
import time
import threading

# Global variable for ability signal value
binary_value = 0
value_lock = threading.Lock()

# Function to send calm calibration signal
def send_calm_calibration_signal(value):
    try:
        response = requests.post('http://127.0.0.1:5000/calm_calibration', json={"value": value})
        if response.status_code == 200:
            print(f"Calm calibration signal sent with value: {value}")
        else:
            print(f"Failed to send calm calibration signal: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")

# Function to send anxious calibration signal
def send_anxious_calibration_signal(value):
    try:
        response = requests.post('http://127.0.0.1:5000/anxious_calibration', json={"value": value})
        if response.status_code == 200:
            print(f"Anxious calibration signal sent with value: {value}")
        else:
            print(f"Failed to send anxious calibration signal: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")

# Function to send ability signal
def send_ability_value(value):
    try:
        response = requests.post('http://127.0.0.1:5000/ability', json={"value": value})
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
        # Calm calibration: Press '$'
        if keyboard.is_pressed('$'):
            print("Calm calibration signal triggered.")
            send_calm_calibration_signal(1)
            time.sleep(0.5)

        # Anxious calibration: Press '%'
        elif keyboard.is_pressed('%'):
            print("Anxious calibration signal triggered.")
            send_anxious_calibration_signal(1)
            time.sleep(0.5)

        # Ability: Press '^'
        elif keyboard.is_pressed('^'):
            with value_lock:
                if binary_value == 0:
                    binary_value = 1
                    print("Ability value set to 1, starting 45-second timer.")
                    send_ability_value(binary_value)
                    threading.Thread(target=reset_ability_value_after_delay, daemon=True).start()
            time.sleep(0.5)

if __name__ == "__main__":
    print("Press '$' for calm calibration, '%' for anxious calibration, and '^' for ability signal.")
    check_for_keypress()

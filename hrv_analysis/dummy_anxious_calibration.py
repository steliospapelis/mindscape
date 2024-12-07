import requests
import keyboard
import time

def send_anxious_calibration_signal(value):
    """
    Send the anxious calibration signal to the server.
    """
    try:
        # Send the signal to the server
        response = requests.post('http://127.0.0.1:5000/anxious_calibration', json={"value": value})
        if response.status_code == 200:
            print(f"Anxious calibration signal sent with value: {value}")
        else:
            print(f"Failed to send anxious calibration signal: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")

def check_for_keypress():
    """
    Check for the '%' key press to send the anxious calibration signal.
    """
    while True:
        if keyboard.is_pressed('%'):  # Press '%' for anxious calibration
            print("Anxious calibration signal triggered.")
            send_anxious_calibration_signal(1)  # Send the signal to the server
            time.sleep(0.5)  # Prevent multiple triggers for the same press

if __name__ == "__main__":
    print("Press '%' to trigger the anxious calibration signal.")
    check_for_keypress()

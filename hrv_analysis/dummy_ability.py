import time
import requests
import keyboard
import threading

# Global variable to store the value (0 or 1)
binary_value = 0

# Lock to ensure thread-safe access to binary_value
value_lock = threading.Lock()

def send_value(value):
    """Send the current binary value to localhost:5000/ability."""
    try:
        # Send the value to the server
        response = requests.post('http://127.0.0.1:5000/ability', json={"value": value})
        if response.status_code == 200:
            print(f"Sent value: {value}")
        else:
            print(f"Failed to send value: {response.status_code}")
    except requests.ConnectionError:
        print("Connection error. Is the server running?")

def reset_value_after_delay():
    """Reset the binary value to 0 after 45 seconds."""
    global binary_value
    time.sleep(45)
    with value_lock:
        binary_value = 0
        print("Value reset to 0 after 45 seconds.")
        send_value(binary_value)  # Send the updated value to the server

def check_for_keypress():
    """Check for 'a' key press and update the binary value."""
    global binary_value
    while True:
        if keyboard.is_pressed('^'):
            with value_lock:
                if binary_value == 0:
                    binary_value = 1
                    print("Value set to 1, starting 45-second timer.")
                    send_value(binary_value)  # Send the value to the server when it changes
                    # Start a thread to reset the value after 45 seconds
                    threading.Thread(target=reset_value_after_delay, daemon=True).start()
            # Prevent multiple presses of 'a' while waiting
            time.sleep(0.5)

if __name__ == "__main__":
    # Start checking for keypresses in the main thread
    check_for_keypress()

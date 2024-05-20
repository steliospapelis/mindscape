from pythonosc import dispatcher
from pythonosc import osc_server
import threading
import queue
import os
import keyboard 
from data_processor import data_processor 

# Global queue for sharing data between threads
data_queue = queue.Queue()
file_lock = threading.Lock()

def ppg_green_handler(address, *args):
    '''if "PPG:GRN" in address:
        for arg in args:
            data_queue.put(arg)  # Put the PPG values into the queue
        #print(f"Received and queued PPG Green data: {args}")'''
        
    with file_lock:
        with open("live_output.txt", "a") as file:
            file.write(f"{address}: {args}\n")
            if "PPG:GRN" in address:
                for arg in args:
                    data_queue.put(arg)  # Put the PPG values into the queue
        #print(f"Received and queued PPG Green data: {args}")

#the data_processor function was originally implemented in this file but it is now implemented in a different file
#this is a simplier version that just prints the data
'''def data_processor(stop_event):
    while not stop_event.is_set() or not data_queue.empty():
        try:
            # Get data from the queue
            data = data_queue.get(timeout=0.1)  # Adjust timeout as needed
            # Process the data (in this case, just printing)
            print(f"Processing data: {data}")
        except queue.Empty:
            continue  # Continue if no data is available'''

def run_osc_listener(ip, port, stop_event):
    disp = dispatcher.Dispatcher()
    disp.set_default_handler(ppg_green_handler)
    server = osc_server.BlockingOSCUDPServer((ip, port), disp)
    print(f"Listening on {ip}:{port}")
    
    while not stop_event.is_set():
        server.handle_request()

#press Enter to stop the listener
def exit_listener(stop_event):
    print("Press 'Enter' to stop listening...")
    while not stop_event.is_set():
        if keyboard.is_pressed('enter'):
            stop_event.set()
            print("Stop command received, shutting down...")

def main():
    ip = '127.0.0.1'
    port = 12345
    stop_event = threading.Event()

    # Run the OSC server in a separate thread
    listener_thread = threading.Thread(target=run_osc_listener, args=(ip, port, stop_event))
    listener_thread.start()

    # Run the data processor in a separate thread (function is found i a different file named )
    data_thread = threading.Thread(target=data_processor, args=(data_queue, stop_event))
    data_thread.start()

    # Run the exit listener in a separate thread
    exit_thread = threading.Thread(target=exit_listener, args=(stop_event,))
    exit_thread.start()

    # Wait for both threads to finish
    listener_thread.join()
    data_thread.join()
    exit_thread.join()
    print("Server has been stopped.")

if __name__ == "__main__":
    main()

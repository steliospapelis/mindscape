from pythonosc import udp_client
import time

def send_data_from_file(filepath, ip, port, delay):
    # Initialize the OSC client
    client = udp_client.SimpleUDPClient(ip, port)
    
    # Read data from the file
    with open(filepath, 'r') as file:
        for line in file:
            line = line.strip()
            if line:
                # Extract the OSC address and the data
                address_start = line.find("/EmotiBit/")
                if address_start != -1:
                    # Address is extended to include the type or color after the main type
                    address_end = line.find(":", address_start)
                    if address_end != -1:
                        # Look for the second colon to include the subtype or color
                        data_start = line.find("(", address_end)
                        if data_start != -1:
                            address = line[address_start:line.rfind(":", 0, data_start)]
                            try:
                                # Convert the tuple in the string to an actual tuple
                                data = eval(line[data_start:])
                                client.send_message(address, data)
                                print(f"Sent data to {address} with message: {data}")
                            except SyntaxError as e:
                                print(f"Error parsing data: {line[data_start:]}", e)
                time.sleep(delay)  # Delay to simulate real-time data

if __name__ == "__main__":
    #filepath = 'C:/Users/nikol/Desktop/University/8th_semester/biomedical_technologies/HRV Analysis/real_time_data/output_all_measurements.txt'  # Path to your file with data
    filepath = './real_time_data/output_all_measurements.txt'
    ip = '127.0.0.1' 
    port = 12345  
    delay = 0.007  # Delay in seconds between messages
    
    #14 measurement between two ppg green messages
    #2.5 values of ppg per message on average
    #we want a ppg value every 0.04sec so 2.5 x (0.04 / 14) = 0.00714

    send_data_from_file(filepath, ip, port, delay)

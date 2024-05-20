from flask import Flask, jsonify
import threading
import time
import random

app = Flask(__name__)
computed_value = 0

@app.route('/get_value', methods=['GET'])
def get_value():
    global computed_value
    return jsonify({'value': computed_value})

def update_computed_value():
    global computed_value
    while True:
        # Your sensor data reading and computation logic here
        # This is just a dummy example
        time.sleep(1)  # Simulating computation delay
        computed_value = 1 if computed_value == 0 else 0  # Toggle between 0 and 1
        #computed_value = random.sample([0, 1])

if __name__ == '__main__':
    # Start the computation in a separate thread
    threading.Thread(target=update_computed_value).start()
    # Run the Flask server
    app.run(host='0.0.0.0', port=5000)

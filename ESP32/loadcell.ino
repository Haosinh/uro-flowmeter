#include <Q2HX711.h>
#include "KalmanFilter.h" // Include the Kalman filter header file
#define I2C_SCL 9
#define I2C_SDA 8
#define BUZZER 38
#define BTN_RESET 37
#define HX711_DOUT 15
#define HX711_SCK 16
#define SENSOR_PROMIXY 7
#define RGB_LED 6
#define ADC_BATERRY 4
//#define CD42_KEY 5
const byte hx711_data_pin = HX711_DOUT;
const byte hx711_clock_pin = HX711_SCK;
long zero = 0;
float initial_weight = 0; // Initial average weight
long kl;
const float scale_factor = 3.8; // Set the scale factor here

Q2HX711 hx711(hx711_data_pin, hx711_clock_pin);

// Kalman filter instance
KalmanFilter kalman(0.1, 0.5);

const int numReadings = 10; // Number of readings to average
float readings[numReadings]; // The readings array (float for filtered values)
int currentIndex = 0; // The index of the current reading
float total = 0; // The running total (float for filtered values)
float average = 0; // The averaged value (float for filtered values)

void setup() {
  Serial.begin(112500);
  Serial.println("Calibration");
  for (int i = 0; i < 100; i++) {
    zero += hx711.read();
  }
  zero /= 100;
  Serial.println("Calibration is succeeded");
  delay(500);
  
  // Initialize the readings array
  for (int i = 0; i < numReadings; i++) {
    readings[i] = 0;
  }

  // Calculate initial average weight
  long initial_total = 0;
  for (int i = 0; i < 100; i++) {
    long raw_value = hx711.read();
    long calibrated_value = raw_value - zero;
    float filtered_value = kalman.filter(calibrated_value);
    initial_total += filtered_value;
    delay(10); // Small delay to allow stable reading
  }
  initial_weight = initial_total / 100.0;
  Serial.print("Initial Weight: ");
  Serial.println(initial_weight / scale_factor);
}

void loop() {
  // Read the raw sensor value
  long raw_value = hx711.read();
  
  // Calibrate the raw value
  long calibrated_value = raw_value - zero;
  
  // Apply Kalman filter
  float filtered_value = kalman.filter(calibrated_value);
  
  // Add the new reading to the total and subtract the oldest reading
  total = total - readings[currentIndex];
  readings[currentIndex] = filtered_value;
  total = total + readings[currentIndex];
  
  // Move to the next index
  currentIndex = (currentIndex + 1) % numReadings;
  
  // Calculate the average
  average = total / numReadings;
  
  // Adjust the average to start from zero
  float adjusted_average = average - initial_weight;
  
  // Convert to weight using scale factor
  kl = adjusted_average / scale_factor;
  
  // Get the current time in milliseconds
  unsigned long current_time = millis();
  
  // Output the time and weight to the serial monitor
  Serial.print("Time (ms): ");
  Serial.print(current_time);
  Serial.print(" Weight: ");
  Serial.println(kl);
}
